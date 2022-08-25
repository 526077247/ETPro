using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	public class UIBgView : Entity,IAwake,IOnCreate,IOnEnable<string>
	{
		public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIBgView.prefab";

		public UIImage bg;
	}
}
