namespace ET
{
    /// <summary>
    /// 被打断
    /// </summary>
    [SkillWatcher(SkillStepType.Interrupt)]
    [FriendClass(typeof(SkillAbility))]
    public class SkillWatcher_Interrupt : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
            Log.Info("SkillWatcher_Interrupt");
            para.Ability.LastSpellOverTime = TimeHelper.ServerNow();
        }
    }
}