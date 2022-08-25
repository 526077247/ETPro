using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UIMessageWindow))]
	public class UIMessageWindowOnCreateSystem: OnCreateSystem<UIMessageWindow>
	{
		public override void OnCreate(UIMessageWindow self)
		{
			self.allText = "";
			self.showLen = 0;
			self.Text = self.AddUIComponent<UITextmesh>("Space/Bg/Content");
			self.Name = self.AddUIComponent<UITextmesh>("Space/Bg/NameBg/Name");
			self.NameBg = self.AddUIComponent<UIEmptyGameobject>("Space/Bg/NameBg");
			self.UIPointerClick = self.AddUIComponent<UIPointerClick>("Space/Bg");
			self.UIPointerClick.SetOnClick(()=>
			{
				self.OnCancel();
			});
			self.FastBtn = self.AddUIComponent<UIButton>("Space/Quick");
			self.FastBtn.SetOnClick(UIMessageWindowSystem.OnFastBtnClick);

			self.AutoBtn = self.AddUIComponent<UIButton>("Space/Auto");
			self.AutoBtn.SetOnClick(()=>
			{
				self.OnAutoBtnClick();
			});
			self.AutoBtnText = self.AddUIComponent<UITextmesh>("Space/Auto/Text");
			self.RecordBtn = self.AddUIComponent<UIButton>("Space/Record");
			self.RecordBtn.SetOnClick(()=>
			{
				self.OnRecordBtnClick().Coroutine();
			});
			self.CancelAction = () =>
			{
				self.OnRunningStateEnd();
			};
			
			self.Arrow = self.AddUIComponent<UIEmptyGameobject>("Space/Bg/Content/Arrow");
			var rect = (self.Arrow.GetTransform() as RectTransform).rect;
			self.Offset = new Vector3(rect.width / 2+5, rect.height / 2);
		}
	}
	[UISystem]
	[FriendClass(typeof(UIMessageWindow))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public class UIMessageWindowOnEnableSystem: OnEnableSystem<UIMessageWindow,float,long>
	{
		public override void OnEnable(UIMessageWindow self,float t,long v)
		{
			self.AutoBtnText.SetI18NKey(GalGameEngineComponent.Instance.AutoPlay?"GalGame_Stop":"Global_AutoPlay");
			self.speed = t;
			self.waitTime = v;
			if (GalGameEngineComponent.Instance.CancelToken == null)
				GalGameEngineComponent.Instance.CancelToken = new ETCancellationToken();
			GalGameEngineComponent.Instance.CancelToken.Add(self.CancelAction);
		}
	}
	[UISystem]
	[FriendClass(typeof(UIMessageWindow))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public class UIMessageWindowOnDisableSystem: OnDisableSystem<UIMessageWindow>
	{
		public override void OnDisable(UIMessageWindow self)
		{
			GalGameEngineComponent.Instance.CancelToken?.Remove(self.CancelAction);
		}
	}
	[FriendClass(typeof(UIMessageWindow))]
	[FriendClass(typeof(GalGameEngineComponent))]
	public static class UIMessageWindowSystem
	{
		
		public static async ETTask SetContent(this UIMessageWindow self,string key, bool play = true, bool clear = true)
		{
			if (string.IsNullOrEmpty(key)) return;
			I18NComponent.Instance.I18NTryGetText(key,out string text);
			var baseLen = self.showLen;
			// text=text.Replace("{0}", UserDataComponent.Instance.CurUserAssets.Name);
			self.allText += text;
			self.Text.SetText(self.allText);
			self.showLen = self.allText.Length;
			self.token = new ETCancellationToken();
			self.isPlay = play;
			self.Arrow.SetActive(false);
			for (int i = baseLen + 1; i <= self.showLen && self.isPlay && self.speed>0; i++)
			{
				self.Text.SetMaxVisibleCharacters(i);
				await TimerComponent.Instance.WaitAsync((long)(50 / self.speed), self.token);
				self.showLen = self.Text.GetCharacterCount();
			}
			self.Text.SetMaxVisibleCharacters(int.MaxValue);
			await TimerComponent.Instance.WaitAsync(1);
			self.isPlay = false;
			self.token = new ETCancellationToken();
			self.Arrow.SetActive(true);
			self.Arrow.GetTransform().localPosition = self.Text.GetLastCharacterLocalPosition()+self.Offset;
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

		public static void SetName(this UIMessageWindow self,string key)
        {
			self.NameBg.SetActive(!string.IsNullOrEmpty(key));
			if(!string.IsNullOrEmpty(key))
			{
				self.Name.SetI18NKey(key);
			}
		}
		public static void OnCancel(this UIMessageWindow self)
		{
			if (self.isPlay)
			{
				self.Text.SetMaxVisibleCharacters(int.MaxValue);
				self.isPlay = false;
			}
			self.token?.Cancel();
		}
		public static void OnFastBtnClick()
        {
			GalGameEngineComponent.Instance.ChangePlayFastModel().Coroutine();

		}

		public static void OnAutoBtnClick(this UIMessageWindow self)
		{
			GalGameEngineComponent.Instance.AutoPlay = !GalGameEngineComponent.Instance.AutoPlay;
			self.AutoBtnText.SetI18NKey(GalGameEngineComponent.Instance.AutoPlay?"GalGame_Stop":"Global_AutoPlay");
		}

		public static async ETTask OnRecordBtnClick(this UIMessageWindow self)
        {
	        await UIManagerComponent.Instance.CloseWindow<UIGalGameHelper>();
			//先停掉快进状态
			if(GalGameEngineComponent.Instance.State == GalGameEngineComponent.GalGameEngineState.FastForward)
				await GalGameEngineComponent.Instance.ChangePlayFastModel();
			UIManagerComponent.Instance.OpenWindow<UIReview, bool, List<GalGameEngineComponent.ReviewItem>>(UIReview.PrefabPath,
				GalGameEngineComponent.Instance.AutoPlay,GalGameEngineComponent.Instance.ReviewItems,UILayerNames.TipLayer).Coroutine();
			GalGameEngineComponent.Instance.AutoPlay = false;
		}
		
		public static void OnRunningStateEnd(this UIMessageWindow self)
		{
			self.speed = 0;
			self.OnCancel();
		}
	}
}
