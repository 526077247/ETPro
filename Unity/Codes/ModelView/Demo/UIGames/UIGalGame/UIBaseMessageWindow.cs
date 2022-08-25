using System;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
	public class UIBaseMessageWindow : Entity,IOnWidthPaddingChange,IAwake,IOnCreate,IOnEnable<float,long>,IOnDisable
	{
		public static string UIMessageWindowMiddle = "UIGames/UIGalGame/Prefabs/UIMessageWindowMiddle.prefab";
		public static string UIMessageWindowFull = "UIGames/UIGalGame/Prefabs/UIMessageWindowFull.prefab";
		public UITextmesh Text;
		public UIPointerClick UIPointerClick;
		public string allText;
		public ETCancellationToken token;
		public bool isPlay = false;
		public int showLen;
		public float speed;
		public long waitTime;
		public Action CancelAction;
	}
}
