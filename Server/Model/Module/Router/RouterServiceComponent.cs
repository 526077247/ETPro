using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{

    public class RouterIPEndPoint
    {
        public IPEndPoint ClientEndPoint;

        public IPEndPoint TargetEndPoint;
        public uint MsgTime;
        public RouterIPEndPoint(IPEndPoint clientEndPoint,IPEndPoint targetEndPoint, uint msgTime)
        {
            ClientEndPoint = clientEndPoint;
            TargetEndPoint = targetEndPoint;
            MsgTime = msgTime;
        }
    }

    /// <summary>
    /// 软路由服务组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public sealed class RouterServiceComponent : Entity,IAwake<IPEndPoint>,IUpdate,IDestroy
    {
        // RouterService创建的时间
        public long StartTime;

        // 当前时间 - RouterService创建的时间, 线程安全
        public uint TimeNow
        {
            get
            {
                return (uint)(TimeHelper.ClientNow() - this.StartTime);
            }
        }

        public Socket socket;
        public long CurrTimeSecond;


        
        public readonly Dictionary<uint, RouterIPEndPoint> waitConnectChannels = new Dictionary<uint, RouterIPEndPoint>();
        public readonly List<uint> waitRemoveConnectChannels = new List<uint>();
        public readonly Dictionary<ulong, RouterIPEndPoint> clientsAddress = new Dictionary<ulong, RouterIPEndPoint>();
        public readonly List<ulong> waitRemoveAddress = new List<ulong>();

        public readonly byte[] cache = new byte[8192];
        public EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        
    }

}