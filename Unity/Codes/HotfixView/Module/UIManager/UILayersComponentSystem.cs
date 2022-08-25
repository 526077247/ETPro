using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
	[ObjectSystem]
	[FriendClass(typeof(UIManagerComponent))]
	public class UILayersComponentAwakeSystem : AwakeSystem<UILayersComponent>
    {

		UILayerDefine[] GetConfig()
        {
			UILayerDefine GameBackgroudLayer = new UILayerDefine
			{
				Name = UILayerNames.GameBackgroudLayer,
				PlaneDistance = 1000,
				OrderInLayer = 0,
			};

			//主界面、全屏的一些界面
			UILayerDefine BackgroudLayer = new UILayerDefine
			{
				Name = UILayerNames.BackgroudLayer,
				PlaneDistance = 900,
				OrderInLayer = 1000,
			};

			//游戏内的View层
			UILayerDefine GameLayer = new UILayerDefine
			{
				Name = UILayerNames.GameLayer,
				PlaneDistance = 800,
				OrderInLayer = 1800,
			};
			// 场景UI，如：点击建筑查看建筑信息---一般置于场景之上，界面UI之下
			UILayerDefine SceneLayer = new UILayerDefine
			{
				Name = UILayerNames.SceneLayer,
				PlaneDistance = 700,
				OrderInLayer = 2000,
			};
			//普通UI，一级、二级、三级等窗口---一般由用户点击打开的多级窗口
			UILayerDefine NormalLayer = new UILayerDefine
			{
				Name = UILayerNames.NormalLayer,
				PlaneDistance = 600,
				OrderInLayer = 3000,
			};
			//提示UI，如：错误弹窗，网络连接弹窗等
			UILayerDefine TipLayer = new UILayerDefine
			{
				Name = UILayerNames.TipLayer,
				PlaneDistance = 500,
				OrderInLayer = 4000,
			};
			//顶层UI，如：场景加载
			UILayerDefine TopLayer = new UILayerDefine
			{
				Name = UILayerNames.TopLayer,
				PlaneDistance = 400,
				OrderInLayer = 5000,
			};

			return new UILayerDefine[]
			{
				GameBackgroudLayer ,
				BackgroudLayer,
				GameLayer,
				SceneLayer,
				NormalLayer,
				TipLayer,
				TopLayer,
			};
		}

		public override void Awake(UILayersComponent self)
        {
			Log.Info("UILayersComponent Awake");
			UILayersComponent.Instance = self;
			self.UIRootPath = "Global/UI";
			self.EventSystemPath = "EventSystem";
			self.UICameraPath = self.UIRootPath + "/UICamera";
			self.gameObject = GameObject.Find(self.UIRootPath);
			var event_system = GameObject.Find(self.EventSystemPath);
			var transform = self.gameObject.transform;
			self.UICamera = GameObject.Find(self.UICameraPath).GetComponent<Camera>();
			GameObject.DontDestroyOnLoad(self.gameObject);
			GameObject.DontDestroyOnLoad(event_system);
			self.Resolution = new Vector2(Define.DesignScreen_Width, Define.DesignScreen_Height);//分辨率
			self.layers = new Dictionary<UILayerNames, UILayer>();

			var UILayers = GetConfig();
			for (int i = 0; i < UILayers.Length; i++)
			{
				var layer = UILayers[i];
				var go = new GameObject(layer.Name.ToString())
				{
					layer = 5
				};
				var trans = go.transform;
				trans.SetParent(transform, false);
				UILayer new_layer = self.AddChild<UILayer, UILayerDefine, GameObject>(layer, go);
				self.layers[layer.Name] = new_layer;
				UIManagerComponent.Instance.window_stack[layer.Name] = new LinkedList<string>();
			}

			var flagx = (float)Define.DesignScreen_Width / (Screen.width > Screen.height ? Screen.width : Screen.height);
			var flagy = (float)Define.DesignScreen_Height / (Screen.width > Screen.height ? Screen.height : Screen.width);
			UIManagerComponent.Instance.ScreenSizeflag = flagx > flagy ? flagx : flagy;
		}
    }

    public class UILayersComponentDestroySystem : DestroySystem<UILayersComponent>
	{
		public override void Destroy(UILayersComponent self)
		{
			foreach (var item in self.layers)
			{
				var obj = item.Value.transform.gameObject;
				GameObject.Destroy(obj);
			}
			self.layers.Clear();
			self.layers = null;
			Log.Info("UILayersComponent Dispose");
		}

	}
    [FriendClass(typeof(UILayersComponent))]
	public static class UILayersComponentSystem
    {
		public static GameObject GetUIRoot(this UIManagerComponent self)
        {
			return UILayersComponent.Instance.gameObject;
		}

		public static Camera GetUICamera(this UIManagerComponent self)
        {
			return UILayersComponent.Instance.UICamera;
		}

		public static GameObject GetUICameraGo(this UIManagerComponent self)
        {
			return UILayersComponent.Instance.UICamera.gameObject;
		}

		public static Vector2 GetResolution(this UIManagerComponent self)
        {
			return UILayersComponent.Instance.Resolution;
		}

		public static void SetNeedTurn(this UIManagerComponent self,bool flag)
        {
			UILayersComponent.Instance.need_turn = flag;
		}

		public static bool GetNeedTurn(this UIManagerComponent self)
        {
			return UILayersComponent.Instance.need_turn;
		}
		

		public static Entity GetView(this UIManagerComponent self,string ui_name)
		{
			var res = self.GetWindow(ui_name);
			if (res != null)
            {
				return res.GetComponent<Entity>();
            }
			return null;
		}

		public static UILayer GetLayer(this UIManagerComponent self, UILayerNames layer)
        {
			if(UILayersComponent.Instance.layers.TryGetValue(layer,out var res))
            {
				return res;
			}
			return null;
        }

		public static void SetCanvasScaleEditorPortrait(this UILayersComponent self, bool flag)
		{
			self.layers[UILayerNames.GameLayer].SetCanvasScaleEditorPortrait(flag);
			self.layers[UILayerNames.TipLayer].SetCanvasScaleEditorPortrait(flag);
			self.layers[UILayerNames.TopLayer].SetCanvasScaleEditorPortrait(flag);
			self.layers[UILayerNames.GameBackgroudLayer].SetCanvasScaleEditorPortrait(flag);
		}

	}
}
