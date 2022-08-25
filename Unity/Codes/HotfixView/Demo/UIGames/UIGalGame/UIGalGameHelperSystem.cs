using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UIGalGameHelper))]
	public class UIGalGameHelperOnCreateSystem: OnCreateSystem<UIGalGameHelper>
	{
		public override void OnCreate(UIGalGameHelper self)
		{
			self.inputer = self.GetTransform().GetComponent<KeyListener>();
		}
	}
	[UISystem]
	[FriendClass(typeof(UIGalGameHelper))]
	public class UIGalGameHelperOnEnableSystem: OnEnableSystem<UIGalGameHelper>
	{
		public override void OnEnable(UIGalGameHelper self)
		{
			self.inputer.OnKeyUp += UIGalGameHelperSystem.OnKeyHandler;
		}
	}
	[UISystem]
	[FriendClass(typeof(UIGalGameHelper))]
	public class UIGalGameHelperOnDisableSystem: OnDisableSystem<UIGalGameHelper>
	{
		public override void OnDisable(UIGalGameHelper self)
		{
			self.inputer.OnKeyUp -= UIGalGameHelperSystem.OnKeyHandler;
		}
	}
	[FriendClass(typeof(GalGameEngineComponent))]
	public static class UIGalGameHelperSystem
	{

		public static void OnKeyHandler(KeyCode code)
		{
			GalGameEngineComponent.Instance.WaitInput.SetResult(code);
			GalGameEngineComponent.Instance.WaitInput = ETTask<KeyCode>.Create();
		}
	}
}
