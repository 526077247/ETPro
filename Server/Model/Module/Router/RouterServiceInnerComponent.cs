using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ET
{

    
    public class RouterServiceInnerData
    {
        public string Clientaddress;
        public uint MsgTime;

        public RouterServiceInnerData(string clientaddress, uint msgTime)
        {
            Clientaddress = clientaddress;
            MsgTime = msgTime;
        }
    }
    /// <summary>
    /// 软路由服务组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public sealed class RouterServiceInnerComponent : Entity,IAwake,IAwake<IPEndPoint>,IDestroy,IUpdate
    {
        // RouterService创建的时间
        public long StartTime;
        public RouterServiceComponent OuterRouterService;

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
        
        public readonly byte[] cache = new byte[8192];
        public EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

    }
}