using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class AOITriggerAwakeSystem : AwakeSystem<AOITrigger,float,Action<AOIUnitComponent, AOITriggerType>>
    {
        public override void Awake(AOITrigger self,float a,Action<AOIUnitComponent, AOITriggerType> b)
        {
            if (Define.Debug)
            {
                self.DebugMap = DictionaryComponent<AOICell, int>.Create();
                self.LogInfo = ListComponent<string>.Create();
            }
            self.Selecter = ListComponent<UnitType>.Create();
            self.Radius = a;
            self.SqrRadius = a * a;
            self.Handler = b;
            self.Enable = true;
            self.FollowCell = ListComponent<AOICell>.Create();
        }
    }
    [ObjectSystem]
    public class AOITriggerAwakeSystem1 : AwakeSystem<AOITrigger,float>
    {
        public override void Awake(AOITrigger self,float a)
        {
            if (Define.Debug)
            {
                self.DebugMap = DictionaryComponent<AOICell, int>.Create();
                self.LogInfo = ListComponent<string>.Create();
            }
            self.Selecter = ListComponent<UnitType>.Create();
            self.Radius = a;
            self.SqrRadius = a * a;
            self.Handler = null;
            self.Enable = true;
            self.FollowCell = ListComponent<AOICell>.Create();
        }
    }
    [ObjectSystem]
    [FriendClass(typeof(AOICell))]
    public class AOITriggerDestroySystem : DestroySystem<AOITrigger>
    {
        public override void Destroy(AOITrigger self)
        {
            if(self.FollowCell == null) return;
            // Log.Info("RemoverTrigger"+self.Id);
            if (self.TriggerType != TriggerShapeType.Cube) //OBB的在子组件处理
            {
                if(!self.IsCollider)
                    self.GetParent<AOIUnitComponent>().RemoverTrigger(self);
                else
                    self.GetParent<AOIUnitComponent>().RemoverCollider(self);
            }

            if (self.Selecter != null)
            {
                self.Selecter.Dispose();
                self.Selecter = null;
            }

            self.Handler=null;
            if (self.FollowCell != null)
            {
                self.FollowCell.Dispose();
                self.FollowCell = null;
            }

            if (Define.Debug)
            {
                bool hasErr = false;
                foreach (var item in self.DebugMap)
                {
                    if (item.Key!=null&&!item.Key.IsDisposed&&item.Value != 0)
                    {
                        hasErr = true;
                        Log.Error("碰撞器没完全移除 "+item.Value+"       "+item.Key.posx+","+
                                  item.Key.posy);
                    }
                }

                if (hasErr)
                {
                    for (int i = 0; i < self.LogInfo.Count; i++)
                    {
                        var item = self.LogInfo[i];
                        Log.Info(item);
                    }
                }
                self.DebugMap.Dispose();
                self.LogInfo.Dispose();
            }
        }
    }
    [FriendClass(typeof(OBBComponent))]
    [FriendClass(typeof(AOITrigger))]
    [FriendClass(typeof(AOIUnitComponent))]
    [FriendClass(typeof(AOISceneComponent))]
    [FriendClass(typeof(AOICell))]
    public static class AOITriggerSystem
    {
        /// <summary>
        /// 设置是否生效
        /// </summary>
        /// <param name="self"></param>
        /// <param name="enable"></param>
        public static void SetEnable(this AOITrigger self, bool enable)
        {
            if(self.Enable == enable) return;
            self.Enable = enable;
            if (self.Enable)
            {
                if(self.IsCollider)
                    self.GetParent<AOIUnitComponent>().AddColliderListener(self);
                else
                    self.GetParent<AOIUnitComponent>().AddTriggerListener(self,self.Flag);
            }
            else
            {
                if(self.IsCollider)
                    self.GetParent<AOIUnitComponent>().RemoverColliderListener(self);
                else
                    self.GetParent<AOIUnitComponent>().RemoveTriggerListener(self);
            }
        }
        public static void OnTrigger(this AOITrigger self, AOITrigger other, AOITriggerType type)
        {
#if SERVER
            if(self.GetParent<AOIUnitComponent>().IsGhost()) return;  
#endif
            // Log.Info("OnTrigger"+type);
            self.Handler?.Invoke(other.GetParent<AOIUnitComponent>(),type);
        }
        /// <summary>
        /// 获取偏移后的位置
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Vector3 GetRealPos(this AOITrigger self)
        {
            if (self.OffsetY != 0)
            {
                return self.GetParent<AOIUnitComponent>().Position + new Vector3(0, self.OffsetY, 0);
            }
            else
            {
                return self.GetParent<AOIUnitComponent>().Position;
            }
        }
        /// <summary>
        /// 获取偏移后的位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3 GetRealPos(this AOITrigger self, Vector3 pos)
        {
            if (self.OffsetY != 0)
            {
                return pos + new Vector3(0, self.OffsetY, 0);
            }
            else
            {
                return pos;
            }
        }
        /// <summary>
        /// 获取偏移后的位置
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Quaternion GetRealRot(this AOITrigger self)
        {
            return self.GetParent<AOIUnitComponent>().Rotation;
        }
        /// <summary>
        /// 初始化触发器数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="radius"></param>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <param name="selecter"></param>
        /// <returns></returns>
        static AOITrigger AddTrigger(this AOIUnitComponent self, float radius, AOITriggerType type,
            Action<AOIUnitComponent, AOITriggerType> handler, params UnitType[] selecter)
        {
            AOITrigger trigger = self.AddChild<AOITrigger,float,Action<AOIUnitComponent, AOITriggerType>>(radius,handler);
            trigger.Flag = type;
           
            trigger.Selecter.AddRange(selecter);
            trigger.TriggerType=TriggerShapeType.Sphere;
            trigger.IsCollider = false;
            self.SphereTriggers.Add(trigger);
            return trigger;
        }
        /// <summary>
        /// 初始化碰撞器数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        static AOITrigger AddCollider(this AOIUnitComponent self, float radius)
        {
            AOITrigger trigger = self.AddChild<AOITrigger,float>(radius);
            trigger.Selecter = null;
            trigger.TriggerType=TriggerShapeType.Sphere;
            trigger.IsCollider = true;
            self.SphereTriggers.Add(trigger);
            return trigger;
        }
        /// <summary>
        /// 添加监听事件，并判断触发进入触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        /// <param name="type"></param>
        /// <param name="broadcast">是否需要检测触发</param>
        static void AddTriggerListener(this AOIUnitComponent self,AOITrigger trigger,AOITriggerType type,bool broadcast = true)
        {
            var len = self.Scene.gridLen;
            int count = (int)Mathf.Ceil(trigger.Radius / len);
            if (count > 2) Log.Info("检测范围超过2格，触发半径："+ trigger.Radius);
            var pos = trigger.GetRealPos();
            var rot = trigger.GetRealRot();
            if (!broadcast)
            {
                using (var grids = self.GetNearbyGrid(count))
                {
                    for (int i = 0; i < grids.Count; i++)
                    {
                        var item = grids[i];
                        var flag = item.IsIntersectWithTrigger(trigger,pos,rot);
                        if (flag) //格子在范围有重叠部分
                        {
                            item.AddTriggerListener(trigger);
                        }
                    }
                }
            }
            else
            {
                using (var grids = self.GetNearbyGrid(count))
                {
                    HashSetComponent<AOITrigger> temp1 = HashSetComponent<AOITrigger>.Create();
                    for (int i = 0; i < grids.Count; i++)
                    {
                        var item = grids[i];
                        var flag = item.IsIntersectWithTrigger(trigger,pos,rot);
                        // Log.Info("grids pos "+item.posx+" "+item.posy+" flag"+flag);
                        if (flag) //格子在范围有重叠部分
                        {
                            item.AddTriggerListener(trigger);
                            //别人进入自己
                            if (type == AOITriggerType.All || type == AOITriggerType.Enter) //注意不能放前面判断
                            {
                                using (var colliders = item.GetAllCollider(trigger.Selecter, trigger))
                                {
                                    for (int j = 0; j < colliders.Count; j++)
                                    {
                                        var collider = colliders[j];
                                        if (collider.Parent.Id == self.Id) continue;
                                        if (!temp1.Contains(collider) && trigger.IsInTrigger(collider,
                                                trigger.GetRealPos(),
                                                trigger.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                                        {
                                            // Log.Info("grids pos " + item.posx + " " + item.posy);
                                            temp1.Add(collider);
                                        }
                                    }
                                }
                            }
                            
                        }
                    }

                    foreach (var item in temp1)
                    {
                        trigger.OnTrigger(item, AOITriggerType.Enter);
                    }

                    temp1.Dispose();
                }
            }
        }
        /// <summary>
        /// 添加监听事件，并判断触发进入触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        /// <param name="broadcast">是否需要检测触发</param>
        static void AddColliderListener(this AOIUnitComponent self,AOITrigger trigger,bool broadcast = true)
        {
            var len = self.Scene.gridLen;
            int count = (int)Mathf.Ceil(trigger.Radius / len);
            if (count > 2) Log.Info("检测范围超过2格，触发半径："+ trigger.Radius);
            var pos = trigger.GetRealPos();
            var rot = trigger.GetRealRot();
            if (!broadcast)
            {
                using (var grids = self.GetNearbyGrid(count))
                {
                    for (int i = 0; i < grids.Count; i++)
                    {
                        var item = grids[i];
                        var flag = item.IsIntersectWithTrigger(trigger,pos,rot);
                        if (flag) //格子在范围有重叠部分
                        {
                            item.AddTriggerListener(trigger);
                        }
                    }
                }
            }
            else
            {
                using (var grids = self.GetNearbyGrid(count))
                {
                    HashSetComponent<AOITrigger> temp2 = HashSetComponent<AOITrigger>.Create();
                    for (int i = 0; i < grids.Count; i++)
                    {
                        var item = grids[i];
                        var flag = item.IsIntersectWithTrigger(trigger,pos,rot);
                        // Log.Info("grids pos "+item.posx+" "+item.posy+" flag"+flag);
                        if (flag) //格子在范围有重叠部分
                        {
                            item.AddTriggerListener(trigger);
                            //自己进入别人
                            if (trigger.IsCollider)
                            {
                                var unitType = trigger.GetParent<AOIUnitComponent>().Type;
                                for (int j =  item.Triggers.Count-1; j >=0; j--)
                                {
                                    var collider = item.Triggers[j];
                                    if (collider.IsDisposed)
                                    {
                                        item.Triggers.RemoveAt(j);
                                        Log.Warning("自动移除不成功");
                                        continue;
                                    }
                                    if (collider == trigger) continue;
                                    if (!collider.Selecter.Contains(unitType)) continue;
                                    if (collider.Flag != AOITriggerType.Enter && collider.Flag != AOITriggerType.All)
                                        continue;
                                    if (!temp2.Contains(collider) && collider.IsInTrigger(trigger,
                                            collider.GetRealPos(),
                                            collider.GetRealRot(), trigger.GetRealPos(), trigger.GetRealRot()))
                                    {
                                        // Log.Info("grids pos " + item.posx + " " + item.posy);
                                        temp2.Add(collider);
                                    }
                                }
                            }
                        }
                    }
                    
                    foreach (var item in temp2)
                    {
                        item.OnTrigger(trigger, AOITriggerType.Enter);
                    }
                    
                    temp2.Dispose();
                }
            }
        }
        /// <summary>
        /// 添加球形触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="radius">半径</param>a
        /// <param name="flag">监听进出类型</param>
        /// <param name="handler">当触发发生事件</param>
        /// <param name="selecter">筛选AOI类型</param>
        /// <returns></returns>
        public static AOITrigger AddSphereTrigger(this AOIUnitComponent self, float radius, AOITriggerType flag, 
            Action<AOIUnitComponent, AOITriggerType> handler, params UnitType[] selecter)
        {
            #region 数据初始化
            var trigger = self.AddTrigger(radius, flag, handler,selecter);
            #endregion

            #region 添加监听事件，并判断触发进入触发器

            self.AddTriggerListener(trigger, flag);
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加球形碰撞器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="radius">半径</param>a
        /// <returns></returns>
        public static AOITrigger AddSphereCollider(this AOIUnitComponent self, float radius)
        {
            if (self.Collider != null)
            {
                Log.Error("添加Collider时，Collider已存在");
                return null;
            }
            #region 数据初始化
            var trigger = self.AddCollider(radius);
            #endregion

            #region 添加监听事件，并判断触发进入触发器

            self.AddColliderListener(trigger);
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加球形触发器不触发检测
        /// </summary>
        /// <param name="self"></param>
        /// <param name="radius">半径</param>a
        /// <param name="flag">监听进出类型</param>
        /// <param name="handler">当触发发生事件</param>
        /// <param name="selecter">筛选AOI类型</param>
        /// <returns></returns>
        public static AOITrigger AddSphereTriggerWithoutBroadcast(this AOIUnitComponent self, float radius, AOITriggerType flag, 
            Action<AOIUnitComponent, AOITriggerType> handler, params UnitType[] selecter)
        {
            #region 数据初始化
            var trigger = self.AddTrigger(radius, flag, handler, selecter);
            #endregion

            #region 添加监听事件

            self.AddTriggerListener(trigger, flag,false);
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加球形碰撞器不触发检测
        /// </summary>
        /// <param name="self"></param>
        /// <param name="radius">半径</param>a
        /// <returns></returns>
        public static AOITrigger AddSphereColliderWithoutBroadcast(this AOIUnitComponent self, float radius)
        {
            if (self.Collider != null)
            {
                Log.Error("添加Collider时，Collider已存在");
                return null;
            }
            #region 数据初始化
            var trigger = self.AddCollider(radius);
            #endregion

            #region 添加监听事件

            self.AddColliderListener(trigger,false);
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加立方体触发
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scale">长宽高</param>
        /// <param name="flag">监听进出类型</param>
        /// <param name="handler">当触发发生事件</param>
        /// <param name="selecter">筛选AOI类型</param>
        /// <returns></returns>
        public static AOITrigger AddOBBTrigger(this AOIUnitComponent self, Vector3 scale, AOITriggerType flag,
            Action<AOIUnitComponent, AOITriggerType> handler, params UnitType[] selecter)
        {
            float radius = Mathf.Sqrt(scale.x*scale.x+scale.y*scale.y+scale.z*scale.z)/2;
            var trigger = self.AddTrigger(radius, flag, handler,selecter);
            trigger.AddComponent<OBBComponent, Vector3>(scale);
            trigger.TriggerType=TriggerShapeType.Cube;
            #region 添加监听事件，并判断触发进入触发器

            self.AddTriggerListener(trigger, flag);
            
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加立方体碰撞器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scale">长宽高</param>
        /// <returns></returns>
        public static AOITrigger AddOBBCollider(this AOIUnitComponent self, Vector3 scale)
        {
            if (self.Collider != null)
            {
                Log.Error("添加Collider时，Collider已存在");
                return null;
            }
            float radius = Mathf.Sqrt(scale.x*scale.x+scale.y*scale.y+scale.z*scale.z)/2;
            var trigger = self.AddCollider(radius);
            trigger.AddComponent<OBBComponent, Vector3>(scale);
            trigger.TriggerType=TriggerShapeType.Cube;
            #region 添加监听事件，并判断触发进入触发器

            self.AddColliderListener(trigger);
            
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加立方体触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scale">长宽高</param>
        /// <param name="flag">监听进出类型</param>
        /// <param name="handler">当触发发生事件</param>
        /// <param name="selecter">筛选AOI类型</param>
        /// <returns></returns>
        public static AOITrigger AddOBBTriggerWithoutBroadcast(this AOIUnitComponent self, Vector3 scale, AOITriggerType flag,
            Action<AOIUnitComponent, AOITriggerType> handler, params UnitType[] selecter)
        {
            float radius = Mathf.Sqrt(scale.x*scale.x+scale.y*scale.y+scale.z*scale.z)/2;
            var trigger = self.AddTrigger(radius, flag, handler,selecter);
            trigger.AddComponent<OBBComponent, Vector3>(scale);
            trigger.TriggerType=TriggerShapeType.Cube;
            #region 添加监听事件，并判断触发进入触发器

            self.AddTriggerListener(trigger, flag,false);
            
            #endregion
            return trigger;
        }
        /// <summary>
        /// 添加立方体碰撞器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="scale">长宽高</param>
        /// <returns></returns>
        public static AOITrigger AddOBBColliderWithoutBroadcast(this AOIUnitComponent self, Vector3 scale)
        {
            if (self.Collider != null)
            {
                Log.Error("添加Collider时，Collider已存在");
                return null;
            }
            float radius = Mathf.Sqrt(scale.x*scale.x+scale.y*scale.y+scale.z*scale.z)/2;
            var trigger = self.AddCollider(radius);
            trigger.AddComponent<OBBComponent, Vector3>(scale);
            trigger.TriggerType=TriggerShapeType.Cube;
            #region 添加监听事件，并判断触发进入触发器

            self.AddColliderListener(trigger,false);
            
            #endregion
            return trigger;
        }
        
        /// <summary>
        /// 移除碰撞器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        public static void RemoverTrigger(this AOIUnitComponent self, AOITrigger trigger)
        {
            self.SphereTriggers.Remove(trigger);
            self.RemoveTriggerListener(trigger);
            trigger.Dispose();
        }
        /// <summary>
        /// 移除监听事件，并判断触发离开触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        public static void RemoveTriggerListener(this AOIUnitComponent self, AOITrigger trigger)
        {
            #region 移除监听事件，并判断触发离开触发器
            var len = self.Scene.gridLen;
            int count = (int)Mathf.Ceil(trigger.Radius / len);
            if (count > 2) Log.Info("检测范围超过2格，触发半径："+ trigger.Radius);
            HashSetComponent<AOITrigger> temp = HashSetComponent<AOITrigger>.Create();
            for (int i = trigger.FollowCell.Count-1; i >=0 ; i--)
            {
                var item = trigger.FollowCell[i];
                item.RemoveTriggerListener(trigger);
                //离开触发器
                if (trigger.Flag == AOITriggerType.All || trigger.Flag == AOITriggerType.Exit)//注意不能放前面判断
                {
                    using (var colliders = item.GetAllCollider(trigger.Selecter,trigger))
                    {
                        for (int j = 0; j < colliders.Count; j++)
                        {
                            var collider = colliders[j];
                            if (collider.Parent.Id == self.Id) continue;
                            if (trigger.IsInTrigger(collider,trigger.GetRealPos(),
                                    trigger.GetRealRot(),collider.GetRealPos(),collider.GetRealRot()))
                            {
                                temp.Add(collider);
                            }
                        }
                    }
                }                   
            }
            foreach (var item in temp)
            {
                trigger.OnTrigger(item,AOITriggerType.Exit);
            }
            temp.Dispose();
            
            #endregion
        }
        /// <summary>
        /// 移除碰撞器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="collider"></param>
        public static void RemoverCollider(this AOIUnitComponent self, AOITrigger collider)
        {
            self.SphereTriggers.Remove(collider);
            self.RemoverColliderListener(collider);
            collider.Dispose();
        }

        /// <summary>
        /// 移除监听事件，并判断触发离开触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="collider"></param>
        public static void RemoverColliderListener(this AOIUnitComponent self, AOITrigger collider)
        {
            #region 移除监听事件，并判断触发离开触发器
            var len = self.Scene.gridLen;
            int count = (int)Mathf.Ceil(collider.Radius / len);
            if (count > 2) Log.Info("检测范围超过2格，触发半径："+ collider.Radius);
            HashSetComponent<AOITrigger> temp = HashSetComponent<AOITrigger>.Create();
            for (int i = collider.FollowCell.Count-1; i >=0 ; i--)
            {
                var item = collider.FollowCell[i];
                item.RemoveTriggerListener(collider);
                //离开触发器
                for (int j = 0; j < item.Triggers.Count; j++)
                {
                    var trigger = item.Triggers[j];
                    if (collider.Parent.Id == self.Parent.Id) continue;
                    if (trigger.Flag!=AOITriggerType.Exit&&trigger.Flag!=AOITriggerType.All&&trigger.IsInTrigger(collider,trigger.GetRealPos(),
                            trigger.GetRealRot(),collider.GetRealPos(),collider.GetRealRot()))
                    {
                        temp.Add(collider);
                    }
                }
                           
            }
            foreach (var item in temp)
            {
                item.OnTrigger(collider,AOITriggerType.Exit);
            }
            temp.Dispose();
            #endregion
        }
        /// <summary>
        /// 触发器自己坐标改变后，看别人有没有进来或离开
        /// </summary>
        /// <param name="self"></param>
        /// <param name="beforePosition"></param>
        /// <param name="changeCell">是否跨格子</param>
        public static void AfterTriggerChangeBroadcastToMe(this AOITrigger self, Vector3 beforePosition,
            bool changeCell)
        {
            if (self.IsCollider) return;
            var unit = self.GetParent<AOIUnitComponent>();
            var len = unit.Scene.gridLen;
            int count = (int) Mathf.Ceil(self.Radius / len);
            if (count > 2) Log.Info("检测范围超过2格，触发半径：" + self.Radius);
            Quaternion beforeRotation = self.GetRealRot();
            HashSetComponent<AOITrigger> pre; //之前有的
            HashSetComponent<AOITrigger> after; //现在有的
            var nowPos = self.GetRealPos();
            var cell = unit.Cell;

            if (!changeCell && self.FollowCell.Count == 1 && cell.xMin + self.Radius < nowPos.x &&
                cell.xMax - self.Radius > nowPos.x
                && cell.yMin + self.Radius < nowPos.z && cell.yMax - self.Radius > nowPos.z) //大多数情况,在本格子内移动
            {
                using (var colliders = cell.GetAllCollider(self.Selecter, self))
                {
                    if (colliders.Count <= 0) return;
                    pre = HashSetComponent<AOITrigger>.Create(); //之前有的
                    after = HashSetComponent<AOITrigger>.Create(); //现在有的
                    for (int i = colliders.Count - 1; i >= 0; i--)
                    {
                        var collider = colliders[i];
                        if (collider.IsDisposed)
                        {
                            cell.Colliders.RemoveAt(i);
                            Log.Warning("自动移除不成功");
                            continue;
                        }

                        if (collider.Parent.Id == self.Parent.Id) continue;

                        if (!after.Contains(collider) && self.IsInTrigger(collider,
                                self.GetRealPos(beforePosition),
                                beforeRotation, collider.GetRealPos(), collider.GetRealRot()))
                        {
                            after.Add(collider);
                            // Log.Info(" after.Add "+collider.Id);
                        }

                        if (!pre.Contains(collider) && self.IsInTrigger(collider, self.GetRealPos(),
                                self.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                        {
                            pre.Add(collider);
                            // Log.Info(" pre.Add "+collider.Id);
                        }
                    }
                }
            }
            else
            {
                DictionaryComponent<AOICell, int> triggers = DictionaryComponent<AOICell, int>.Create();
                pre = HashSetComponent<AOITrigger>.Create(); //之前有的
                after = HashSetComponent<AOITrigger>.Create(); //现在有的
                for (int i = 0; i < self.FollowCell.Count; i++)
                {
                    //旧的
                    triggers.Add(self.FollowCell[i], -1);
                }


                using (var grids = unit.Scene.GetNearbyGrid(count, nowPos))
                {
                    //新的
                    for (int i = 0; i < grids.Count; i++)
                    {
                        var item = grids[i];
                        var flag = item.IsIntersectWithTrigger(self, nowPos, self.GetRealRot());
                        if (flag) //格子在范围内部
                        {
                            if (triggers.ContainsKey(item))
                                triggers[item]++;
                            else
                                triggers.Add(item, 1);
                        }
                        // Log.Info("new "+flag+" "+ item.posx+","+item.posy);
                    }
                }

                #region 筛选格子里的单位

                //不完全包围的格子需要逐个计算
                foreach (var item in triggers)
                {
                    if (item.Value > 0) //之前无现在有
                    {
                        item.Key.AddTriggerListener(self);
                        using (var colliders = item.Key.GetAllCollider(self.Selecter, self))
                        {
                            for (int i = 0; i < colliders.Count; i++)
                            {
                                var collider = colliders[i];
                                if (collider.Parent.Id == self.Parent.Id) continue;
                                if (!pre.Contains(collider) && self.IsInTrigger(collider, self.GetRealPos(),
                                        self.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                                {
                                    pre.Add(collider);
                                }

                            }
                        }
                    }
                    else if (item.Value < 0) //之前有现在无
                    {
                        item.Key.RemoveTriggerListener(self);
                        using (var colliders = item.Key.GetAllCollider(self.Selecter, self))
                        {
                            for (int i = 0; i < colliders.Count; i++)
                            {
                                var collider = colliders[i];
                                if (collider.Parent.Id == self.Parent.Id) continue;
                                if (!after.Contains(collider) && self.IsInTrigger(collider,
                                        self.GetRealPos(beforePosition),
                                        beforeRotation, collider.GetRealPos(), collider.GetRealRot()))
                                {
                                    after.Add(collider);
                                }

                            }
                        }
                    }
                    else //之前有现在有，但坐标变了
                    {
                        using (var colliders = item.Key.GetAllCollider(self.Selecter, self))
                        {
                            for (int i = 0; i < colliders.Count; i++)
                            {
                                var collider = colliders[i];
                                if (collider.Parent.Id == self.Parent.Id) continue;
                                if (!after.Contains(collider) && self.IsInTrigger(collider,
                                        self.GetRealPos(beforePosition),
                                        beforeRotation, collider.GetRealPos(), collider.GetRealRot()))
                                {
                                    after.Add(collider);
                                }

                                if (!pre.Contains(collider) && self.IsInTrigger(collider, self.GetRealPos(),
                                        self.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                                {
                                    pre.Add(collider);
                                }
                            }
                        }
                    }

                }

                triggers.Dispose();
            }

            if (pre.Count > 0 || after.Count > 0)
            {
                DictionaryComponent<AOITrigger, int> colliderDic =
                    DictionaryComponent<AOITrigger, int>.Create();
                foreach (var collider in pre)
                {
                    colliderDic.Add(collider, 1);
                }

                foreach (var collider in after)
                {
                    if (colliderDic.ContainsKey(collider))
                    {
                        colliderDic[collider]--;
                    }
                    else
                    {
                        colliderDic.Add(collider, -1);
                    }

                }

                pre.Dispose();
                after.Dispose();

                #endregion

                //判断事件
                foreach (var item in colliderDic)
                {
                    if (self == item.Key) continue;
                    if (item.Value < 0 && (self.Flag == AOITriggerType.All || self.Flag == AOITriggerType.Exit)) //离开
                    {
                        self.OnTrigger(item.Key, AOITriggerType.Exit);
                    }
                    else if (item.Value > 0 &&
                             (self.Flag == AOITriggerType.All || self.Flag == AOITriggerType.Enter)) //进入
                    {
                        self.OnTrigger(item.Key, AOITriggerType.Enter);
                    }

                }

                colliderDic.Dispose();
            }
            else
            {
                pre.Dispose();
                after.Dispose();
            }
        }


        /// <summary>
        /// 触发器自己改变方向后，看别人有没有进来或离开
        /// </summary>
        /// <param name="self"></param>
        /// <param name="before"></param>
        public static void AfterTriggerChangeRotationBroadcastToMe(this AOITrigger self,Quaternion before)
        {
            if (self.IsCollider) return;
            if (self.TriggerType==TriggerShapeType.Sphere) return;
            var unit = self.GetParent<AOIUnitComponent>();
            var len = unit.Scene.gridLen;
            int count = (int)Mathf.Ceil(self.Radius / len);
            if (count > 2) Log.Info("检测范围超过2格，触发半径："+ self.Radius);
            DictionaryComponent<AOICell,int> triggers = DictionaryComponent<AOICell, int>.Create();

            for (int i = 0; i < self.FollowCell.Count; i++)
            {
                //旧的
                triggers.Add(self.FollowCell[i], -1);
            }

            var nowPos = self.GetRealPos(unit.Position);
            using (var grids = unit.Scene.GetNearbyGrid(count,nowPos))
            {
                //新的
                for (int i = 0; i < grids.Count; i++)
                {
                    var item = grids[i];
                    var flag = item.IsIntersectWithTrigger(self,nowPos,self.GetRealRot());
                    if ( flag)//格子在范围内部
                    {
                        if (triggers.ContainsKey(item))
                            triggers[item]++;
                        else
                            triggers.Add(item,1);
                    }
                    // Log.Info("new "+flag+" "+ item.posx+","+item.posy);
                }
            }

            #region 筛选格子里的单位
            HashSetComponent<AOITrigger> pre = HashSetComponent<AOITrigger>.Create();//之前有的
            HashSetComponent<AOITrigger> after = HashSetComponent<AOITrigger>.Create();//现在有的
            //完全包围的格子不需要逐个计算
            foreach (var item in triggers)
            {
                if (item.Value > 0)
                {
                    item.Key.AddTriggerListener(self);
                    for (int i = item.Key.Colliders.Count-1; i >=0; i--)
                    {
                        var collider = item.Key.Colliders[i];
                        if (collider.IsDisposed)
                        {
                            item.Key.Colliders.RemoveAt(i);
                            Log.Warning("自动移除不成功");
                            continue;
                        }
                        if (collider.Parent.Id == self.Parent.Id) continue;
                        pre.Add(collider);
                    }
                }
                else if (item.Value < 0)
                {
                    item.Key.RemoveTriggerListener(self);
                    for (int i = item.Key.Colliders.Count-1; i >=0; i--)
                    {
                        var collider = item.Key.Colliders[i];
                        if (collider.IsDisposed)
                        {
                            item.Key.Colliders.RemoveAt(i);
                            Log.Warning("自动移除不成功");
                            continue;
                        }
                        if (collider.Parent.Id == self.Parent.Id) continue;
                        after.Add(collider);
                    }
                }
            }
            //不完全包围的格子需要逐个计算
            foreach (var item in triggers)
            {
                if (item.Value > 0)//之前无现在有
                {
                    item.Key.AddTriggerListener(self);
                    for (int i = item.Key.Colliders.Count-1; i >=0; i--)
                    {
                        var collider = item.Key.Colliders[i];
                        if (collider.IsDisposed)
                        {
                            item.Key.Colliders.RemoveAt(i);
                            Log.Warning("自动移除不成功");
                            continue;
                        }
                        if (collider.Parent.Id == self.Parent.Id) continue;
                        if (!pre.Contains(collider)&&collider.Selecter.Contains(unit.Type)&&self.IsInTrigger(collider,self.GetRealPos(),
                                self.GetRealRot(),collider.GetRealPos(),collider.GetRealRot()))
                        {
                            pre.Add(collider);
                        }
                            
                    }
                }
                else if (item.Value < 0)//之前有现在无
                {
                    item.Key.RemoveTriggerListener(self);
                    for (int i = item.Key.Colliders.Count-1; i >=0; i--)
                    {
                        var collider = item.Key.Colliders[i];
                        if (collider.IsDisposed)
                        {
                            item.Key.Colliders.RemoveAt(i);
                            Log.Warning("自动移除不成功");
                            continue;
                        }
                        if (collider.Parent.Id == self.Parent.Id) continue;
                        if (!after.Contains(collider)&&collider.Selecter.Contains(unit.Type)&&self.IsInTrigger(collider,self.GetRealPos(),
                                before,collider.GetRealPos(),collider.GetRealRot()))
                        {
                            after.Add(collider);
                        }
                            
                    }
                }
                else//之前有现在有，但坐标变了
                {
                    for (int i = item.Key.Colliders.Count - 1; i >= 0; i--)
                    {
                        var collider = item.Key.Colliders[i];
                        if (collider.IsDisposed)
                        {
                            item.Key.Colliders.RemoveAt(i);
                            Log.Warning("自动移除不成功");
                            continue;
                        }
                        if (collider.Parent.Id == self.Parent.Id) continue;
                        if (!after.Contains(collider) && collider.Selecter.Contains(unit.Type) && self.IsInTrigger(
                                collider, self.GetRealPos(),
                                before, collider.GetRealPos(), collider.GetRealRot()))
                        {
                            after.Add(collider);
                        }

                        if (!pre.Contains(collider) && collider.Selecter.Contains(unit.Type) && self.IsInTrigger(
                                collider, self.GetRealPos(),
                                self.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                        {
                            pre.Add(collider);
                        }
                    }
                }

                
            }
            triggers.Dispose();
            if (pre.Count > 0 || after.Count > 0)
            {
                DictionaryComponent<AOITrigger, int> colliderDic =
                    DictionaryComponent<AOITrigger, int>.Create();
                foreach (var collider in pre)
                {
                    colliderDic.Add(collider, -1);
                }

                foreach (var collider in after)
                {
                    if (colliderDic.ContainsKey(collider))
                    {
                        colliderDic[collider]++;
                    }
                    else
                    {
                        colliderDic.Add(collider, 1);
                    }

                }

                pre.Dispose();
                after.Dispose();

                #endregion

                //判断事件
                foreach (var item in colliderDic)
                {
                    if (self == item.Key) continue;
                    if (item.Value < 0) //离开
                    {
                        if (self.IsCollider &&
                            (item.Key.Flag == AOITriggerType.All || item.Key.Flag == AOITriggerType.Exit))
                            item.Key.OnTrigger(self, AOITriggerType.Exit);
                        else if (item.Key.IsCollider &&
                                 (self.Flag == AOITriggerType.All || self.Flag == AOITriggerType.Exit))
                            self.OnTrigger(item.Key, AOITriggerType.Exit);
                    }
                    else if (item.Value > 0) //进入
                    {
                        if (self.IsCollider &&
                            (item.Key.Flag == AOITriggerType.All || item.Key.Flag == AOITriggerType.Enter))
                            item.Key.OnTrigger(self, AOITriggerType.Enter);
                        else if (item.Key.IsCollider &&
                                 (self.Flag == AOITriggerType.All || self.Flag == AOITriggerType.Enter))
                            self.OnTrigger(item.Key, AOITriggerType.Enter);
                    }

                }
                colliderDic.Dispose();
            }
            else
            {
                pre.Dispose();
                after.Dispose();
            }
        }
        /// <summary>
        /// 触发器自己坐标方向改变后，有没有进来或离开别人
        /// </summary>
        /// <param name="self"></param>
        /// <param name="beforePosition"></param>
        /// <param name="beforeRotation"></param>
        /// <param name="changeCell">是否跨格子</param>
        public static void AfterColliderChangeBroadcastToOther(this AOITrigger self,Vector3 beforePosition,Quaternion beforeRotation,bool changeCell)
        {
            if(!self.IsCollider) return;
            var unit = self.GetParent<AOIUnitComponent>();
            HashSetComponent<AOITrigger> pre; //之前有的
            HashSetComponent<AOITrigger> after; //现在有的
            
            var nowPos = self.GetRealPos();
            var cell = unit.Cell;
            if (!changeCell&&self.FollowCell.Count==1&&cell.xMin+self.Radius<nowPos.x &&cell.xMax-self.Radius>nowPos.x
                &&cell.yMin+self.Radius<nowPos.z &&cell.yMax-self.Radius>nowPos.z)//大多数情况,在本格子内移动
            {
                if(cell.Triggers.Count<=0) return;
                pre = HashSetComponent<AOITrigger>.Create(); //之前有的
                after = HashSetComponent<AOITrigger>.Create(); //现在有的
                for (int i =cell.Triggers.Count-1; i >=0; i--)
                {
                    var collider = cell.Triggers[i];
                    if (collider.IsDisposed)
                    {
                        cell.Triggers.RemoveAt(i);
                        Log.Warning("自动移除不成功");
                        continue;
                    }
                    if (collider.Parent.Id == self.Parent.Id) continue;
                    if (!after.Contains(collider)&&collider.Selecter.Contains(unit.Type)&&self.IsInTrigger(collider,self.GetRealPos(beforePosition),
                            beforeRotation,collider.GetRealPos(),collider.GetRealRot()))
                    {
                        after.Add(collider);
                        // Log.Info(" after.Add "+collider.Id);
                    }
                    if (!pre.Contains(collider)&&collider.Selecter.Contains(unit.Type)&&self.IsInTrigger(collider,self.GetRealPos(),
                            self.GetRealRot(),collider.GetRealPos(),collider.GetRealRot()))
                    {
                        pre.Add(collider);
                        // Log.Info(" pre.Add "+collider.Id);
                    }
                }
            }
            else
            {
                pre = HashSetComponent<AOITrigger>.Create(); //之前有的
                after = HashSetComponent<AOITrigger>.Create(); //现在有的
                int count = (int) Mathf.Ceil(self.Radius / unit.Scene.gridLen);
                if (count > 2) Log.Info("检测范围超过2格，触发半径：" + self.Radius);
                DictionaryComponent<AOICell, int> triggers = DictionaryComponent<AOICell, int>.Create();
                for (int i = 0; i < self.FollowCell.Count; i++)
                {
                    //旧的
                    triggers.Add(self.FollowCell[i], -1);
                }
                using (var grids = unit.Scene.GetNearbyGrid(count, nowPos))
                {
                    //新的
                    for (int i = 0; i < grids.Count; i++)
                    {
                        var item = grids[i];
                        var flag = item.IsIntersectWithTrigger(self, nowPos, self.GetRealRot());
                        if (flag) //格子在范围内部
                        {
                            if (triggers.ContainsKey(item))
                                triggers[item]++;
                            else
                                triggers.Add(item, 1);
                        }
                    }
                }
                #region 筛选格子里的单位
                //不完全包围的格子需要逐个计算
                foreach (var item in triggers)
                {
                    if (item.Value > 0) //之前无现在有
                    {
                        item.Key.AddTriggerListener(self);
                        for (int i = item.Key.Triggers.Count - 1; i >= 0; i--)
                        {
                            var collider = item.Key.Triggers[i];
                            if (collider.IsDisposed)
                            {
                                item.Key.Triggers.RemoveAt(i);
                                Log.Warning("自动移除不成功");
                                continue;
                            }
                            if (collider.Parent.Id == self.Parent.Id) continue;
                            if (!pre.Contains(collider) && collider.Selecter.Contains(unit.Type) && self.IsInTrigger(
                                    collider, self.GetRealPos(),
                                    self.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                            {
                                pre.Add(collider);
                                // Log.Info(" pre.Add "+collider.Id);
                            }

                        }
                    }
                    else if (item.Value < 0) //之前有现在无
                    {
                        item.Key.RemoveTriggerListener(self);
                        for (int i = item.Key.Triggers.Count - 1; i >= 0; i--)
                        {
                            var collider = item.Key.Triggers[i];
                            if (collider.IsDisposed)
                            {
                                item.Key.Triggers.RemoveAt(i);
                                Log.Warning("自动移除不成功");
                                continue;
                            }
                            if (collider.Parent.Id == self.Parent.Id) continue;
                            if (!after.Contains(collider) && collider.Selecter.Contains(unit.Type) && self.IsInTrigger(
                                    collider, self.GetRealPos(beforePosition),
                                    beforeRotation, collider.GetRealPos(), collider.GetRealRot()))
                            {
                                after.Add(collider);
                                // Log.Info(" after.Add "+collider.Id);
                            }

                        }
                    }
                    else //之前有现在有，但坐标变了
                    {
                        for (int i = item.Key.Triggers.Count - 1; i >= 0; i--)
                        {
                            var collider = item.Key.Triggers[i];
                            if (collider.IsDisposed)
                            {
                                item.Key.Triggers.RemoveAt(i);
                                Log.Warning("自动移除不成功");
                                continue;
                            }
                            if (collider.Parent.Id == self.Parent.Id) continue;
                            if (!after.Contains(collider) && collider.Selecter.Contains(unit.Type) && self.IsInTrigger(
                                    collider, self.GetRealPos(beforePosition),
                                    beforeRotation, collider.GetRealPos(), collider.GetRealRot()))
                            {
                                after.Add(collider);
                                // Log.Info(" after.Add "+collider.Id);
                            }

                            if (!pre.Contains(collider) && collider.Selecter.Contains(unit.Type) && self.IsInTrigger(
                                    collider, self.GetRealPos(),
                                    self.GetRealRot(), collider.GetRealPos(), collider.GetRealRot()))
                            {
                                pre.Add(collider);
                                // Log.Info(" pre.Add "+collider.Id);
                            }
                        }
                    }


                }
                triggers.Dispose();
                #endregion
            }

            if (pre.Count > 0 || after.Count > 0)
            {
                DictionaryComponent<AOITrigger, int> colliderDic =
                    DictionaryComponent<AOITrigger, int>.Create();
                foreach (var collider in pre)
                {
                    colliderDic.Add(collider, 1);
                }

                foreach (var collider in after)
                {
                    if (colliderDic.ContainsKey(collider))
                    {
                        colliderDic[collider]--;
                    }
                    else
                    {
                        colliderDic.Add(collider, -1);
                    }

                }
                pre.Dispose();
                after.Dispose();

                //判断事件
                foreach (var item in colliderDic)
                {
                    // Log.Info(" colliderDic "+item.Key.Id+"  "+item.Value);
                    if (self == item.Key) continue;
                    if (item.Value < 0 &&
                        (item.Key.Flag == AOITriggerType.All || item.Key.Flag == AOITriggerType.Exit)) //离开
                    {
                        item.Key.OnTrigger(self, AOITriggerType.Exit);
                    }
                    else if (item.Value > 0 &&
                             (item.Key.Flag == AOITriggerType.All || item.Key.Flag == AOITriggerType.Enter)) //进入
                    {
                        item.Key.OnTrigger(self, AOITriggerType.Enter);
                    }

                }

                colliderDic.Dispose();
            }
            else
            {
                pre.Dispose();
                after.Dispose();
            }
           
        }
        /// <summary>
        /// 判断是否触发
        /// </summary>
        /// <param name="trigger1"></param>
        /// <param name="trigger2"></param>
        /// <param name="position1"></param>
        /// <param name="rotation1"></param>
        /// <param name="position2"></param>
        /// <param name="rotation2"></param>
        /// <returns></returns>
        public static bool IsInTrigger(this AOITrigger trigger1, AOITrigger trigger2,
            Vector3 position1, Quaternion rotation1, Vector3 position2, Quaternion rotation2)
        {
            if(trigger1==null||trigger2==null)
            {
                return false;
            }
            if (!trigger1.IsCollider && !trigger2.IsCollider)//至少一个为碰撞器
            {
                return false;
            }
            // Log.Info("IsInTrigger");
            var pos1 = trigger1.GetRealPos(position1);
            var pos2 = trigger1.GetRealPos(position2);
            var sqrDis = Vector3.SqrMagnitude(pos1- pos2);
            // Log.Info("dis"+dis+"pos1"+pos1+"pos2"+pos2+"trigger1.Radius"+trigger1.Radius+"trigger2.Radius"+trigger2.Radius);
            var dis = trigger1.Radius + trigger2.Radius;
            bool isSphereTrigger = dis*dis > sqrDis;
            if (trigger1.TriggerType == TriggerShapeType.Sphere && trigger2.TriggerType == TriggerShapeType.Sphere)//判断球触发
            {
                // Log.Info("判断球触发");
                return isSphereTrigger;
            }
            if (!isSphereTrigger) return false;//外接球不相交
            if (trigger1.TriggerType == TriggerShapeType.Cube && trigger2.TriggerType == TriggerShapeType.Cube)//判断OBB触发
            {
                return trigger1.IsInTrigger(trigger2, pos1, rotation1, pos2, rotation2);
            }
            else if(trigger1.TriggerType <= TriggerShapeType.Cube && trigger2.TriggerType <= TriggerShapeType.Cube)//判断OBB和球触发
            {
                // Log.Info("判断OBB和球触发");
                var triggerOBB = trigger1;
                var triggerSphere = trigger2;
                var posOBB = pos1;
                var posSp = pos2;
                var rotOBB = rotation1;
                if (trigger2.TriggerType == TriggerShapeType.Cube)
                {
                    triggerOBB = trigger2;
                    triggerSphere = trigger1;
                    posOBB = pos2;
                    posSp = pos1;
                    rotOBB = rotation2;
                }
                var obb = triggerOBB.GetComponent<OBBComponent>();
                Vector3 temp = Quaternion.Inverse(rotOBB)*(posSp - posOBB); //转换到触发器模型空间坐标
                var xMax = obb.Scale.x / 2;
                var yMax = obb.Scale.y / 2;
                var zMax = obb.Scale.z / 2;
                if (-xMax <= temp.x && temp.x <= xMax && -yMax <= temp.y && temp.y <= yMax &&
                    -zMax <= temp.z && temp.z <= zMax)//球心在立方体内
                {
                    // Log.Info("球心在立方体内");
                    return true;
                }

                if (-xMax - triggerSphere.Radius > temp.x || temp.x > xMax + triggerSphere.Radius ||
                    -yMax - triggerSphere.Radius > temp.y || temp.y > yMax + triggerSphere.Radius ||
                    -zMax - triggerSphere.Radius > temp.z || temp.z > zMax + triggerSphere.Radius)//球心离立方体超过半径
                {
                    // Log.Info("球心离立方体超过半径 xMax"+xMax+" yMax"+yMax+" zMax"+zMax+" Radius"+triggerSphere.Radius+" temp"+temp);
                    return false;
                }
                // Log.Info("一个轴出立方");
                //一个轴出立方
                if (-xMax <= temp.x && temp.x <= xMax && -yMax <= temp.y && temp.y <= yMax)//z方向不在立方体内
                {
                    return -zMax - triggerSphere.Radius <= temp.z && temp.z <= zMax + triggerSphere.Radius;
                }
                if (-yMax <= temp.y && temp.y <= yMax && -zMax <= temp.z && temp.z <= zMax)//x方向不在立方体内
                {
                    return -xMax - triggerSphere.Radius <= temp.x && temp.x <= xMax + triggerSphere.Radius;
                }
                if (-xMax <= temp.x && temp.x <= xMax && -zMax <= temp.z && temp.z <= zMax)//y方向不在立方体内
                {
                    return -yMax - triggerSphere.Radius <= temp.y && temp.y <= yMax + triggerSphere.Radius;
                }

                #region 两个轴出立方

                // Log.Info("两个轴出立方");
                //两个轴出立方
                if (-xMax <= temp.x && temp.x <= xMax)
                {
                    if (-yMax > temp.y&& -zMax > temp.z)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(temp.x, -yMax, -zMax)) <= triggerSphere.SqrRadius;
                    }
                    if (-yMax > temp.y&& temp.z > zMax)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(temp.x, -yMax, zMax)) <= triggerSphere.SqrRadius;
                    }
                    if (temp.y > yMax && -zMax > temp.z)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(temp.x, yMax, -zMax)) <= triggerSphere.SqrRadius;
                    }
                    if (temp.y > yMax && temp.z > zMax)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(temp.x, yMax, zMax)) <= triggerSphere.SqrRadius;
                    }
                }
                //两个轴出立方
                if (-yMax <= temp.y && temp.y <= yMax)
                {
                    if (-xMax > temp.x&& -zMax > temp.z)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(-xMax, temp.y, -zMax)) <= triggerSphere.SqrRadius;
                    }
                    if (-xMax > temp.x&& temp.z > zMax)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(-xMax, temp.y, zMax)) <= triggerSphere.SqrRadius;
                    }
                    if (temp.x > xMax && -zMax > temp.z)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(xMax, temp.y, -zMax)) <= triggerSphere.SqrRadius;
                    }
                    if (temp.x > xMax && temp.z > zMax)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(xMax, temp.y, zMax)) <= triggerSphere.SqrRadius;
                    }
                }
                //两个轴出立方
                if (-zMax <= temp.z && temp.z <= zMax)
                {
                    if (-yMax > temp.y&& -xMax > temp.x)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(-xMax, -yMax, temp.z)) <= triggerSphere.SqrRadius;
                    }
                    if (-yMax > temp.y&& temp.x > xMax)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(xMax, -yMax, temp.z)) <= triggerSphere.SqrRadius;
                    }
                    if (temp.y > yMax && -xMax > temp.x)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(-xMax, yMax, temp.z)) <= triggerSphere.SqrRadius;
                    }
                    if (temp.y > yMax && temp.x > xMax)
                    {
                        return Vector3.SqrMagnitude(temp- new Vector3(xMax, yMax, temp.z)) <= triggerSphere.SqrRadius;
                    }
                }
                #endregion
                Log.Info("离8个角较近的位置");
                //离8个角较近的位置
                using (var points = obb.GetAllVertex(posOBB,rotOBB))
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (Vector3.SqrMagnitude(temp- points[i]) > triggerSphere.SqrRadius)
                        {
                            return false;
                        }
                    }
                }
                return true;

            }
            else//未处理
            {
                Log.Error("未处理的触发器触发判断，类型 trigger1.TriggerType="+trigger1.TriggerType+"; trigger2.TriggerType="+trigger2.TriggerType);
            }

            return false;
        }
        
        /// <summary>
        /// 判断某个点是否在触发器移到指定位置后之内
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="position"></param>
        /// <param name="center"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsPointInTrigger(this AOITrigger trigger, Vector3 position,Vector3 center,Quaternion rotation)
        {
            var sqrDis = Vector3.SqrMagnitude(center- position);
            if (trigger.SqrRadius < sqrDis) return false;
            if (trigger.TriggerType==TriggerShapeType.Cube)
            {
                var obb = trigger.GetComponent<OBBComponent>();
                return AOIHelper.IsPointInTrigger(position, center, rotation, obb.Scale);
            }
            return true;
        }


        /// <summary>
        /// 判断射线是否在触发器移到指定位置后之内
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="ray"></param>
        /// <param name="center"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsRayInTrigger(this AOITrigger trigger,Ray ray, Vector3 center,Quaternion rotation)
        {
            //求点到直线的距离
            var dis = Math.Sqrt(Vector3.Cross(ray.Start - center, ray.Dir).sqrMagnitude / ray.Dir.sqrMagnitude);
            if (dis > trigger.Radius) return false;
            var disHitStartSqr = (ray.Start - center).sqrMagnitude - dis * dis;
            var hit = ray.Start + (float)(Math.Sqrt(disHitStartSqr)-Math.Sqrt(trigger.Radius * trigger.Radius - dis * dis))
                *ray.Dir;
            if (ray.Distance * ray.Distance< disHitStartSqr||Vector3.SqrMagnitude(hit- center) - trigger.SqrRadius > 0.1)
            {
                return false;//即使距离小于半径，由于是射线也要判断一下
            }
            if (trigger.TriggerType == TriggerShapeType.Cube)
            {
                return trigger.GetComponent<OBBComponent>().IsRayInTrigger(ray, center, rotation,out hit);
            }
            return true;
        }
        
        /// <summary>
        /// 判断射线是否在触发器移到指定位置后之内
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="ray"></param>
        /// <param name="center"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsRayInTrigger(this AOITrigger trigger,Ray ray, Vector3 center,Quaternion rotation,out Vector3 hit)
        {
            hit = Vector3.zero;
            //求点到直线的距离
            var dis = Math.Sqrt(Vector3.Cross(ray.Start - center, ray.Dir).sqrMagnitude / ray.Dir.sqrMagnitude);
            if (dis > trigger.Radius) return false;
            var disHitStartSqr = (ray.Start - center).sqrMagnitude - dis * dis;
            hit = ray.Start + (float)(Math.Sqrt(disHitStartSqr)-Math.Sqrt(trigger.Radius * trigger.Radius - dis * dis))
                *ray.Dir;
            if (ray.Distance * ray.Distance< disHitStartSqr||Vector3.SqrMagnitude(hit- center) > trigger.SqrRadius)
            {
                return false;//即使距离小于半径，由于是射线也要判断一下
            }
            if (trigger.TriggerType == TriggerShapeType.Cube)
            {
                return trigger.GetComponent<OBBComponent>().IsRayInTrigger(ray, center, rotation,out hit);
            }
            return true;
        }
    }
}