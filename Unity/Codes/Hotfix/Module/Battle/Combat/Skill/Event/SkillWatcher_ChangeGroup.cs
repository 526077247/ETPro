namespace ET
{
    /// <summary>
    /// 判断切换Group
    /// </summary>
    [SkillWatcher(SkillStepType.ChangeGroup)]
    [FriendClass(typeof(SkillAbility))]
    public class SkillWatcher_ChangeGroup : ISkillWatcher
    {
        public void Run(SkillPara para)
        {

            if (para.GetCurSkillStepPara().Paras.Length == 0)
            {
                Log.Error(para.SkillConfigId+"判断切换Group参数数量不对"+para.GetCurSkillStepPara().Paras.Length);
                return;
            }
            
            var stepPara = para.GetCurSkillStepPara();
            var unit = para.From;
            var spell = unit.GetComponent<SpellComponent>();
            if (stepPara.Paras.Length == 1)
            {
                if(StepParaHelper.TryParseString(ref stepPara.Paras[0], out var group))
                {
                    spell.ChangeGroup(group);
                }
            }
            else if (stepPara.Paras.Length >= 2)
            {
#if SERVER//纯客户端单机游戏去掉
                if (unit.unit.IsGhost())
                {
                    spell.WaitStep(SkillStepType.ChangeGroup);
                    return;
                }
                if(StepParaHelper.TryParseString(ref stepPara.Paras[0], out var condition))
                {
                    var res = ConditionWatcherComponent.Instance.Run(condition,para);
                    M2C_ChangeSkillGroup msg = new M2C_ChangeSkillGroup {  UnitId = unit.Id, Result = res?1:0,Timestamp = TimeHelper.ServerNow()};
                    MessageHelper.Broadcast(unit.unit,msg);
                    StepParaHelper.TryParseString(ref stepPara.Paras[1], out var suc);
                    if (res)
                    {
                        spell.ChangeGroup(suc);
                    }
                    else if(stepPara.Paras.Length >= 3)
                    {
                        StepParaHelper.TryParseString(ref stepPara.Paras[2], out var fail);
                        spell.ChangeGroup(fail);
                    }
                }
#else
                spell.WaitStep(SkillStepType.ChangeGroup);
#endif
            }

        }
    }
}