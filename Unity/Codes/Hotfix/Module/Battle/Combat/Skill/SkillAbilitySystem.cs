using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class SkillAbilityAwakeSystem : AwakeSystem<SkillAbility,int>
    {
        public override void Awake(SkillAbility self, int a)
        {
            self.ConfigId = a;
            SkillStepComponent.Instance.GetSkillStepInfo(self.ConfigId,out self.TimeLine,out self.StepType,out self.Paras);
                   
        }
    }
    public static class SkillAbilitySystem
    {
        /// <summary>
        /// 是否可用
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool CanUse(this SkillAbility self)
        {
            return true;
        }
    }
}