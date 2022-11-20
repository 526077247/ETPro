using System.Collections.Generic;

namespace ET
{
    public partial class SkillStepConfigCategory
    {
        public MultiMap<int, SkillStepConfig> groups;
        public MultiDictionary<int, string, SkillStepConfig> idGroupMap;
        public override void AfterEndInit()
        {
            base.AfterEndInit();
            groups = new MultiMap<int, SkillStepConfig>();
            idGroupMap = new MultiDictionary<int, string, SkillStepConfig>();
            for (int i = 0; i < list.Count; i++)
            {
                groups.Add(list[i].SkillId,list[i]);
                idGroupMap.Add(list[i].SkillId,this.list[i].Group,list[i]);
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
        
        public SkillStepConfig GetSkillGroup(int skillId,string group)
        {
            if(idGroupMap.TryGetValue(skillId,group,out var res))
            {
                return res;
            }
            return null;
        }
    }
}