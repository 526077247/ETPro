using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 影子（注意在实际项目中应通过在地图边缘设置不可通过地形、怪物位置远离地图边缘等方式尽量避免跨地图战斗）
    /// </summary>
    [ComponentOf(typeof(AOIUnitComponent))]
    public class GhostComponent :Entity,IAwake,IDestroy
    {
        public Dictionary<int,int> AreaIds { get; set; }
        public bool IsGoast { get; set; }

        public Vector3? LeavePos;
        public int RealAreaId=> (int) this.DomainScene().Id;
        /// <summary>
        /// 需要转发到其他Area的协议
        /// </summary>
        public static readonly Dictionary<Type, Type> MsgMap = new Dictionary<Type, Type>()
        {
            {TypeInfo<M2C_PathfindingResult>.Type,TypeInfo<M2M_PathfindingResult>.Type},
            {TypeInfo<M2C_Stop>.Type,TypeInfo<M2M_Stop>.Type},
            {TypeInfo<M2C_UseSkill>.Type,TypeInfo<M2M_UseSkill>.Type},
            {TypeInfo<M2C_AddBuff>.Type,TypeInfo<M2M_AddBuff>.Type},
            {TypeInfo<M2C_Damage>.Type,TypeInfo<M2M_Damage>.Type},
            {TypeInfo<M2C_ChangeSkillGroup>.Type,TypeInfo<M2M_ChangeSkillGroup>.Type},
            {TypeInfo<M2C_RemoveBuff>.Type,TypeInfo<M2M_RemoveBuff>.Type},
            {TypeInfo<M2C_Interrupt>.Type,TypeInfo<M2M_Interrupt>.Type}
        };
    }
}