using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UIBgView))]
	public class UIBgViewOnCreateSystem: OnCreateSystem<UIBgView>
	{
		public override void OnCreate(UIBgView self)
		{
			self.bg = self.AddUIComponent<UIImage>("Panel");
		}
	}
	[UISystem]
	public class UIBgViewOnEnableSystem: OnEnableSystem<UIBgView,string>
	{
		public override void OnEnable(UIBgView self,string a)
		{
			self.SetSprite(a);
		}
	}
	[FriendClass(typeof(UIBgView))]
	public static class UIBgViewSystem
	{
		public static void SetSprite(this UIBgView self,string path)
        {
	        self.bg.SetSpritePath(path).Coroutine();
		}

	}
}
