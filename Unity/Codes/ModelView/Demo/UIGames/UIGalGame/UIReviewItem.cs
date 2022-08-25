using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	public class UIReviewItem : Entity,IOnCreate,IOnEnable,IAwake
	{
		public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIReviewItem.prefab";
		public UITextmesh Title;
		public UITextmesh Content;
		 

	}
}
