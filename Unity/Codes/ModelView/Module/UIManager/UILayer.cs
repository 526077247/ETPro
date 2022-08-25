using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    //sorting layer名字配置
    public class SortingLayerNames
    {
        public static readonly string Default = "Default";
        public static readonly string Map = "Map";
        public static readonly string Scene = "Scene";
        public static readonly string Charactor = "Charactor";
        public static readonly string UI = "UI";
    }

    [ChildOf(typeof(UILayersComponent))]
    public class UILayer : Entity,IAwake<UILayerDefine, GameObject>,IDestroy
    {
        public UILayerNames Name;
        public Canvas unity_canvas;
        public CanvasScaler unity_canvas_scaler;
        public GraphicRaycaster unity_graphic_raycaster;
        public RectTransform rectTransform;
        public int top_window_order;
        public int min_window_order;
        protected GameObject _gameObject;
        protected Transform _transform;
        public GameObject gameObject
        {
            get
            {
                return _gameObject;
            }
            set
            {
                _gameObject = value;
                _transform = _gameObject.transform;
            }
        }
        public Transform transform
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
                _gameObject = _transform.gameObject;
            }
        }
    }


}
