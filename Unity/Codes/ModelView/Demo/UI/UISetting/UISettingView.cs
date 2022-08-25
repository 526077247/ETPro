using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	public class UISettingView : Entity, IAwake, ILoad, IOnCreate, IOnEnable
	{
		public static string PrefabPath => "UI/UISetting/Prefabs/UISettingView.prefab";
		public UIToggle Chinese;
		public UIToggle English;
		public UIButton BackBtn;
		 

	}
}
