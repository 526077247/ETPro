using UnityEngine;

namespace ET
{
    [FriendClass(typeof(UICanvasRaycastFilter))]
    public static class UICanvasRaycastFilterSystem
    {
        [UISystem]
        [FriendClass(typeof(UICanvasRaycastFilter))]
        public class UICanvasRaycastFilterOnDestroySystem : OnDestroySystem<UICanvasRaycastFilter>
        {
            public override void OnDestroy(UICanvasRaycastFilter self)
            {
                if(self.canvasRaycastFilter!=null)
                    self.canvasRaycastFilter.Target = null;
            }
        }

        static void ActivatingComponent(this UICanvasRaycastFilter self)
        {
            if (self.canvasRaycastFilter == null)
            {
                self.canvasRaycastFilter = self.GetGameObject().GetComponent<CanvasRaycastFilter>();
                if (self.canvasRaycastFilter == null)
                {
                    Log.Error($"添加UI侧组件UICanvasRaycastFilter时，物体{self.GetGameObject().name}上没有找到CanvasRaycastFilter组件");
                }
            }
        }
        
        public static void SetTarget(this UICanvasRaycastFilter self,RectTransform target)
        {
            self.ActivatingComponent();
            self.canvasRaycastFilter.Target = target;
        }
        
        public static RectTransform GetTarget(this UICanvasRaycastFilter self)
        {
            return self.canvasRaycastFilter?.Target;
        }
    }
}