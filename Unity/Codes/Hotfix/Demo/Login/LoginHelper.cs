using System;


namespace ET
{
    [FriendClass(typeof(RouterDataComponent))]
    [FriendClass(typeof(GetRouterComponent))]
    public static class LoginHelper
    {
        public static async ETTask Login(Scene zoneScene, string address, string account, string password,Action onError=null)
        {
            try
            {
                // 创建一个ETModel层的Session
                R2C_Login r2CLogin;
                Session session = null;
                try
                {
                    session = zoneScene.GetComponent<NetKcpComponent>().Create(NetworkHelper.ToIPEndPoint(address));
                    {
                        r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = account, Password = password });
                    }
                }
                finally
                {
                    session?.Dispose();
                }

                long channelId = RandomHelper.RandInt64();
                var routercomponent = zoneScene.AddComponent<GetRouterComponent, long, long>(r2CLogin.GateId, channelId);
                string routerAddress = await routercomponent.Tcs;
                if (routerAddress == "")
                {
                    zoneScene.RemoveComponent<GetRouterComponent>();
                    throw new Exception("routerAddress 失败");
                }
                Log.Debug("routerAddress 获取成功:" + routerAddress);
                zoneScene.RemoveComponent<GetRouterComponent>();
                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = zoneScene.GetComponent<NetKcpComponent>().Create(channelId, NetworkHelper.ToIPEndPoint(routerAddress));
                gateSession.AddComponent<RouterDataComponent>().Gateid = r2CLogin.GateId;

                gateSession.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().Session = gateSession;
				
                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId});

                Log.Debug("登陆gate成功!");

                await Game.EventSystem.PublishAsync(new EventType.LoginFinish() {ZoneScene = zoneScene,Account = account});
            }
            catch (Exception e)
            {
                onError?.Invoke();
                Log.Error(e);
            }
        } 
    }
}