using System;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UIBaseMessageWindow))]
	public class UIBaseMessageWindowOnCreateSystem: OnCreateSystem<UIBaseMessageWindow>
	{
		public override void OnCreate(UIBaseMessageWindow self)
		{
			self.allText = "";
			self.showLen = 0;
			self.Text = self.AddUIComponent<UITextmesh>("Scroll View/Viewport/Content/TextSpace");
			self.UIPointerClick = self.AddUIComponent<UIPointerClick>("Scroll View");
			self.UIPointerClick.SetOnClick(()=>
			{
				self.OnCancel();
			});
			self.CancelAction = () =>
			{
				self.OnRunningStateEnd();
			};
		}
	}
	[UISystem]
	[FriendClass(typeof(UIBaseMessageWindow))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public class UIBaseMessageWindowOnEnableSystem: OnEnableSystem<UIBaseMessageWindow,float,long>
	{
		public override void OnEnable(UIBaseMessageWindow self,float t,long v)
		{
			self.speed = t;
			self.waitTime = v;
			if (GalGameEngineComponent.Instance.CancelToken == null)
				GalGameEngineComponent.Instance.CancelToken = new ETCancellationToken();
			GalGameEngineComponent.Instance.CancelToken.Add(self.CancelAction);
		}
	}
	[UISystem]
	[FriendClass(typeof(UIBaseMessageWindow))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public class UIBaseMessageWindowOnDisableSystem: OnDisableSystem<UIBaseMessageWindow>
	{
		public override void OnDisable(UIBaseMessageWindow self)
		{
			GalGameEngineComponent.Instance.CancelToken?.Remove(self.CancelAction);
		}
	}
	[FriendClass(typeof(UIBaseMessageWindow))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public static class UIBaseMessageWindowSystem 
	{

		public static async ETTask SetContent(this UIBaseMessageWindow self,string key, bool play = true, bool clear = true)
		{
			if (string.IsNullOrEmpty(key)) return;
			I18NComponent.Instance.I18NTryGetText(key,out string text);
			var baseLen = self.showLen;
			self.allText += text;
			self.Text.SetText(self.allText);
			self.showLen = self.allText.Length;
			self.token = new ETCancellationToken();
			self.isPlay = play;
			for (int i = baseLen + 1; i <= self.showLen && self.isPlay && self.speed>0; i++)
			{
				self.Text.SetMaxVisibleCharacters(i);
				await TimerComponent.Instance.WaitAsync((long)(50 / self.speed), self.token);
				self.showLen = self.Text.GetCharacterCount();
			}
			self.Text.SetMaxVisibleCharacters(int.MaxValue);
			self.isPlay = false;
			self.token = new ETCancellationToken();
			if (GalGameEngineComponent.Instance.AutoPlay || !clear || self.speed <= 0)
			{
				if (self.speed <= 0)
					await TimerComponent.Instance.WaitAsync(10);
				else
					await TimerComponent.Instance.WaitAsync((long)(self.waitTime / self.speed), self.token);
			}
			if (!(GalGameEngineComponent.Instance.AutoPlay || !clear || self.speed <= 0))
            {
				while (true)
				{
					KeyCode keycode = await GalGameEngineComponent.Instance.WaitInput;
					if (keycode == KeyCode.Mouse0) break;
				}
			}
			if (clear)
			{
				self.allText = "";
				self.showLen = 0;
			}
		}

		public static void OnCancel(this UIBaseMessageWindow self)
        {
	        self.Text.SetMaxVisibleCharacters(int.MaxValue);
	        self.isPlay = false;
	        self.token?.Cancel();
		}

		public static void OnRunningStateEnd(this UIBaseMessageWindow self)
        {
	        self.speed = 0;
	        self.OnCancel();
		}
	}
}
