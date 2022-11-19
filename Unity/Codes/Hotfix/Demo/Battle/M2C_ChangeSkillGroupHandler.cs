namespace ET
{
    [MessageHandler]
    [FriendClass(typeof(SpellComponent))]
    public class M2C_ChangeSkillGroupHandler : AMHandler<M2C_ChangeSkillGroup>
    {
        protected override void Run(Session session, M2C_ChangeSkillGroup message)
        {
            UnitComponent uc = session.DomainScene().CurrentScene().GetComponent<UnitComponent>();
            var unit = uc.Get(message.UnitId);
            if (unit != null)
            {
                var combatU = unit.GetComponent<CombatUnitComponent>();
                if (combatU == null)
                {
                    Log.Info("combatU == null "+message.UnitId);
                    combatU = unit.AddComponent<CombatUnitComponent>();
                }

                var spell = combatU.GetComponent<SpellComponent>();
                var para = spell.Para;
                var stepPara = spell.Para.StepPara[spell.Para.CurIndex];
                if(StepParaHelper.TryParseString(ref stepPara.Paras[0], out var condition))
                {
                    var res = ConditionWatcherComponent.Instance.Run(condition,para);
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
                spell.WaitStepOver(SkillStepType.ChangeGroup);
            }
        }

    }
}