namespace ET
{
    /// <summary>
    /// 移除自己BUFF
    /// </summary>
    [SkillWatcher(SkillStepType.RemoveBuff)]
    public class SkillWatcher_RemoveBuff : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
#if SERVER //纯客户端单机游戏去掉
            var unit = para.From.unit;
            if(unit.IsGhost()) return;//纯客户端单机游戏去掉
            var stepPara = para.GetCurSkillStepPara();
            Log.Info("SkillWatcher_RemoveBuff");
            if (stepPara.Paras.Length >= 1)
            {
                if (StepParaHelper.TryParseInt(ref stepPara.Paras[0], out var buffId))
                {
                    var bc = unit.GetComponent<CombatUnitComponent>().GetComponent<BuffComponent>();
                    if (bc != null)
                    {
                        bc.Remove(buffId,true);
                    }
                }
            }
#endif
        }
    }
}