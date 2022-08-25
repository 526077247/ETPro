using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
namespace ET
{
    /// <summary>
    /// 在session上挂载的保存路由信息的组件.切换路由用
    /// </summary>
    [ComponentOf(typeof(Session))]
    public class RouterDataComponent : Entity,IAwake
    {
        public long Gateid;
    }
    [FriendClass(typeof(SwitchRouterComponent))]
    [FriendClass(typeof(RouterDataComponent))]
    [FriendClass(typeof(GetRouterComponent))]
    [FriendClass(typeof(Session))]
    public class SwitchRouterComponentAwakeSystem : AwakeSystem<SwitchRouterComponent>
    {
        public override void Awake(SwitchRouterComponent self)
        {
            ChangeRouter(self).Coroutine();
        }
        
        public async ETTask ChangeRouter(SwitchRouterComponent self)
        {
            Session session = self.GetParent<Session>();
            session.RemoveComponent<SessionIdleCheckerComponent>();
            var gateid = session.GetComponent<RouterDataComponent>().Gateid;
            var routercomponent = session.AddComponent<GetRouterComponent, long, long>(gateid, session.Id);
            string routerAddress = await routercomponent.Tcs;
            session.RemoveComponent<GetRouterComponent>();
            if (routerAddress == "")
            {
                session.Dispose();
                return;
            }
            (session.AService as KService).ChangeAddress(session.Id, NetworkHelper.ToIPEndPoint(routerAddress));
            session.LastRecvTime = TimeHelper.ClientNow();
            session.AddComponent<SessionIdleCheckerComponent,int>(NetThreadComponent.checkInteral);
            session.RemoveComponent<SwitchRouterComponent>();
        }
    }
    /// <summary>
    /// 切换路由组件
    /// </summary>
    [ComponentOf(typeof(Session))]
    public class SwitchRouterComponent : Entity,IAwake
    {
        
    }

}
