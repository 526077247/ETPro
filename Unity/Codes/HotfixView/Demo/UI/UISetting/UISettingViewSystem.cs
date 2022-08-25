using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UISettingView))]
	public class UISettingViewOnCreateSystem : OnCreateSystem<UISettingView>
	{

		public override void OnCreate(UISettingView self)
		{
			self.Chinese = self.AddUIComponent<UIToggle>("Panel/Language/Chinese");
			self.English = self.AddUIComponent<UIToggle>("Panel/Language/English");
			self.BackBtn = self.AddUIComponent<UIButton>("Panel/BackBtn");
			self.Chinese.SetOnValueChanged((val)=>{self.SetOnChineseValueChanged(val);});
			self.English.SetOnValueChanged((val)=>{self.SetOnEnglishValueChanged(val);});
			self.BackBtn.SetOnClick(()=>{self.OnClickBackBtn();});
		}

	}
	[ObjectSystem]
	[FriendClass(typeof(UISettingView))]
	public class UISettingViewLoadSystem : LoadSystem<UISettingView>
	{

		public override void Load(UISettingView self)
		{
			self.Chinese.SetOnValueChanged((val)=>{self.SetOnChineseValueChanged(val);});
			self.English.SetOnValueChanged((val)=>{self.SetOnEnglishValueChanged(val);});
			self.BackBtn.SetOnClick(()=>{self.OnClickBackBtn();});
		}

	}
	
	[UISystem]
	[FriendClass(typeof(UISettingView))]
	public class UISettingViewOnEnableSystem : OnEnableSystem<UISettingView>
	{

		public override void OnEnable(UISettingView self)
		{
			self.Chinese.SetIsOn(I18NComponent.Instance.GetCurLanguage()==I18NComponent.LangType.Chinese,false);
			self.English.SetIsOn(I18NComponent.Instance.GetCurLanguage()==I18NComponent.LangType.English,false);
		}

	}
	[FriendClass(typeof(UISettingView))]
	public static class UISettingViewSystem
	{
		public static void SetOnChineseValueChanged(this UISettingView self, bool val)
		{
			if(val)
				I18NComponent.Instance.SwitchLanguage(I18NComponent.LangType.Chinese);
		}
		public static void SetOnEnglishValueChanged(this UISettingView self, bool val)
		{
			if (val)
			{
				I18NComponent.Instance.SwitchLanguage(I18NComponent.LangType.English);
				GuidanceComponent.Instance.NoticeEvent("Click_Setting");
			}
		}
		
		public static void OnClickBackBtn(this UISettingView self)
		{
			self.CloseSelf().Coroutine();
		}
	}

}
