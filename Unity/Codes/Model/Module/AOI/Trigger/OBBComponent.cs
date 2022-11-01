using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ET
{
    /// <summary>
    /// 立方体触发器(有个外接球，先判断是否在外接球里,再判断是否在立方体里)
    /// </summary>
    [ComponentOf(typeof(AOITrigger))]
    public class OBBComponent:Entity,IAwake<Vector3>,IDestroy
    {
        /// <summary>
        /// 长宽高
        /// </summary>
        public Vector3 Scale;
        
        public class TempPosRot
        {
            public Vector3 Pos;
            public Quaternion Rot;
        }

        public TempPosRot LastVertexPosRot;
        public ListComponent<Vector3> LastVertex;
        public TempPosRot LastSidesPosRot;
        public ListComponent<Ray> LastSides;
    }
}