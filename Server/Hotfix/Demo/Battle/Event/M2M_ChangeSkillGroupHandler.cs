namespace ET
{
    [ActorMessageHandler]
    [FriendClass(typeof(SpellComponent))]
    public class M2M_ChangeSkillGroupHandler :  AMActorLocationHandler<Scene, M2M_ChangeSkillGroup>
    {
        protected override async ETTask Run(Scene scene, M2M_ChangeSkillGroup message)
        {
            await ETTask.CompletedTask;
            UnitComponent uc = scene.GetComponent<UnitComponent>();
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
                var para = spell.GetComponent<SkillPara>();
                var stepPara = para.GetCurSkillStepPara();
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