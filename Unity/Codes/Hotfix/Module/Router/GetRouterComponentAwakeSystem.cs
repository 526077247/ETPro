using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace ET
{
    [ObjectSystem]
    public class GetRouterComponentSynAwakeSystem : AwakeSystem<GetRouterComponent, long, long>
    {
        public override void Awake(GetRouterComponent self, long gateid, long channelid)
        {
            self.ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
            self.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            // 作为客户端不需要修改发送跟接收缓冲区大小
            self.socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                self.socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
            }
            self.ChangeTimes = 3;
            SynAsync(self, gateid, channelid).Coroutine();
        }
        /// <summary>
        /// 应从cdn获取.此处临时写假的
        /// </summary>
        /// <returns></returns>
        static async ETTask<string[]> GetRouterListFake()
        {
#if !NOT_UNITY
            return await HttpManager.Instance.HttpGetResult<string[]>(ServerConfigComponent.Instance.GetCurConfig().RouterListUrl + "/router.list");
#else
            await ETTask.CompletedTask;
            return new string[]{"172.22.213.58:10007", "172.22.213.58:10008", "172.22.213.58:10009", };
#endif
        }
        private static async ETTask SynAsync(GetRouterComponent self, long gateid, long channelid)
        {
            self.CancellationToken = new ETCancellationToken();
            self.Tcs = ETTask<string>.Create();
            //value是对应gate的scene.
            var insid = new InstanceIdStruct(gateid);
            uint localConn = (uint)((ulong)channelid & uint.MaxValue);
            var routerlist = await GetRouterListFake();
            if (routerlist == null)
            {
                self.Tcs?.SetResult("");
                self.Tcs = null;
                Log.Error("从cdn获取路由失败");
                return;
            }
            Log.Debug("路由数量:" + routerlist.Length.ToString());
            Log.Debug("gateid:" + insid.Value.ToString());
            byte[] buffer = self.cache;
            buffer.WriteTo(0, KcpProtocalType.RouterSYN);
            buffer.WriteTo(1, localConn);
            buffer.WriteTo(5, insid.Value);
            for (int i = 0; i < self.ChangeTimes; i++)
            {
                string router = routerlist.RandomArray();
                Log.Debug("router:" + router);
                self.socket.SendTo(buffer, 0, 9, SocketFlags.None, NetworkHelper.ToIPEndPoint(router));
                var returnbool = await TimerComponent.Instance.WaitAsync(300, self.CancellationToken);
                if (returnbool == false)
                {
                    Log.Debug("提前取消了.可能连接上了");
                    return;
                }
            }
            await TimerComponent.Instance.WaitAsync(1300, self.CancellationToken);
            self.Tcs?.SetResult("");
            self.Tcs = null;
            Log.Debug("三次失败.获取路由失败");
        }
    }
    [ObjectSystem]
    public class GetRouterComponentUpdateSystem : UpdateSystem<GetRouterComponent>
    {
        public override void Update(GetRouterComponent self)
        {
            self.Recv();
        }
    }
    [ObjectSystem]
    public class GetRouterComponentDestroySystem : DestroySystem<GetRouterComponent>
    {
        public override void Destroy(GetRouterComponent self)
        {
            self.CancellationToken?.Cancel();
            self.CancellationToken = null;
            self.ChangeTimes = 0;
            self.socket.Dispose();
            self.socket = null;
            self.ipEndPoint = null;
            self.Tcs = null;
        }
    }
    [FriendClass(typeof(GetRouterComponent))]
    public static class GetRouterComponentSystem
    {
        public static void Recv(this GetRouterComponent self)
        {
            if (self.socket == null)
            {
                return;
            }

            while (self.socket != null && self.socket.Available > 0)
            {
                int messageLength = self.socket.ReceiveFrom(self.cache, ref self.ipEndPoint);

                // 长度小于1，不是正常的消息
                if (messageLength < 1)
                {
                    continue;
                }
                byte flag = self.cache[0];
                try
                {
                    switch (flag)
                    {
                        case KcpProtocalType.RouterACK:
                            Log.Debug("RouterACK:"+ self.ipEndPoint.ToString());
                            self.Tcs?.SetResult(self.ipEndPoint.ToString());
                            self.Tcs = null;
                            self.CancellationToken?.Cancel();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"kservice error: {flag}\n{e}");
                }
            }
        }
    }
}
