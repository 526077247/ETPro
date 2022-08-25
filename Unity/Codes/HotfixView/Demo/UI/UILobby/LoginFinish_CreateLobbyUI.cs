

namespace ET
{
	public class LoginFinish_CreateLobbyUI: AEventAsync<EventType.LoginFinish>
	{
		protected override async ETTask Run(EventType.LoginFinish args)
		{
			args.ZoneScene.GetComponent<PlayerComponent>().Account = args.Account;
			//todo: 服务端下发数据，包括引导进度
			GuidanceComponent.Instance.CheckGroupStart();
			await UIManagerComponent.Instance.OpenWindow<UILobbyView,Scene>(UILobbyView.PrefabPath,args.ZoneScene);
		}
	}
}
