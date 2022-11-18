using System.Collections.Generic;

namespace ET
{
    
    [FriendClass(typeof(SkillAbilityGroup))]
    public static class SkillAbilityGroupSystem
    {
        public class AwakeSystem : AwakeSystem<SkillAbilityGroup,int>
        {
            public override void Awake(SkillAbilityGroup self, int a)
            {
                self.ConfigId = a;
            }
        }
        
        public static List<int> GetTimeLine(this SkillAbilityGroup self)
        {
            return SkillStepComponent.Instance.GetSkillStepTimeLine(self.ConfigId);
        }
        
        public static List<int> GetStepType(this SkillAbilityGroup self)
        {
            return SkillStepComponent.Instance.GetSkillStepType(self.ConfigId);
        }
        
        public static List<object[]> GetParas(this SkillAbilityGroup self)
        {
            return SkillStepComponent.Instance.GetSkillStepParas(self.ConfigId);
        }
    }
}