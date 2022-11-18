using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(SkillAbility))]
    public class SkillAbilityGroup:Entity,IAwake<int>,ITransfer
    {
        public int ConfigId { get; set; }

        public SkillStepConfig Config => SkillStepConfigCategory.Instance.Get(ConfigId);
    }
}