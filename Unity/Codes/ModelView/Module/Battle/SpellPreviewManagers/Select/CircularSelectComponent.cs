using System;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(SpellPreviewComponent))]
    public class CircularSelectComponent : Entity,IAwake,IUpdate,IDestroy,IShow<Action<Vector3>,int[]>,IHide,IInput,IAutoSpell<Action<Vector3>,int[]>
    {
        public ETTask<GameObject> waiter;
        public Action<Vector3> OnSelectPointCallback { get; set; }
        public GameObject HeroObj;
        public GameObject RangeCircleObj;
        public GameObject SkillPointObj;
        public GameObject gameObject;
        public bool IsShow;
        public int distance;
        public int range;
        public int Mode { get; set; }
    }
}