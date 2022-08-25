using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	public class UILoadingView : Entity,IAwake,IOnCreate,IOnEnable
	{
		public static UILoadingView Instance;
		public static string PrefabPath => "UI/UILoading/Prefabs/UILoadingView.prefab";
		public UISlider slider;
    }
}
