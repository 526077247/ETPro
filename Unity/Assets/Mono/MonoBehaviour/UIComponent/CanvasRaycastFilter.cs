using UnityEngine;
using UnityEngine.UI;
namespace ET
{
    public class CanvasRaycastFilter: MonoBehaviour, ICanvasRaycastFilter
    {
        /// <summary>
        /// 要高亮显示的目标
        /// </summary>
        public RectTransform Target { get; set; }
        
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (Target == null) return true;

            //在目标范围内做事件渗透
            return !RectTransformUtility.RectangleContainsScreenPoint(Target, sp, eventCamera);
        }
    }
}