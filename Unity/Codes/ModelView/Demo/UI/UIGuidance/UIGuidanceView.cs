using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	public class UIGuidanceView : Entity, IAwake, ILoad, IOnCreate, IOnEnable<GameObject,int>
	{
		public static string PrefabPath => "UI/UIGuidance/Prefabs/UIGuidanceView.prefab";
		public UICircleMaskControl CircleMask;
		public UIRectMaskControl RectMask;
		public PointerMask Mask;

		public Canvas CurCanvas;
	}
}
