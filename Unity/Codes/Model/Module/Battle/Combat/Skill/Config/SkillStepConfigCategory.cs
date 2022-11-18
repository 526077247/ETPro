using System.Collections.Generic;

namespace ET
{
    public partial class SkillStepConfigCategory
    {
        public MultiMap<int, SkillStepConfig> groups;
        public override void AfterEndInit()
        {
            base.AfterEndInit();
            groups = new MultiMap<int, SkillStepConfig>();
            for (int i = 0; i < list.Count; i++)
            {
                groups.Add(list[i].SkillId,list[i]);
            }
        }

        public List<SkillStepConfig> GetSkillGroups(int skillId)
        {
            if(groups.TryGetValue(skillId,out var res))
            {
                return res;
            }
            return null;
        }
    }
}