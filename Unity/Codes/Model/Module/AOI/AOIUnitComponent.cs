using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    /// <summary>1
    /// 使用时注意 平面是x,z；竖直方向是y
    /// </summary>
    [ComponentOf(typeof(Unit))]
    public class AOIUnitComponent:Entity,IAwake<Vector3,Quaternion,UnitType,int>,IAwake<Vector3,Quaternion,UnitType>,
            IAwake<Vector3,Quaternion,UnitType,int,bool>,IAwake<Vector3,Quaternion,UnitType,bool>,IDestroy
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public AOISceneComponent Scene{ get; set; }
        public UnitType Type { get; set; }
        private AOICell cell;
        public AOICell Cell
        {
            get =>this.cell;
            set
            {
                EventSystem.Instance.Publish(new EventType.ChangeGrid()
                {
                    Unit = this,
                    NewCell = value,
                    OldCell = this.cell
                });
                this.cell = value;
            }
        }

        public int Range;
        public readonly List<AOITrigger> SphereTriggers = new List<AOITrigger>();//自己的触发器
        // public float MaxTriggerRadius=0;
        public AOITrigger Collider
        {
            get
            {
                for (int i = 0; i < SphereTriggers.Count; i++)
                {
                    if (SphereTriggers[i].IsCollider) 
                        return SphereTriggers[i];
                }

                return null;
            }
        }
    }
}
