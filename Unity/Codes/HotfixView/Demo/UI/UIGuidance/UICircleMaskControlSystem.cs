using UnityEngine;
using UnityEngine.UI;
namespace ET
{
    [FriendClass(typeof(UICircleMaskControl))]
    public static class UICircleMaskControlSystem
    {
        [UISystem]
        [FriendClass(typeof(UICircleMaskControl))]
        public class UICircleMaskControlOnCreateSystem : OnCreateSystem<UICircleMaskControl>
        {
            public override void OnCreate(UICircleMaskControl self)
            {
                self.Image = self.AddUIComponent<UIImage>();
                self._material = self.Image.GetMaterial();
                self.CanvasRaycast = self.AddUIComponent<UICanvasRaycastFilter>();
            }
        }
        
        [FriendClass(typeof(UICircleMaskControl))]
        public class UICircleMaskControlUpdateSystem : UpdateSystem<UICircleMaskControl>
        {
            public override void Update(UICircleMaskControl self)
            {
                if (self.GetTarget() == null) return;
                self.Refresh();
                //从当前半径到目标半径差值显示收缩动画
                float value = Mathf.SmoothDamp(self._currentRadius, self._radius, ref self._shrinkVelocity, self._shrinkTime);
                if (!Mathf.Approximately(value, self._currentRadius))
                {
                    self._currentRadius = value;

                    self._material.SetFloat("_Slider", self._currentRadius);
                }
            }
        }
        
        [UISystem]
        [FriendClass(typeof(UICircleMaskControl))]
        public class UICircleMaskControlOnDestroySystem : OnDestroySystem<UICircleMaskControl>
        {
            public override void OnDestroy(UICircleMaskControl self)
            {
                self._material.SetFloat("_Slider", 0);
            }
        }
        [UISystem]
        [FriendClass(typeof(UICircleMaskControl))]
        public class UICircleMaskControlOnEnableSystem : OnEnableSystem<UICircleMaskControl,RectTransform,Canvas>
        {
            public override void OnEnable(UICircleMaskControl self,RectTransform rectTransform,Canvas canvas)
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
        public static void SetTarget(this UICircleMaskControl self,RectTransform target)
        {
            self.CanvasRaycast.SetTarget(target);
        }
        
        public static RectTransform GetTarget(this UICircleMaskControl self)
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

        public static void SetCurTarget(this UICircleMaskControl self, RectTransform target)
        {
            self.SetTarget(target);
            self.Refresh();
        }
        
        public static void Refresh(this UICircleMaskControl self)
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
            
            //计算最终高亮显示区域的半径
            self._radius = Vector2.Distance(WorldToCanvasPos(self.Canvas, self._corners[0]), WorldToCanvasPos(self.Canvas, self._corners[1])) / 2f + 30f;

            //计算高亮显示区域的圆心
            float x = self._corners[0].x + ((self._corners[3].x - self._corners[0].x) / 2f);
            float y = self._corners[0].y + ((self._corners[1].y - self._corners[0].y) / 2f);

            Vector3 centerWorld = new Vector3(x, y, 0);

            Vector2 center = WorldToCanvasPos(self.Canvas, centerWorld);

            //设置遮罩材料中的圆心变量
            Vector4 centerMat = new Vector4(center.x, center.y, 0, 0);

            self._material.SetVector("_Center", centerMat);

            //计算当前高亮显示区域的半径
            RectTransform canRectTransform = self.Canvas.transform as RectTransform;
            if (canRectTransform != null)
            {
                //获取画布区域的四个顶点
                canRectTransform.GetWorldCorners(self._corners);

                //将画布顶点距离高亮区域中心最远的距离作为当前高亮区域半径的初始值
                foreach (Vector3 corner in self._corners)
                {
                    self._currentRadius = Mathf.Max(Vector3.Distance(WorldToCanvasPos(self.Canvas, corner), center), self._currentRadius);
                }
            }

            self._material.SetFloat("_Slider", self._currentRadius);


        }
        
    }
}