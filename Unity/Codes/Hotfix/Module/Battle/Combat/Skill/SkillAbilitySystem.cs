using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class SkillAbilityAwakeSystem : AwakeSystem<SkillAbility,int>
    {
        public override void Awake(SkillAbility self, int a)
        {
            self.ConfigId = a;
            self.Groups = new Dictionary<string, long>();
            self.LastSpellOverTime = TimeInfo.Instance.ServerNow()-self.SkillConfig.CDTime;
            self.LastSpellTime = TimeInfo.Instance.ServerNow()-self.SkillConfig.CDTime;
            var groups = SkillStepConfigCategory.Instance.GetSkillGroups(self.ConfigId); 
            for (int i = 0; i < groups.Count; i++)
            {
                var group = self.AddChild<SkillAbilityGroup, int>(groups[i].Id);
                self.Groups.Add(groups[i].Group,group.Id);
            }
        }
    }
    [FriendClass(typeof(SkillAbility))]
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
        public static SkillAbilityGroup GetGroup(this SkillAbility self,string group)
        {
            if (self.Groups.TryGetValue(group, out var res))
            {
                return self.GetChild<SkillAbilityGroup>(res);
            }

            return null;
        }
        
        public static SkillAbilityGroup GetCurGroup(this SkillAbility self)
        {
            if (self.Groups.TryGetValue(self.CurGroupId, out var res))
            {
                return self.GetChild<SkillAbilityGroup>(res);
            }

            return null;
        }
    }
}