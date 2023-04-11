using System;
using UnityEngine;
using UnityEngine.UI;
namespace ET
{
    [ComponentOf(typeof(SpellPreviewComponent))]
    public class TargetSelectComponent:Entity,IAwake,IDestroy,IUpdate,IShow<Action<Unit>,int[]>,IHide,IInput,IAutoSpell<Action<Unit>,int[]>
    {
        public ETTask<GameObject> waiter;
        public Action<Unit> OnSelectTargetCallback { get; set; }
        public int TargetLimitType { get; set; }
        public Color CursorColor { get; set; }
        public GameObject HeroObj;
        public GameObject RangeCircleObj;
        public Image CursorImage;
        public GameObject gameObject;
        public bool IsShow;
        public int Distance;
        /// <summary>
        /// 施法模式（0：距离不够则选最大施法范围ps选目标的则不施法;1:距离不够走到最远距离施法）
        /// </summary>
        public int Mode { get; set; }

    }
}