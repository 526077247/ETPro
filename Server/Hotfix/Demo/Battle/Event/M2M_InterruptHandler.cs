namespace ET
{
    [FriendClass(typeof(SpellComponent))]
    [FriendClass(typeof(SkillAbility))]
    [ActorMessageHandler]
    public class M2M_InterruptHandler : AMActorLocationHandler<Scene, M2M_Interrupt>
    {
        protected override async ETTask Run(Scene scene, M2M_Interrupt message)
        {
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
                if (spell.CurSkillConfigId == message.ConfigId)//在释放技能
                {
                    spell.Interrupt();
                }
                else//已经释放结束了
                {
                    spell.CurSkillConfigId = message.ConfigId;
                    spell.GetComponent<SkillPara>().CurGroup = spell.GetComponent<SkillPara>().SkillConfig.InterruptGroup;
                    TimerComponent.Instance.Remove(ref spell.TimerId);
                    spell.PlayNextSkillStep(0);
                }
            }
            await ETTask.CompletedTask;
        }
    }
}