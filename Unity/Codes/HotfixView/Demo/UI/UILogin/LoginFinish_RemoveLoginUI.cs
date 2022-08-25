

namespace ET
{
	public class LoginFinish_RemoveLoginUI: AEventAsync<EventType.LoginFinish>
	{
		protected override async ETTask Run(EventType.LoginFinish args)
		{
			await UIManagerComponent.Instance.CloseWindow<UILoginView>();
		}
	}
}
