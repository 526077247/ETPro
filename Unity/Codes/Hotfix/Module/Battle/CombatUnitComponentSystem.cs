using UnityEngine;
using System.Collections.Generic;
namespace ET
{
    [ObjectSystem]
    public class CombatUnitAwakeSystem : AwakeSystem<CombatUnitComponent,List<int>>
    {
        public override void Awake(CombatUnitComponent self,List<int> skills)
        {
            self.AddComponent<SpellComponent>();//技能施法组件
            for (int i = 0; i < skills.Count; i++)
            {
                self.AddSkill(skills[i]);
            }
            self.AddComponent<BuffComponent>();//buff容器组件
            self.AddComponent<MoveAndSpellComponent>();//移动过去施法组件
            EventSystem.Instance.Publish(new EventType.AfterCombatUnitComponentCreate
            {
                CombatUnitComponent = self
            });
        }
    }
    [ObjectSystem]
    public class CombatUnitAwakeSystem1 : AwakeSystem<CombatUnitComponent>
    {
        public override void Awake(CombatUnitComponent self)
        {
            self.AddComponent<SpellComponent>();//技能施法组件
            self.AddComponent<BuffComponent>();//buff容器组件
            self.AddComponent<MoveAndSpellComponent>();//移动过去施法组件
            EventSystem.Instance.Publish(new EventType.AfterCombatUnitComponentCreate
            {
                CombatUnitComponent = self
            });
        }
    }
    [ObjectSystem]
    public class CombatUnitDestroySystem : DestroySystem<CombatUnitComponent>
    {
        public override void Destroy(CombatUnitComponent self)
        {
            self.IdSkillMap.Clear();
        }
    }
    [FriendClass(typeof(CombatUnitComponent))]
    public static class CombatUnitComponentSystem
    {
        /// <summary>
        /// 添加技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="configId"></param>
        /// <returns></returns>
        public static SkillAbility AddSkill(this CombatUnitComponent self,int configId)
        {
            if (!self.IdSkillMap.ContainsKey(configId))
            {
                var skill = self.AddChild<SkillAbility, int>(configId);
                self.IdSkillMap.Add(configId, skill.Id);
            }
            return self.GetChild<SkillAbility>(self.IdSkillMap[configId]);
        }

        public static bool TryGetSkillAbility(this CombatUnitComponent self, int configId,out SkillAbility skill)
        {
            if (self.IdSkillMap.ContainsKey(configId))
            {
                skill = self.GetChild<SkillAbility>(self.IdSkillMap[configId]);
                return true;
            }
            skill = null;
            return false;
        }
    }
}