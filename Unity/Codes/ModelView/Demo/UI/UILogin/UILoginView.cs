using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace ET
{
	public class UILoginView: Entity,IAwake,IOnCreate,IOnEnable<Scene>
	{
		public UIButton loginBtn;
		public UIInput password;
		public UIInput account;
		public UIInputTextmesh ipaddr;
		public UIButton registerBtn;
		public UILoopListView2 settingView;
		public Scene scene;

        public static string PrefabPath => "UI/UILogin/Prefabs/UILoginView.prefab";

	}
}
