using UnityEngine;
namespace ET
{
    [FriendClass(typeof(UIRectMaskControl))]
    public static class UIRectMaskControlSystem
    {
        [UISystem]
        [FriendClass(typeof(UIRectMaskControl))]
        public class UIRectMaskControlOnCreateSystem : OnCreateSystem<UIRectMaskControl>
        {
            public override void OnCreate(UIRectMaskControl self)
            {
                self.Image = self.AddUIComponent<UIImage>();
                self._material = self.Image.GetMaterial();
                self.CanvasRaycast = self.AddUIComponent<UICanvasRaycastFilter>();
            }
        }
        
        [FriendClass(typeof(UIRectMaskControl))]
        public class UIRectMaskControlUpdateSystem : UpdateSystem<UIRectMaskControl>
        {
            public override void Update(UIRectMaskControl self)
            {
                if (self.GetTarget() == null) return;
                self.Refresh();
                //从当前半径到目标半径差值显示收缩动画
                float valueX = Mathf.SmoothDamp(self._currentOffsetX, self._targetOffsetX, ref self._shrinkVelocityX, self._shrinkTime);
                float valueY = Mathf.SmoothDamp(self._currentOffsetY, self._targetOffsetY, ref self._shrinkVelocityY, self._shrinkTime);
                if (!Mathf.Approximately(valueX, self._currentOffsetX))
                {
                    self._currentOffsetX = valueX;
                    self._material.SetFloat("_SliderX", self._currentOffsetX);
                }

                if (!Mathf.Approximately(valueY, self._currentOffsetY))
                {
                    self._currentOffsetY = valueY;
                    self._material.SetFloat("_SliderY", self._currentOffsetY);
                }
            }
        }
        
        [UISystem]
        [FriendClass(typeof(UIRectMaskControl))]
        public class UIRectMaskControlOnDestroySystem : OnDestroySystem<UIRectMaskControl>
        {
            public override void OnDestroy(UIRectMaskControl self)
            {
                self._material.SetFloat("_SliderX", 0);
                self._material.SetFloat("_SliderY", 0);
            }
        }
        
        [UISystem]
        [FriendClass(typeof(UIRectMaskControl))]
        public class UIRectMaskControlOnEnableSystem : OnEnableSystem<UIRectMaskControl,RectTransform,Canvas>
        {
            public override void OnEnable(UIRectMaskControl self,RectTransform rectTransform,Canvas canvas)
            {
                self.Canvas = canvas;
                self.SetCurTarget(rectTransform);
            }
        }
        [UISystem]
        [FriendClass(typeof(UIRectMaskControl))]
        public class UIRectMaskControlOnDisableSystem : OnDisableSystem<UIRectMaskControl>
        {
            public override void OnDisable(UIRectMaskControl self)
            {
                self.Canvas = null;
                self.SetTarget(null);
            }
        }

        public static void SetTarget(this UIRectMaskControl self,RectTransform target)
        {
            self.CanvasRaycast.SetTarget(target);
        }
        
        public static RectTransform GetTarget(this UIRectMaskControl self)
        {
            return self.CanvasRaycast.GetTarget();
        }
         /// <summary>
        /// 世界坐标向画布坐标转换
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="world">世界坐标</param>
        /// <returns>返回画布上的二维坐标</returns>
        static Vector2 WorldToCanvasPos(Canvas canvas, Vector3 world)
        {
            Vector2 position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, world, null, out position);

            return position;
        }
         

        public static void SetCurTarget(this UIRectMaskControl self,RectTransform target)
        {
            self.SetTarget(target);
            self.Refresh();
        }
        
        public static void Refresh(this UIRectMaskControl self)
        {
            var target = self.GetTarget();
            //获取高亮区域的四个顶点的世界坐标
            target.GetWorldCorners(self._corners2);
            bool change = false;
            for (int i = 0; i < self._corners2.Length; i++)
            {
                if ((self._corners1[i] - self._corners2[i]).sqrMagnitude > 0.01)
                {
                    change = true;
                    break;
                }
            }
            if(!change)
                return;
            ObjectHelper.Swap(ref self._corners1,ref self._corners2);
            for (int i = 0; i < self._corners.Length; i++)
            {
                self._corners[i] = self._corners1[i];
            }
            //计算高亮显示区域咋画布中的范围
            self._targetOffsetX = Vector2.Distance(WorldToCanvasPos(self.Canvas, self._corners[0]), WorldToCanvasPos(self.Canvas, self._corners[3])) / 2f;
            self._targetOffsetY = Vector2.Distance(WorldToCanvasPos(self.Canvas, self._corners[0]), WorldToCanvasPos(self.Canvas, self._corners[1])) / 2f;
            //计算高亮显示区域的中心
            float x = self._corners[0].x + ((self._corners[3].x - self._corners[0].x) / 2f);
            float y = self._corners[0].y + ((self._corners[1].y - self._corners[0].y) / 2f);
            Vector3 centerWorld = new Vector3(x, y, 0);
            Vector2 center = WorldToCanvasPos(self.Canvas, centerWorld);
            //设置遮罩材料中中心变量
            Vector4 centerMat = new Vector4(center.x, center.y, 0, 0);
            self._material.SetVector("_Center", centerMat);
            //计算当前偏移的初始值
            RectTransform canvasRectTransform = (self.Canvas.transform as RectTransform);
            if (canvasRectTransform != null)
            {
                //获取画布区域的四个顶点
                canvasRectTransform.GetWorldCorners(self._corners);
                //求偏移初始值
                for (int i = 0; i < self._corners.Length; i++)
                {
                    if (i % 2 == 0)
                        self._currentOffsetX = Mathf.Max(Vector3.Distance(WorldToCanvasPos(self.Canvas, self._corners[i]), center), self._currentOffsetX);
                    else
                        self._currentOffsetY = Mathf.Max(Vector3.Distance(WorldToCanvasPos(self.Canvas, self._corners[i]), center), self._currentOffsetY);
                }
            }

            //设置遮罩材质中当前偏移的变量
            self._material.SetFloat("_SliderX", self._currentOffsetX);
            self._material.SetFloat("_SliderY", self._currentOffsetY);
        }

    }
}