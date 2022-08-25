using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class GlobalComponent: Entity, IAwake
    {
        public static GlobalComponent Instance;
        
        public Transform Global;
        public Transform Unit { get; set; }
        
        public Transform Scene { get; set; }

        public bool ColliderDebug { get; set; }
    }
}