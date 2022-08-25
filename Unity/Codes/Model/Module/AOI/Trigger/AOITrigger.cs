using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ET
{
    public static class TriggerShapeType
    {
        public const int Sphere = 0;//球
        public const int Cube = 1;//OBB
    }
    /// <summary>
    /// 球形触发器
    /// </summary>
    [ChildOf(typeof(AOIUnitComponent))]
    public class AOITrigger:Entity,IAwake<float,Action<AOIUnitComponent, AOITriggerType>>,IAwake<float>,IDestroy
    {
        public float Radius;
        public float SqrRadius;
        public AOITriggerType Flag;
        public ListComponent<UnitType> Selecter;
        public Action<AOIUnitComponent, AOITriggerType> Handler;
        public int TriggerType = TriggerShapeType.Sphere;//形状
        /// <summary>
        /// 是否“碰撞器”，碰撞器不挂触发事件
        /// </summary>
        public bool IsCollider{ get; set; }
        public DictionaryComponent<AOICell, int> DebugMap;
        public ListComponent<string> LogInfo;
        public float OffsetY;
        public ListComponent<AOICell> FollowCell;
        public bool Enable;
    }
}