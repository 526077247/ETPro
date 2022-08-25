using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{

    /// <summary>
    /// 主动技能的预览组件，预览功能不是所有游戏都会有，moba类游戏一般会有技能预览的功能，部分mmorpg游戏也可能有，回合制、卡牌、arpg动作游戏一般没有
    /// </summary>
    [ComponentOf(typeof(CombatUnitComponent))]
    public class SpellPreviewComponent : Entity,IAwake<Dictionary<int,int>>,IAwake,IInput,IDestroy
    {
#if SERVER //单机去掉
        public SpellComponent SpellComp => parent.GetComponent<SpellComponent>();//选位置和方向的距离不够直接最大施法距离施法，选目标的则不施法
        
#endif
        public MoveAndSpellComponent MoveAndSpellComp => parent.GetComponent<MoveAndSpellComponent>();//距离不够则走过去施法
        public bool Previewing;
        public SkillAbility PreviewingSkill { get; set; }

        public Entity CurSelect;
        public Dictionary<int, SkillAbility> InputSkills { get;  } = new Dictionary<int, SkillAbility>();

        public bool Enable;
    }
}
