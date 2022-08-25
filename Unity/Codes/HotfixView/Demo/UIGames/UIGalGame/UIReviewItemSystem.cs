using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using SuperScrollView;
namespace ET
{
	[UISystem]
	[FriendClass(typeof(UIReviewItem))]
	public class UIReviewItemOnCreateSystem : OnCreateSystem<UIReviewItem>
	{

		public override void OnCreate(UIReviewItem self)
		{
			self.Title = self.AddUIComponent<UITextmesh>("Title");
			self.Content = self.AddUIComponent<UITextmesh>("Content");
		}

	}
	[FriendClass(typeof(UIReviewItem))]
	public static class UIReviewItemSystem
	{
		public static void SetData(this UIReviewItem self, string title, string content)
		{
			self.Title.SetText(title);
			self.Content.SetText(content);
		}
	}

}
