using UnityEngine;
using UnityEngine.UI;

namespace ET
{

    public class UICircleMaskControl: Entity,IAwake,IDestroy,IOnCreate,IUpdate,IOnEnable,IOnEnable<RectTransform,Canvas>,IOnDisable
    {
        /// <summary>
        /// 要高亮显示的目标
        /// </summary>
        public UICanvasRaycastFilter CanvasRaycast;

        public UIImage Image;
        
        //获取画布
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
        /// 镂空区域半径
        /// </summary>
        public float _radius;

        /// <summary>
        /// 遮罩材质
        /// </summary>
        public Material _material;

        /// <summary>
        /// 当前高亮区域的半径
        /// </summary>
        public float _currentRadius;

        /// <summary>
        /// 高亮区域缩放的动画时间
        /// </summary>
        public float _shrinkTime = 0.2f;

        /// <summary>
        /// 收缩速度
        /// </summary>
        public float _shrinkVelocity = 0f;

    }
}