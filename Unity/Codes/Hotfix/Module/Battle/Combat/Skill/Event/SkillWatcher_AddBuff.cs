﻿namespace ET
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
            int curIndex = para.CurIndex;
            var stepPara = para.StepPara[curIndex];
            Log.Info("SkillWatcher_AddBuff");
            if (stepPara.Paras.Length >= 2)
            {
                if (int.TryParse(stepPara.Paras[0].ToString(), out var buffId))
                {
                    int.TryParse(stepPara.Paras[1].ToString(), out var time);
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