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
            if (!self.gameObject.TryGetComponent(out self.unityCanvas))
            {
                //说明：很坑爹，这里添加UI组件以后transform会Unity被替换掉，必须重新获取
                self.unityCanvas = self.gameObject.AddComponent<Canvas>();
                self.gameObject = self.unityCanvas.gameObject;
            }
            self.unityCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            self.unityCanvas.worldCamera = UILayersComponent.Instance.UICamera;
            self.unityCanvas.planeDistance = layer.PlaneDistance;
            self.unityCanvas.sortingLayerName = SortingLayerNames.UI;
            self.unityCanvas.sortingOrder = layer.OrderInLayer;

            //scaler
            if (!self.gameObject.TryGetComponent(out self.unityCanvasScaler))
            {
                self.unityCanvasScaler = self.gameObject.AddComponent<CanvasScaler>();
            }
            self.unityCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            self.unityCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            self.unityCanvasScaler.referenceResolution = UILayersComponent.Instance.Resolution;
            if (Screen.width / Screen.height > Define.DesignScreenWidth / Define.DesignScreenHeight)
                self.unityCanvasScaler.matchWidthOrHeight = 1;
            else
                self.unityCanvasScaler.matchWidthOrHeight = 0;

            //raycaster
            if (!self.gameObject.TryGetComponent(out self.unityGraphicRaycaster))
            {
                self.unityGraphicRaycaster = self.gameObject.AddComponent<GraphicRaycaster>();
            }
            // window order
            self.topWindowOrder = layer.OrderInLayer;
            self.minWindowOrder = layer.OrderInLayer;
            self.rectTransform = self.gameObject.GetComponent<RectTransform>();
        }
    }
    [ObjectSystem]
    public class UILayerDestroySystem : DestroySystem<UILayer>
    {

        public override void Destroy(UILayer self)
        {
            self.unityCanvas = null;
            self.unityCanvasScaler = null;
            self.unityGraphicRaycaster = null;
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
            var old_camera = self.unityCanvas.worldCamera;
            if (old_camera != camera)
            {
                self.unityCanvas.worldCamera = camera;
            }
        }

        public static int GetCanvasLayer(this UILayer self)
        {
            return self.transform.gameObject.layer;
        }

        public static int PopWindowOder(this UILayer self)
        {
            var cur = self.topWindowOrder;
            self.topWindowOrder += UIManagerComponent.Instance.MaxOderPerWindow;
            return cur;
        }

        public static int PushWindowOrder(this UILayer self)
        {
            var cur = self.topWindowOrder;
            self.topWindowOrder -= UIManagerComponent.Instance.MaxOderPerWindow;
            return cur;
        }

        public static int GetMinOrderInLayer(this UILayer self)
        {
            return self.minWindowOrder;
        }

        public static void SetTopOrderInLayer(this UILayer self,int order)
        {
            if (self.topWindowOrder < order)
            {
                self.topWindowOrder = order;
            }
        }

        public static int GetTopOrderInLayer(this UILayer self)
        {
            return self.topWindowOrder;
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
                self.unityCanvasScaler.referenceResolution = new Vector2(Define.DesignScreenHeight, Define.DesignScreenWidth);
                self.unityCanvasScaler.matchWidthOrHeight = 0;
            }
            else
            {
                self.unityCanvasScaler.referenceResolution = UIManagerComponent.Instance.GetComponent<UILayersComponent>().Resolution;
                self.unityCanvasScaler.matchWidthOrHeight = 1;
            }
        }
    }
}
