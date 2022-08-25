using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    [FriendClass(typeof(Unit))]
    [FriendClass(typeof(MoveComponent))]
    [FriendClass(typeof(NumericComponent))]
    [FriendClass(typeof(BuffComponent))]
    [FriendClass(typeof(Buff))]
    [FriendClass(typeof(CombatUnitComponent))]
    public static class UnitHelper
    {
        public static UnitInfo CreateUnitInfo(Unit unit)
        {
            UnitInfo unitInfo = new UnitInfo();
            
            unitInfo.UnitId = unit.Id;
            unitInfo.ConfigId = unit.ConfigId;
            unitInfo.Type = (int)unit.Type;
            Vector3 position = unit.Position;
            unitInfo.X = position.x;
            unitInfo.Y = position.y;
            unitInfo.Z = position.z;
            Vector3 forward = unit.Forward;
            unitInfo.ForwardX = forward.x;
            unitInfo.ForwardY = forward.y;
            unitInfo.ForwardZ = forward.z;

            #region 移动信息
            MoveComponent moveComponent = unit.GetComponent<MoveComponent>();
            if (moveComponent != null)
            {
                if (!moveComponent.IsArrived())
                {
                    unitInfo.MoveInfo = new MoveInfo();
                    for (int i = moveComponent.N; i < moveComponent.Targets.Count; ++i)
                    {
                        Vector3 pos = moveComponent.Targets[i];
                        unitInfo.MoveInfo.X.Add(pos.x);
                        unitInfo.MoveInfo.Y.Add(pos.y);
                        unitInfo.MoveInfo.Z.Add(pos.z);
                    }
                }
            }
            

            #endregion

            #region 数值信息

            NumericComponent nc = unit.GetComponent<NumericComponent>();
            if(nc!=null)
            {
                foreach ((int key, long value) in nc.NumericDic)
                {
                    if (key > NumericType.Max) //不需要同步最终值
                    {
                        unitInfo.Ks.Add(key);
                        unitInfo.Vs.Add(value);
                    }
                }
            }
            #endregion

            #region 战斗数据

            var cuc = unit.GetComponent<CombatUnitComponent>();
            if (cuc != null)
            {
                //技能
                unitInfo.SkillIds.AddRange(cuc.IdSkillMap.Keys);
                var buffC = cuc.GetComponent<BuffComponent>();
                if (buffC != null)
                {
                    for (int i = 0; i < buffC.AllBuff.Count; i++)
                    {
                        var buff = buffC.GetChild<Buff>(buffC.AllBuff[i]);
                        unitInfo.BuffIds.Add(buff.ConfigId);
                        unitInfo.BuffTimestamp.Add(buff.Timestamp);
                        unitInfo.BuffSourceIds.Add(buff.FromUnitId);
                    }
                }
            }
            
            #endregion
           
            
            return unitInfo;
        }
        
        /// <summary>
        /// 获取看见unit的玩家，主要用于广播,注意不能Dispose
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<AOIUnitComponent> GetBeSeeUnits(this Unit self)
        {
            return self.GetComponent<AOIUnitComponent>().GetBeSeeUnits();
        }
        
        public static void NoticeUnitAdd(Unit unit, Unit sendUnit)
        {
            M2C_CreateUnits createUnits = new M2C_CreateUnits();
            createUnits.Units.Add(CreateUnitInfo(sendUnit));
            MessageHelper.SendToClient(unit, createUnits);
        }
        
        public static void NoticeUnitRemove(Unit unit, Unit sendUnit)
        {
            M2C_RemoveUnits removeUnits = new M2C_RemoveUnits();
            removeUnits.Units.Add(sendUnit.Id);
            MessageHelper.SendToClient(unit, removeUnits);
        }
        
        public static void NoticeUnitsAdd(Unit unit, List<AOIUnitComponent> sendUnit)
        {
            M2C_CreateUnits createUnits = new M2C_CreateUnits();
            for (int i = 0; i < sendUnit.Count; i++)
            {
                if (unit.Id == sendUnit[i].Id) continue;
                createUnits.Units.Add(CreateUnitInfo(sendUnit[i].GetParent<Unit>()));
            }

            if (createUnits.Units.Count > 0)
            {
                MessageHelper.SendToClient(unit, createUnits);
            }
            
        }
        
        public static void NoticeUnitsRemove(Unit unit, List<long> sendUnit)
        {
            M2C_RemoveUnits removeUnits = new M2C_RemoveUnits();
            removeUnits.Units = sendUnit;
            MessageHelper.SendToClient(unit, removeUnits);
        }
    }
}