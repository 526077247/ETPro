using UnityEngine;

namespace ET
{
    /// <summary>
    /// 消耗计算
    /// </summary>
    [SkillWatcher(SkillStepType.Cost)]
    public class SkillWatcher_Cost : ISkillWatcher
    {

        public void Run(SkillPara para)
        {
            Log.Info("SkillWatcher_Cost");
            if (para.StepPara[para.CurIndex].Paras.Length != 3)
            {
                Log.Error(para.Ability.SkillConfig.Id+"技能配置消耗属性和公式数量不对");
                return;
            }

            var stepPara = para.StepPara[para.CurIndex];
            StepParaHelper.TryParseString(ref stepPara.Paras[0], out var idKey);
            if(NumericType.Map.TryGetValue(idKey,out int attrId))
            {
                var cost = 0;
                StepParaHelper.TryParseInt(ref stepPara.Paras[1], out var costNum);
                StepParaHelper.TryParseInt(ref stepPara.Paras[2], out var costFormulaId);
                if (attrId < NumericType.Max) attrId = attrId * 10 + 1;
                FormulaConfig formula = FormulaConfigCategory.Instance.Get(costFormulaId);
                if (formula != null)
                {
                    FormulaStringFx fx = FormulaStringFx.GetInstance(formula.Formula);
                    NumericComponent f = para.From.unit.GetComponent<NumericComponent>();
                    NumericComponent t = para.To?.unit.GetComponent<NumericComponent>();
                    cost = (int)fx.GetData(f, t)+costNum;
                    float now = f.GetAsFloat(attrId);
                    if (cost > 0) //扣
                    {
                        if (now < cost)
                        {
                            f.Set(attrId,0);
                        }
                        else
                        {
                            f.Set(attrId,now -cost);
                        }
                    }
                    else if (cost < 0)//加
                    {
                        float max = f.GetAsFloat(attrId);
                        if (now + cost >= max)
                        {
                            f.Set(attrId,max);
                        }
                        else
                        {
                            f.Set(attrId,now + cost);
                        }
                    }
                }
                else
                {
                    Log.Error("公式未配置");
                }
                para.Cost.Add(cost);
                para.CostId.Add(attrId);
            }
            else
            {
                Log.Error(idKey+" 未配置");
            }
            
        }
    }
}