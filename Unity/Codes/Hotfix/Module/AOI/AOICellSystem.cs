using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class AOIGridAwakeSystem : AwakeSystem<AOICell>
    {
        public override void Awake(AOICell self)
        {
            self.typeUnits = new Dictionary<UnitType, List<AOIUnitComponent>>();
            for (int i = 0; i < (int)UnitType.MAX; i++)
            {
                self.typeUnits.Add((UnitType)i, new List<AOIUnitComponent>());
            }
            self.Triggers = new List<AOITrigger>();
            self.ListenerUnits = new List<AOIUnitComponent>();
            self.Colliders = new List<AOITrigger>();
        }
    }
    [ObjectSystem]
    public class AOIGridDestroySystem : DestroySystem<AOICell>
    {
        public override void Destroy(AOICell self)
        {
            self.typeUnits.Clear();
            self.Triggers.Clear();
            self.ListenerUnits.Clear();
            self.Colliders.Clear();
            self.typeUnits = null;
            self.Triggers = null;
            self.ListenerUnits = null;
            self.Colliders = null;
        }
    }
    [FriendClass(typeof(AOICell))]
    [FriendClass(typeof(AOITrigger))]
    [FriendClass(typeof(AOIUnitComponent))]
    [FriendClass(typeof(OBBComponent))]
    public static class AOICellSystem
    {
        /// <summary>
        /// 获取与碰撞器的关系：-1无关 0相交或包括碰撞器 1在碰撞器内部
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static int GetRelationshipWithTrigger(this AOICell self, AOITrigger trigger,Vector3 position ,Quaternion rotation)
        {
            var len = self.GetParent<AOISceneComponent>().gridLen;
            if (trigger.TriggerType == TriggerShapeType.Cube)
            {
                var obb = trigger.GetComponent<OBBComponent>();
                return AOIHelper.GetGridRelationshipWithOBB(position, rotation,obb.Scale,len,self.xMin,
                    self.yMin,trigger.Radius,trigger.SqrRadius);
            }
            else
            {
                return AOIHelper.GetGridRelationshipWithSphere(position,trigger.Radius,len,self.xMin,
                    self.yMin,trigger.SqrRadius);
            }
            
        }
        /// <summary>
        /// 获取与碰撞器的关系：false无关 true相交
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static bool IsIntersectWithTrigger(this AOICell self, AOITrigger trigger,Vector3 position ,Quaternion rotation)
        {
            var len = self.GetParent<AOISceneComponent>().gridLen;
            return AOIHelper.IsGridIntersectWithSphere(position,trigger.Radius,len,self.xMin,
                self.yMin,trigger.SqrRadius);
        }
        /// <summary>
        /// 添加触发器监视
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static void AddTriggerListener(this AOICell self, AOITrigger trigger)
        {
            if (Define.Debug)
            {
                if (!trigger.DebugMap.ContainsKey(self)) trigger.DebugMap[self] = 0;
                trigger.DebugMap[self]++;
                trigger.LogInfo.Add("AddTriggerListener "+self.posx+","+self.posy+"  "+trigger.GetRealPos()+"  "+
                                    DateTime.Now.ToString("HH:mm:ss fff:ffffff")+"\r\n"+new StackTrace());
            }
            trigger.FollowCell.Add(self);
            if(trigger.IsCollider)
                self.Colliders.Add(trigger);
            else
                self.Triggers.Add(trigger);
#if SERVER
            var ghost = trigger.Parent.GetComponent<GhostComponent>();
            if (ghost!=null&&self.TryGetCellMap(out int sceneId))
            {
                ghost.AddListenerAreaIds(sceneId);
            }
#endif
        }
        /// <summary>
        /// 移除触发器监视
        /// </summary>
        /// <param name="self"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static void RemoveTriggerListener(this AOICell self, AOITrigger trigger)
        {
            if (Define.Debug)
            {
                if (!trigger.DebugMap.ContainsKey(self)) trigger.DebugMap[self] = 0;
                trigger.DebugMap[self]--;
                trigger.LogInfo.Add("RemoveTriggerListener "+self.posx+","+self.posy+"  "+trigger.GetRealPos()+"  "+
                                    DateTime.Now.ToString("HH:mm:ss fff:ffffff")+"\r\n"+new StackTrace());
            }
            if (self.IsDisposed) return;
            trigger.FollowCell.Remove(self);
            if(trigger.IsCollider)
                self.Colliders.Remove(trigger);
            else
                self.Triggers.Remove(trigger);
#if SERVER
            var ghost = self.Parent.GetComponent<GhostComponent>();
            if (ghost!=null&&self.TryGetCellMap(out int sceneId))
            {
                ghost.RemoveListenerAreaIds(sceneId);
            }
#endif
        }
        /// <summary>
        /// 添加监视
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static void AddListener(this AOICell self, AOIUnitComponent unit)
        {
            // Log.Info("AddListener"+unit.Id+" "+self.posx+","+self.posy);
            self.ListenerUnits.Add(unit);
#if SERVER
            var ghost = unit.GetComponent<GhostComponent>();
            if (ghost!=null&&self.TryGetCellMap(out int sceneId))
            {
                ghost.AddListenerAreaIds(sceneId);
            }
#endif
        }
        /// <summary>
        /// 移除监视
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static void RemoveListener(this AOICell self, AOIUnitComponent unit)
        {
            // Log.Info("RemoveListener"+unit.Id+" "+self.posx+","+self.posy);
            self.ListenerUnits.Remove(unit);
#if SERVER
            var ghost = unit.GetComponent<GhostComponent>();
            if (ghost!=null&&self.TryGetCellMap(out int sceneId))
            {
                ghost.RemoveListenerAreaIds(sceneId);
            }
#endif
        }
        
        /// <summary>
        /// 进入格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static void Add(this AOICell self, AOIUnitComponent unit)
        {
            unit.Cell = self;
            if (Define.Debug&&self.typeUnits[unit.Type].Contains(unit))//Debug开启检测
            {
                Log.Error("self.idUnits[unit.Type].Contains(unit)");
            }
            self.typeUnits[unit.Type].Add(unit);
            ListComponent<AOIUnitComponent> list = ListComponent<AOIUnitComponent>.Create();
            list.Add(unit);
            for (int i = 0; i < self.ListenerUnits.Count; i++)
            {
                var item = self.ListenerUnits[i];
                if (item.Type == UnitType.Player)
                {
                    Game.EventSystem.Publish(new EventType.AOIRegisterUnit()
                    {
                        Receive = item,
                        Units = list
                    });
                }
            }
            list.Dispose();
        }

        /// <summary>
        /// 离开
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static void Remove(this AOICell self, AOIUnitComponent unit)
        {
            if (self == null || self.IsDisposed) return;
            ListComponent<AOIUnitComponent> list = ListComponent<AOIUnitComponent>.Create();
            list.Add(unit);
            if (self.typeUnits.ContainsKey(unit.Type))
            {
                for (int i = 0; i < self.ListenerUnits.Count; i++)
                {
                    var item = self.ListenerUnits[i];
                    if (item.Type == UnitType.Player)
                    {
                        Game.EventSystem.Publish(new EventType.AOIRemoveUnit()
                        {
                            Receive = item,
                            Units = list
                        });
                    }
                }
                self.typeUnits[unit.Type].Remove(unit);
                unit.Cell = null;
            }
            list.Dispose();
        }
        

        /// <summary>
        /// 获取所有指定类型单位
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ListComponent<AOIUnitComponent> GetAllUnit(this AOICell self, UnitType type = UnitType.ALL)
        {
            var res = ListComponent<AOIUnitComponent>.Create();
            if (type == UnitType.ALL)
            {
                foreach (var item in self.typeUnits)
                    res.AddRange(item.Value);
            }
            else if (self.typeUnits.ContainsKey(type))
            {
                res.AddRange(self.typeUnits[type]);
            }
            return res;
        }
        /// <summary>
        /// 获取所有指定类型单位
        /// </summary>
        /// <param name="self"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static ListComponent<AOIUnitComponent> GetAllUnit(this AOICell self, List<UnitType> types)
        {
            var res = ListComponent<AOIUnitComponent>.Create();
            var isAll = types.Contains(UnitType.ALL);
            foreach (var item in self.typeUnits)
                if (types.Contains(item.Key) || isAll)
                {
                    // Log.Info("GetAllUnit key:"+item.Key);
                    res.AddRange(item.Value);
                }
            return res;
        }
        /// <summary>
        /// 获取所有指定类型单位的碰撞器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="types"></param>
        /// <param name="except"></param>
        /// <returns></returns>
        public static ListComponent<AOITrigger> GetAllCollider(this AOICell self, List<UnitType> types,AOITrigger except)
        {
            var res = ListComponent<AOITrigger>.Create();
            if (self.IsDisposed) return res;
            var isAll = types.Contains(UnitType.ALL);
            for (int i = self.Colliders.Count-1; i >=0 ; i--)
            {
                var item = self.Colliders[i];
                if (item.IsDisposed)
                {
                    self.Colliders.RemoveAt(i);
                    Log.Warning("自动移除不成功");
                    continue;
                }
                if(!item.IsCollider||item==except) continue;
                
                if (isAll||types.Contains(item.GetParent<AOIUnitComponent>().Type))
                {
                    // Log.Info("GetAllUnit key:"+item.Key);
                    res.Add(item);
                }
            }
            return res;
        }

        /// <summary>
        /// 获取所有指定类型单位的触发器
        /// </summary>
        /// <param name="self"></param>
        /// <param name="types"></param>
        /// <param name="except"></param>
        /// <returns></returns>
        public static ListComponent<AOITrigger> GetAllTrigger(this AOICell self, List<UnitType> types,
            AOITrigger except)
        {
            var res = ListComponent<AOITrigger>.Create();
            if (self.IsDisposed) return res;
            var isAll = types.Contains(UnitType.ALL);
            for (int i = self.Colliders.Count-1; i >=0 ; i--)
            {
                var item = self.Colliders[i];
                if (item.IsDisposed)
                {
                    self.Colliders.RemoveAt(i);
                    Log.Warning("自动移除不成功");
                    continue;
                }
                if(item.IsCollider||item==except) continue;
                
                if (isAll||types.Contains(item.GetParent<AOIUnitComponent>().Type))
                {
                    // Log.Info("GetAllUnit key:"+item.Key);
                    res.Add(item);
                }
            }
            return res;
        }
        /// <summary>
        /// 获取自身为中心指定圈数的所有格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="turnNum">圈数</param>
        /// <returns></returns>
        public static ListComponent<AOICell> GetNearbyGrid(this AOICell self,int turnNum)
        {
            var scene = self.DomainScene().GetComponent<AOISceneComponent>();
            if (scene == null) return ListComponent<AOICell>.Create();
            return scene.GetNearbyGrid(turnNum, self.posx, self.posy);
        }

        /// <summary>
        /// 获取所有指定类型单位
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ListComponent<AOIUnitComponent> GetAllUnit(this ListComponent<AOICell> self, UnitType type = UnitType.ALL)
        {
            var res = ListComponent<AOIUnitComponent>.Create();
            for (int i = 0; i < self.Count; i++)
            {
                if (type == UnitType.ALL)
                    foreach (var item in self[i].typeUnits)
                        res.AddRange(item.Value);
                else if (self[i].typeUnits.ContainsKey(type))
                    res.AddRange(self[i].typeUnits[type]);
            }
            return res;
        }

        /// <summary>
        /// 获取自身为中心指定圈数的所有格子的所有指定类型单位
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ListComponent<AOIUnitComponent> GetNearbyUnit(this AOICell self, int turnNum, UnitType type = UnitType.ALL)
        {
            var grid = self.GetNearbyGrid(turnNum);
            if (grid != null)
            {
                var res = grid.GetAllUnit(type);
                grid.Dispose();
                return res;
            }
            return ListComponent<AOIUnitComponent>.Create();
        }
        
        #region Ghost
#if SERVER
        
        /// <summary>
        /// 获取当前格子所属场景
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sceneId"></param>
        /// <returns></returns>
        public static bool TryGetCellMap(this AOICell self,out int sceneId)
        {
            var areaComp = self.GetParent<AOISceneComponent>().GetComponent<AreaComponent>();
            if (areaComp == null)
            {
                sceneId = (int)self.DomainScene().Id;
                return true;
            }
            return areaComp.TryGetCellMap(self.Id, out sceneId);
        }

        /// <summary>
        /// 是否是当前场景的格子
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsCurScene(this AOICell self)
        {
            var areaComp = self.GetParent<AOISceneComponent>().GetComponent<AreaComponent>();
            if (areaComp == null)
            {
                return true;
            }
            if (areaComp.TryGetCellMap(self.Id, out int sceneId))
            {
                return sceneId == self.Parent.Id;
            }

            return false;
        }

        /// <summary>
        /// 是否是未开放地区
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsCloseCell(this AOICell self)
        {
            var areaComp = self.GetParent<AOISceneComponent>().GetComponent<AreaComponent>();
            if (areaComp == null)
            {
                return false;
            }
            return areaComp.TryGetCellMap(self.Id, out int _);
        }
#endif
        #endregion
    }
}
