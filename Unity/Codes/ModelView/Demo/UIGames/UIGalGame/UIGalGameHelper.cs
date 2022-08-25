using System;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	public class UIGalGameHelper : Entity,IAwake,IOnCreate,IOnEnable,IOnDisable
	{
		public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIGalGameHelper.prefab";

		public KeyListener inputer;
	}
}
