using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    public class UIRedDotComponent:Entity,IAwake,IOnCreate<string>,IOnCreate<string,Vector2>,IOnEnable
    {
        public GameObject TempObj;
        public RedDotMonoView unity_target;
        public Vector3 Scaler;
        public Vector2 PositionOffset;
        public string Target;
        public bool isRedDotActive = false;
    }
}
