using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    
    public class UIRectMaskControl: Entity, IAwake, IDestroy, IOnCreate, IUpdate,IOnEnable,IOnEnable<RectTransform,Canvas>,IOnDisable
    {
        /// <summary>
        /// 要高亮显示的目标
        /// </summary>
        public UICanvasRaycastFilter CanvasRaycast;

        public UIImage Image;

        public Canvas Canvas;
        
        /// <summary>
        /// 区域范围缓存
        /// </summary>
        public Vector3[] _corners = new Vector3[4];
        /// <summary>
        /// 区域范围缓存
        /// </summary>
        public Vector3[] _corners1 = new Vector3[4];
        /// <summary>
        /// 区域范围缓存
        /// </summary>
        public Vector3[] _corners2 = new Vector3[4];
        /// <summary>
        /// 镂空区域中心
        /// </summary>
        public Vector4 _center;

        /// <summary>
        /// 最终的偏移值X
        /// </summary>
        public float _targetOffsetX = 0f;

        /// <summary>
        /// 最终的偏移值Y
        /// </summary>
        public float _targetOffsetY = 0f;

        /// <summary>
        /// 遮罩材质
        /// </summary>
        public Material _material;

        /// <summary>
        /// 当前的偏移值X
        /// </summary>
        public float _currentOffsetX = 0f;

        /// <summary>
        /// 当前的偏移值Y
        /// </summary>
        public float _currentOffsetY = 0f;

        /// <summary>
        /// 动画收缩时间
        /// </summary>
        public float _shrinkTime = 0.2f;

        /// <summary>
        /// 收缩速度
        /// </summary>
        public float _shrinkVelocityX = 0f;

        public float _shrinkVelocityY = 0f;

       
    }
}