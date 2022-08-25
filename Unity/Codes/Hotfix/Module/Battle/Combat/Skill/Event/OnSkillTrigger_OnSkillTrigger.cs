using System.Collections.Generic;
namespace ET
{

    public class OnSkillTrigger_OnSkillTrigger : AEvent<EventType.OnSkillTrigger>
    {
        protected override void Run(EventType.OnSkillTrigger args)
        {
            if (args.Type == AOITriggerType.Enter)
            {
                OnColliderIn(args.From, args.To, args.Para,args.CostId, args.Cost,args.Config,args.Skill);
            }
            else if (args.Type == AOITriggerType.Exit)
            {
                OnColliderOut(args.From, args.To, args.Para,args.CostId, args.Cost,args.Config,args.Skill);
            }
        }  
        
        /// <summary>
        /// 进入触发器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        /// <param name="skill">技能判断体</param>
        public void OnColliderIn(AOIUnitComponent from, AOIUnitComponent to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config,AOIUnitComponent skill = null)
        {
            if(from==null||to==null) return;//伤害计算参与者无了
            Unit fromU = from.GetParent<Unit>();
            Unit toU = to.GetParent<Unit>();
            var combatToU = toU.GetComponent<CombatUnitComponent>();
            var combatFromU = fromU.GetComponent<CombatUnitComponent>();
            // Log.Info("触发"+type.ToString()+to.Id+"  "+from.Id);
            // Log.Info("触发"+type.ToString()+to.Position+" Dis: "+Vector3.Distance(to.Position,from.Position));
            int formulaId = 0;//公式
            if (stepPara.Paras.Length > 1)
            {
                int.TryParse(stepPara.Paras[1].ToString(), out formulaId);
            }
            float percent = 1;//实际伤害百分比
            if (stepPara.Paras.Length > 2)
            {
                float.TryParse(stepPara.Paras[2].ToString(), out percent);
            }

            int maxNum = 0;
            if (stepPara.Paras.Length > 3)
            {
                int.TryParse(stepPara.Paras[3].ToString(), out maxNum);
            }

            if (maxNum != 0 && stepPara.Count >= maxNum) return;//超上限
            stepPara.Count++;
            
            List<int[]> buffInfo = null;//添加的buff
            if (stepPara.Paras.Length > 4)
            {
                buffInfo = stepPara.Paras[4] as List<int[]>;
                if (buffInfo == null)
                {
                    string[] vs = stepPara.Paras[4].ToString().Split(';');
                    buffInfo = new List<int[]>();
                    for (int i = 0; i < vs.Length; i++)
                    {
                        var data = vs[i].Split(',');
                        int[] temp = new int[data.Length];
                        for (int j = 0; j < data.Length; j++)
                        {
                            temp[j] = int.Parse(data[j]);
                        }
                        buffInfo.Add(temp);
                    }
                    stepPara.Paras[4] = buffInfo;
                }
            }
            
            if(buffInfo!=null&&buffInfo.Count>0)
            {
                var buffC = combatToU.GetComponent<BuffComponent>();
                
                for (int i = 0; i < buffInfo.Count; i++)
                {
                    
                    buffC.AddBuff(buffInfo[i][0],TimeHelper.ServerNow() + buffInfo[i][1],fromU.Id);
                }
            }

            FormulaConfig formula = FormulaConfigCategory.Instance.Get(formulaId);
            if (formula!=null)
            {
                FormulaStringFx fx = FormulaStringFx.GetInstance(formula.Formula);
                NumericComponent f = fromU.GetComponent<NumericComponent>();
                NumericComponent t = toU.GetComponent<NumericComponent>();
                float value = fx.GetData(f, t);
#if SERVER
                BattleHelper.Damage(combatFromU,combatToU,value,ghost:skill?.GetComponent<GhostComponent>());
#else
                BattleHelper.Damage(combatFromU,combatToU,value);
#endif
                
            }
        }
        /// <summary>
        /// 离开触发器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepPara"></param>
        /// <param name="costId"></param>
        /// <param name="cost"></param>
        /// <param name="config"></param>
        /// <param name="skill">技能判断体</param>
        public void OnColliderOut(AOIUnitComponent from, AOIUnitComponent to, SkillStepPara stepPara, List<int> costId,
            List<int> cost,SkillConfig config,AOIUnitComponent skill = null)
        {
            // Log.Info("触发"+type.ToString()+to.Id+"  "+from.Id);
            // Log.Info("触发"+type.ToString()+to.Position+" Dis: "+Vector3.Distance(to.Position,from.Position));
            if (stepPara.Paras.Length > 4)
            {
                List<int[]> buffInfo = stepPara.Paras[4] as List<int[]>;
                if (buffInfo != null&&buffInfo.Count>0)
                {
                    var buffC = to.Parent.GetComponent<CombatUnitComponent>().GetComponent<BuffComponent>();
                    for (int i = 0; i < buffInfo.Count; i++)
                    {
                        if (buffInfo[i][2] == 1)
                        {
                            buffC.RemoveByConfigId(buffInfo[i][0]);
                        }
                    }
                }
            }
        }
    }
}