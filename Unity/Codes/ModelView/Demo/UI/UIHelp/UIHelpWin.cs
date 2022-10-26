using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[UIComponent]
	public class UIHelpWin : Entity, IAwake,IOnCreate,IOnEnable
	{
		public UIText text;
		public UIButton GalBtn;
		public UIButton SettingBtn;
		public static string PrefabPath => "UI/UIHelp/Prefabs/UIHelpWin.prefab";
	}
}
