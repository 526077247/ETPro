
using System.Collections.Generic;

namespace ET
{
	[UIComponent]
	public class UIStageView : Entity,IOnWidthPaddingChange,IAwake,IOnCreate,IOnEnable<GalGameEngineComponent>,
		IOnEnable<GalGameEngineComponent,GalGameEnginePara>
	{	
		
		public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIStageView.prefab";
		public GalGameEngineComponent Engine;
		public Dictionary<string, UIStageRoleInfo> infos;
	}
}
