using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	public class UIMessageWindow : Entity,IOnWidthPaddingChange,IAwake,IOnCreate,IOnEnable<float,long>,IOnDisable
	{
		public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIMessageWindow.prefab";
		public UITextmesh Name;
		public UIEmptyGameobject NameBg;
		public UIButton FastBtn;
		public UIButton AutoBtn;
		public UITextmesh AutoBtnText;
		public UIButton RecordBtn;
		public UITextmesh Text;
		public UIPointerClick UIPointerClick;
		public string allText;
		public ETCancellationToken token;
		public bool isPlay = false;
		public int showLen;
		public float speed;
		public long waitTime;
		public Action CancelAction;
		
		public UIEmptyGameobject Arrow;
		public Vector3 Offset;

	}
}
