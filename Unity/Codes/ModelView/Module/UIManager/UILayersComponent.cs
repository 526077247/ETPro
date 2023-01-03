using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{

	[ComponentOf(typeof(UIManagerComponent))]
	public class UILayersComponent : Entity,IAwake,IDestroy
    {
		public static UILayersComponent Instance;
		public string UIRootPath;//UIRoot路径
		public string EventSystemPath;// EventSystem路径
		public string UICameraPath;// UICamera路径
		
		public Dictionary<UILayerNames, UILayer> Layers;//所有可用的层级
		

		public bool NeedTurn;
		public Camera UICamera;
		public Vector2 Resolution;

		public GameObject gameObject;
	}
}
