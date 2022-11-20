namespace ET
{
    /// <summary>
    /// 给自己加BUFF
    /// </summary>
    [SkillWatcher(SkillStepType.AddBuff)]
    public class SkillWatcher_AddBuff : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
#if SERVER //纯客户端单机游戏去掉
            var unit = para.From.unit;
            if(unit.IsGhost()) return;//纯客户端单机游戏去掉
            var stepPara = para.GetCurSkillStepPara();
            Log.Info("SkillWatcher_AddBuff");
            if (stepPara.Paras.Length >= 2)
            {
                if (StepParaHelper.TryParseInt(ref stepPara.Paras[0], out var buffId))
                {
                    StepParaHelper.TryParseInt(ref stepPara.Paras[1], out var time);
                    var bc = unit.GetComponent<CombatUnitComponent>().GetComponent<BuffComponent>();
                    if (bc != null)
                    {
                        bc.AddBuff(buffId, TimeHelper.ServerNow() + time, unit.Id);
                    }
                }
            }
#endif
        }
        
    }
}