using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace ET
{
    [FriendClass(typeof(RouterServiceComponent))]
    public static class RouterServiceComponentSystem
    {
        [ObjectSystem]
        public class RouterServiceComponentAwakeSystem : AwakeSystem<RouterServiceComponent, IPEndPoint>
        {
            public override void Awake(RouterServiceComponent self, IPEndPoint address)
            {
                self.Awake(address);
            }
        }
        [ObjectSystem]
        public class RouterServiceComponentUpdateSystem : UpdateSystem<RouterServiceComponent>
        {
            public override void Update(RouterServiceComponent self)
            {
                self.Update();
            }
        }
        [ObjectSystem]
        public class RouterServiceComponentDestroySystem : DestroySystem<RouterServiceComponent>
        {
            public override void Destroy(RouterServiceComponent self)
            {
                self.Destroy();
            }
        }
        
        public static void Awake(this RouterServiceComponent self,IPEndPoint ipEndPoint)
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
        }
        
        public static void Destroy(this RouterServiceComponent self)
        {
            self.socket.Close();
            self.socket = null;
        }

        private static IPEndPoint CloneAddress(this RouterServiceComponent self)
        {
            IPEndPoint ip = (IPEndPoint)self.ipEndPoint;
            return new IPEndPoint(ip.Address, ip.Port);
        }
        private static void Recv(this RouterServiceComponent self)
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
                        case KcpProtocalType.RouterSYN:
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            int gateid = BitConverter.ToInt32(self.cache, 5);
                            if (StartSceneConfigCategory.Instance.Contain(gateid))
                            {
                                var conf = StartSceneConfigCategory.Instance.Get(gateid);
                                if (conf.SceneType == "Gate")
                                {
                                    if (self.waitConnectChannels.TryGetValue(remoteConn, out var ipendpoint))
                                    {
                                        //如果是自己重复发.就再返回,否则直接抛弃
                                        if (ipendpoint.ClientEndPoint.ToString() == self.ipEndPoint.ToString())
                                        {
                                            byte[] newbuffer = self.cache;
                                            newbuffer.WriteTo(0, KcpProtocalType.RouterACK);
                                            self.socket.SendTo(newbuffer, 0, 1, SocketFlags.None, self.ipEndPoint);
                                            Log.Debug("RouterSYN repeated:" + self.ipEndPoint.ToString());
                                        }
                                        break;
                                    }
                                    //这是第一次添加
                                    else 
                                    {
                                        var inneraddress = conf.InnerIPOutPort;
                                        self.waitConnectChannels[remoteConn]= new RouterIPEndPoint(self.CloneAddress(), inneraddress, self.TimeNow);
                                        byte[] newbuffer = self.cache;
                                        newbuffer.WriteTo(0, KcpProtocalType.RouterACK);
                                        self.socket.SendTo(newbuffer, 0, 1, SocketFlags.None, self.ipEndPoint);
                                        Log.Debug("RouterSYN 成功:" + self.ipEndPoint.ToString());
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                Log.Debug("Router SYN error:not found gate:" + gateid.ToString());
                                break;
                            }
                            break;
                        case KcpProtocalType.SYN: // accept
                        case KcpProtocalType.RouterReconnect:
                            if (messageLength < 9)
                            {
                                break;
                            }
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            if (self.waitConnectChannels.TryGetValue(remoteConn,out var routerIPEnd))
                            {
                                //syn的时候更新客户端地址.有可能是不同的socket发来的
                                Log.Debug("SYN 之前地址:" + routerIPEnd.ClientEndPoint.ToString());
                                routerIPEnd.ClientEndPoint = self.CloneAddress();
                                self.Domain.GetComponent<RouterServiceInnerComponent>().SendToGate(self.cache, 9, routerIPEnd.TargetEndPoint);
                                Log.Debug("SYN 地址变更成功:" + self.ipEndPoint.ToString());
                            }
                            break;
                        case KcpProtocalType.MSG:
                            if (messageLength < 9)
                            {
                                break;
                            }
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            localConn = BitConverter.ToUInt32(self.cache, 5);
                            remotelocalConn = ((ulong)remoteConn << 32) | localConn;
                            if (self.clientsAddress.TryGetValue(remotelocalConn, out var realTargetAddress))
                            {
                                self.Domain.GetComponent<RouterServiceInnerComponent>().SendToGate(self.cache, messageLength, realTargetAddress.TargetEndPoint);
                                realTargetAddress.MsgTime = self.TimeNow;
                            }
                            else
                            {
                                Log.Debug("Router MSG error:not found gateaddress:" + self.ipEndPoint.ToString());
                                break;
                            }
                            break;
                        case KcpProtocalType.FIN: // 断开
                            if (messageLength < 9)
                            {
                                break;
                            }
                            remoteConn = BitConverter.ToUInt32(self.cache, 1);
                            localConn = BitConverter.ToUInt32(self.cache, 5);
                            remotelocalConn = ((ulong)remoteConn << 32) | localConn;
                            if (self.clientsAddress.TryGetValue(remotelocalConn, out var finTargetAddress))
                            {
                                self.Domain.GetComponent<RouterServiceInnerComponent>().SendToGate(self.cache, messageLength, finTargetAddress.TargetEndPoint);
                                self.RemoveClientAddress(remotelocalConn);
                            }
                            else
                            {
                                Log.Debug("Router MSG FIN:not found gateaddress:" + self.ipEndPoint.ToString());
                                break;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"RouterService error: {flag} {remoteConn} {localConn}\n{e}");
                }
            }
        }

        public static void RemoveClientAddress(this RouterServiceComponent self,ulong remotelocalConn)
        {
            self.clientsAddress.Remove(remotelocalConn);
        }
        /// <summary>
        /// 获取ack或者切换路由成功时,移除连接信息.加入完整的路由信息
        /// </summary>
        /// <param name="remoteConn"></param>
        /// <param name="localConn"></param>
        /// <returns></returns>
        public static bool GetACK(this RouterServiceComponent self,uint remoteConn, uint localConn)
        {
            Log.Debug($"GetACK:{localConn} {remoteConn}");
            if (self.waitConnectChannels.TryGetValue(remoteConn,out var routerIPEndPoint))
            {
                ulong remotelocal = ((ulong)remoteConn << 32) | localConn;
                self.clientsAddress[remotelocal] = routerIPEndPoint;
                self.waitConnectChannels.Remove(remoteConn);
                return true;
            }
            return false;
        }

        public static bool SendToClient(this RouterServiceComponent self,ulong remotelocalConn, int messageLength, byte[] cache)
        {
            if (self.clientsAddress.TryGetValue(remotelocalConn, out var iPEndPointEntity))
            {
                self.socket.SendTo(cache, 0, messageLength, SocketFlags.None, iPEndPointEntity.ClientEndPoint);
                return true;
            }
            else
                return false;
        }

        public static void Update(this RouterServiceComponent self)
        {
            self.Recv();
            var nowtime = TimeHelper.ClientNowSeconds();
            if (self.CurrTimeSecond != nowtime)
            {
                self.CurrTimeSecond = nowtime;
                if (self.CurrTimeSecond % 3 == 0)
                {
                    self.RemoveConnectTimeoutIds();
                }
            }
        }
        private static void RemoveConnectTimeoutIds(this RouterServiceComponent self)
        {
            self.waitRemoveAddress.Clear();
            foreach (var clientaddress in self.clientsAddress.Keys)
            {
                if (self.TimeNow > self.clientsAddress[clientaddress].MsgTime + 30000)
                {
                    self.waitRemoveAddress.Add(clientaddress);
                }
            }
            foreach (var clientkey in self.waitRemoveAddress)
            {
                self.clientsAddress.Remove(clientkey);
            }
            if (self.clientsAddress.Count > 1000)
            {
                Log.Debug("clientsAddress.Count要报警了!:" + self.clientsAddress.Count);
            }
            //下面清理半连接
            self.waitRemoveConnectChannels.Clear();
            foreach (var channel in self.waitConnectChannels.Keys)
            {
                if (self.TimeNow > self.waitConnectChannels[channel].MsgTime + 10000)
                {
                    self.waitRemoveConnectChannels.Add(channel);
                }
            }
            foreach (var channelkey in self.waitRemoveConnectChannels)
            {
                self.waitConnectChannels.Remove(channelkey);
            }
            if (self.waitConnectChannels.Count > 1000)
            {
                Log.Debug("waitConnectChannels.Count要报警了!:" + self.waitConnectChannels.Count);
            }
        }
    }
}