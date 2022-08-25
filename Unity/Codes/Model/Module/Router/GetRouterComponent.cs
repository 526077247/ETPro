using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
namespace ET
{
    /// <summary>
    /// 初始获取路由组件
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class GetRouterComponent : Entity,IAwake<long,long>,IUpdate,IDestroy
    {
        public int ChangeTimes;
        public Socket socket;
        public readonly byte[] cache = new byte[8192];
        public EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public ETCancellationToken CancellationToken;
        public ETTask<string> Tcs;
        public bool IsChangingRouter;
        
    }

}
