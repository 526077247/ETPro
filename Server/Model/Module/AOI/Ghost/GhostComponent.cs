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
            {typeof(M2C_PathfindingResult),typeof(M2M_PathfindingResult)},
            {typeof(M2C_Stop),typeof(M2M_Stop)},
            {typeof(M2C_UseSkill),typeof(M2M_UseSkill)},
            {typeof(M2C_AddBuff),typeof(M2M_AddBuff)},
            {typeof(M2C_Damage),typeof(M2M_Damage)}
        };
    }
}