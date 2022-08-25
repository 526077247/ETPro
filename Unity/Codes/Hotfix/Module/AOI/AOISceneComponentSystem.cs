using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using ET.EventType;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class AOISceneComponentAwakeSystem : AwakeSystem<AOISceneComponent, int>
    {
        public override void Awake(AOISceneComponent self, int gridLen)
        {
            self.gridLen = gridLen;
            self.halfDiagonal = self.gridLen*0.7072f;
            Log.Info("AOIScene StandBy! ");
#if SERVER
            var id = (int)self.Id;
            if (MapSceneConfigCategory.Instance.GetAll().TryGetValue(id, out var config)&&!string.IsNullOrEmpty(config.Area))
            {
                self.AddComponent<AreaComponent, string>(config.Area);
            }
#endif
        }
    }

    [ObjectSystem]
    public class AOISceneComponentDestroySystem : DestroySystem<AOISceneComponent>
    {
        public override void Destroy(AOISceneComponent self)
        {
            Log.Info("AOIScene Destroy! ");
        }
    }
    [FriendClass(typeof(AOISceneComponent))]
    [FriendClass(typeof(AOIUnitComponent))]
    [FriendClass(typeof(AOICell))]
    public static class AOISceneComponentSystem
    {
        /// <summary>
        /// 找到指定位置所在的Grid
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        /// <param name="create">没有是否创建</param>
        public static AOICell GetAOICell(this AOISceneComponent self,Vector3 pos,bool create = true)
        {
            int xIndex = (int)pos.x / self.gridLen;
            int yIndex = (int)pos.z / self.gridLen;
            
            return self.GetCell(xIndex,yIndex,create);
        }

        /// <summary>
        /// 注册一个 AOI 对象, 同时设置其默认 AOI 半径。注：每个对象都有一个默认的 AOI 半径，凡第一次进入半径范围的其它物体，都会触发 AOI 消息。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static async ETTask RegisterUnit(this AOISceneComponent self, AOIUnitComponent unit)
        {
            unit.Scene = self;
            AOICell cell = self.GetAOICell(unit.Position);
#if SERVER
            if (!unit.IsGhost()&&cell.TryGetCellMap(out var sceneId) && !cell.IsCurScene())
            {
                TransferHelper.AreaCreate(unit, StartSceneConfigCategory.Instance.Get(sceneId).InstanceId);
            }
#endif
            cell.Add(unit);
            // Log.Info("RegisterUnit:" + unit.Id + "  Position:" + unit.Position + "  grid x:"+ cell.posx+",y:"+ cell.posy+" type"+unit.Type);
            if (unit.Type == UnitType.Player)
            {
                using (var ListenerGrids = cell.GetNearbyGrid(unit.Range))
                {
                    for (int i = 0; i < ListenerGrids.Count; i++)
                    {
                        var item = ListenerGrids[i];
                        item.AddListener(unit);
#if SERVER
                        if(!item.IsCurScene()) continue;
#endif
                        using (var list = item.GetAllUnit())
                        {
                            Game.EventSystem.Publish(new AOIRegisterUnit()
                            {
                                Receive = unit,
                                Units = list
                            });
                        }

                    }
                }
            }
            await ETTask.CompletedTask;
        }

        /// <summary>
        /// 删除一个 AOI 对象 ,同时有可能触发它相对其它 AOI 对象的离开消息。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static void RemoveUnit(this AOISceneComponent self, AOIUnitComponent unit)
        {
            // Log.Info("RemoveUnit:" + unit.Id);
            unit.Scene = null;
            if (unit.Cell != null)
            {
                using (var ListenerGrids = unit.Cell.GetNearbyGrid(unit.Range))
                {
                    if (unit.Type == UnitType.Player)
                    {
                        for (int i = 0; i < ListenerGrids.Count; i++)
                        {
                            var item = ListenerGrids[i];
                            item.RemoveListener(unit);
#if SERVER
                            if(!item.IsCurScene()) continue;
#endif
                            using (var list = item.GetAllUnit())
                            {
                                Game.EventSystem.Publish(new AOIRemoveUnit()
                                {
                                    Receive = unit,
                                    Units = list
                                });
                            }
                        }
                    }
                }
                unit.Cell.Remove(unit);
            }
            
        }
        
        /// <summary>
        /// 获取指定位置为中心指定圈数的所有格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="turnNum"></param>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        /// <returns></returns>
        public static ListComponent<AOICell> GetNearbyGrid(this AOISceneComponent self,int turnNum,int posx,int posy)
        {
            ListComponent<AOICell> res = ListComponent<AOICell>.Create();
            for (int i = 0; i <= turnNum*2+1; i++)
            {
                var x = posx - turnNum + i;
                for (int j = 0; j <= turnNum * 2 + 1; j++)
                {
                    var y = posy - turnNum + j;
                    res.Add(self.GetCell(x,y));
                }
            }
            return res;
        }
        
        private static AOICell GetCell(this AOISceneComponent self, int x,int y,bool create = true)
        {
            long cellId = AOIHelper.CreateCellId(x, y);
            AOICell cell = self.GetChild<AOICell>(cellId);
            if (cell == null && create)
            {
                cell = self.AddChildWithId<AOICell>(cellId);
                cell.xMin = x * self.gridLen;
                cell.xMax = cell.xMin + self.gridLen;
                cell.yMin = y * self.gridLen;
                cell.yMax = cell.yMin + self.gridLen;
                cell.posx = x;
                cell.posy = y;
                cell.halfDiagonal = self.halfDiagonal;
            }

            return cell;
        }
        /// <summary>
        /// 获取指定位置为中心指定圈数的所有格子
        /// </summary>
        /// <param name="self"></param>
        /// <param name="turnNum"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static ListComponent<AOICell> GetNearbyGrid(this AOISceneComponent self,int turnNum,Vector3 pos)
        {
            var grid = self.GetAOICell(pos);
            ListComponent<AOICell> res = ListComponent<AOICell>.Create();
            for (int i = 0; i <= turnNum*2+1; i++)
            {
                var x = grid.posx - turnNum + i;
                for (int j = 0; j <= turnNum * 2 + 1; j++)
                {
                    var y = grid.posy - turnNum + j;
                    res.Add(self.GetCell(x,y));
                }
            }
            return res;
        }
    }
}
