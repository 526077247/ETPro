using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    [FriendClass(typeof(UILayersComponent))]
    public class UILayerAwakeSystem : AwakeSystem<UILayer, UILayerDefine, GameObject>
    {
        public override void Awake(UILayer self, UILayerDefine layer,GameObject gameObject)
        {
            self.Name = layer.Name;
            self.gameObject = gameObject;
            //canvas
            if (!self.gameObject.TryGetComponent(out self.unity_canvas))
            {
                //说明：很坑爹，这里添加UI组件以后transform会Unity被替换掉，必须重新获取
                self.unity_canvas = self.gameObject.AddComponent<Canvas>();
                self.gameObject = self.unity_canvas.gameObject;
            }
            self.unity_canvas.renderMode = RenderMode.ScreenSpaceCamera;
            self.unity_canvas.worldCamera = UILayersComponent.Instance.UICamera;
            self.unity_canvas.planeDistance = layer.PlaneDistance;
            self.unity_canvas.sortingLayerName = SortingLayerNames.UI;
            self.unity_canvas.sortingOrder = layer.OrderInLayer;

            //scaler
            if (!self.gameObject.TryGetComponent(out self.unity_canvas_scaler))
            {
                self.unity_canvas_scaler = self.gameObject.AddComponent<CanvasScaler>();
            }
            self.unity_canvas_scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            self.unity_canvas_scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            self.unity_canvas_scaler.referenceResolution = UILayersComponent.Instance.Resolution;
            if (Screen.width / Screen.height > Define.DesignScreen_Width / Define.DesignScreen_Height)
                self.unity_canvas_scaler.matchWidthOrHeight = 1;
            else
                self.unity_canvas_scaler.matchWidthOrHeight = 0;

            //raycaster
            if (!self.gameObject.TryGetComponent(out self.unity_graphic_raycaster))
            {
                self.unity_graphic_raycaster = self.gameObject.AddComponent<GraphicRaycaster>();
            }
            // window order
            self.top_window_order = layer.OrderInLayer;
            self.min_window_order = layer.OrderInLayer;
            self.rectTransform = self.gameObject.GetComponent<RectTransform>();
        }
    }
    [ObjectSystem]
    public class UILayerDestroySystem : DestroySystem<UILayer>
    {

        public override void Destroy(UILayer self)
        {
            self.unity_canvas = null;
            self.unity_canvas_scaler = null;
            self.unity_graphic_raycaster = null;
        }
    }
    [FriendClass(typeof(UILayer))]
    [FriendClass(typeof(UIManagerComponent))]
    [FriendClass(typeof(UILayersComponent))]
    public static class UILayerSystem
    {
        //设置canvas的worldCamera
        public static void SetCanvasWorldCamera(this UILayer self,Camera camera)
        {
            var old_camera = self.unity_canvas.worldCamera;
            if (old_camera != camera)
            {
                self.unity_canvas.worldCamera = camera;
            }
        }

        public static int GetCanvasLayer(this UILayer self)
        {
            return self.transform.gameObject.layer;
        }

        public static int PopWindowOder(this UILayer self)
        {
            var cur = self.top_window_order;
            self.top_window_order += UIManagerComponent.Instance.MaxOderPerWindow;
            return cur;
        }

        public static int PushWindowOrder(this UILayer self)
        {
            var cur = self.top_window_order;
            self.top_window_order -= UIManagerComponent.Instance.MaxOderPerWindow;
            return cur;
        }

        public static int GetMinOrderInLayer(this UILayer self)
        {
            return self.min_window_order;
        }

        public static void SetTopOrderInLayer(this UILayer self,int order)
        {
            if (self.top_window_order < order)
            {
                self.top_window_order = order;
            }
        }

        public static int GetTopOrderInLayer(this UILayer self)
        {
            return self.top_window_order;
        }

        public static Vector2 GetCanvasSize(this UILayer self)
        {
            return self.rectTransform.rect.size;
        }

        /// <summary>
        /// editor调整canvas scale
        /// </summary>
        /// <param name="flag">是否竖屏</param>
        public static void SetCanvasScaleEditorPortrait(this UILayer self,bool flag)
        {
            if (flag)
            {
                self.unity_canvas_scaler.referenceResolution = new Vector2(Define.DesignScreen_Height, Define.DesignScreen_Width);
                self.unity_canvas_scaler.matchWidthOrHeight = 0;
            }
            else
            {
                self.unity_canvas_scaler.referenceResolution = UIManagerComponent.Instance.GetComponent<UILayersComponent>().Resolution;
                self.unity_canvas_scaler.matchWidthOrHeight = 1;
            }
        }
    }
}
