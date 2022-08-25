using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{
    [FriendClass(typeof(RouterServiceInnerComponent))]
    public static class RouterServiceInnerComponentSystem
    {
        [ObjectSystem]
        public class RouterServiceInnerComponentAwakeSystem : AwakeSystem<RouterServiceInnerComponent, IPEndPoint>
        {
            public override void Awake(RouterServiceInnerComponent self, IPEndPoint address)
            {
                self.Awake(address);
            }
        }
        [ObjectSystem]
        public class RouterServiceInnerComponentAwakeSystem2 : AwakeSystem<RouterServiceInnerComponent>
        {
            public override void Awake(RouterServiceInnerComponent self)
            {
                self.Awake(new IPEndPoint(IPAddress.Any, 0));
            }
        }
        [ObjectSystem]
        public class RouterServiceInnerComponentUpdateSystem : UpdateSystem<RouterServiceInnerComponent>
        {
            public override void Update(RouterServiceInnerComponent self)
            {
                self.Update();
            }
        }
        [ObjectSystem]
        public class RouterServiceInnerComponentDestroySystem : DestroySystem<RouterServiceInnerComponent>
        {
            public override void Destroy(RouterServiceInnerComponent self)
            {
                self.Destroy();
            }
        }
        
        public static void Awake(this RouterServiceInnerComponent self,IPEndPoint ipEndPoint)
        {
            self.StartTime = TimeHelper.ClientNow();
            self.CurrTimeSecond = TimeHelper.ClientNowSeconds();
            self.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                self.socket.SendBufferSize = Kcp.OneM * 64;
                self.socket.ReceiveBufferSize = Kcp.OneM * 64;
            }

            self.socket.Bind(ipEndPoint);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                self.socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
            }
            self.OuterRouterService = self.Domain.GetComponent<RouterServiceComponent>();
        }
        
        public static void Destroy(this RouterServiceInnerComponent self)
        {
            self.socket.Close();
            self.socket = null;
        }

        private static IPEndPoint CloneAddress(this RouterServiceInnerComponent self)
        {
            IPEndPoint ip = (IPEndPoint)self.ipEndPoint;
            return new IPEndPoint(ip.Address, ip.Port);
        }
        private static void Recv(this RouterServiceInnerComponent self)
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

                // accept
                byte flag = self.cache[0];

                // conn从100开始，如果为1，2，3则是特殊包
                uint remoteConn = 0;
                uint localConn = 0;
                ulong remotelocalConn = 0;
                try
                {
                    switch (flag)
                    {
                        //此处映射gate过来的消息发给哪个客户端
                        case KcpProtocalType.ACK: // accept
                        case KcpProtocalType.RouterReconnectAck:
                            if (messageLength < 9)
                            {
                                break;
                            }
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            localConn = BitConverter.ToUInt32(self.cache, 5);
                            remotelocalConn = ((ulong)localConn << 32) | remoteConn;
                            if (self.OuterRouterService.GetACK(localConn, remoteConn))
                            {
                                self.OuterRouterService.SendToClient(remotelocalConn, messageLength, self.cache);
                            }
                            break;
                        case KcpProtocalType.MSG:
                            if (messageLength < 9)
                            {
                                break;
                            }
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            localConn = BitConverter.ToUInt32(self.cache, 5);
                            remotelocalConn = ((ulong)localConn << 32) | remoteConn;
                            if (!self.OuterRouterService.SendToClient(remotelocalConn,messageLength,self.cache))
                            {
                                //todo: 这里发送失败的话应该主动给服务端发一条FIN消息.免得服务端继续发消息
                                Log.Debug("Router MSG error:not found client:" + remotelocalConn);
                            }
                            break;
                        case KcpProtocalType.FIN: // 断开
                            if (messageLength < 9)
                            {
                                break;
                            }
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            localConn = BitConverter.ToUInt32(self.cache, 5);
                            remotelocalConn = ((ulong)localConn << 32) | remoteConn;
                            self.OuterRouterService.SendToClient(remotelocalConn, messageLength, self.cache);
                            self.OuterRouterService.RemoveClientAddress(remotelocalConn);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"RouterService error: {flag} {remoteConn} {localConn}\n{e}");
                }
            }
        }

        public static void SendToGate(this RouterServiceInnerComponent self,byte[] buffer, int length, IPEndPoint inneraddress)
        {
            self.socket.SendTo(buffer, 0, length, SocketFlags.None, inneraddress);

        }

        public static void Update(this RouterServiceInnerComponent self)
        {
            self.Recv();

        }
    }
}