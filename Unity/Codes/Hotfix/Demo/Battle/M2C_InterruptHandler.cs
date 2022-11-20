namespace ET
{
    [MessageHandler]
    [FriendClass(typeof(SpellComponent))]
    [FriendClass(typeof(SkillPara))]
    public class M2C_InterruptHandler : AMHandler<M2C_Interrupt>
    {
        protected override void Run(Session session, M2C_Interrupt message)
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
                if (spell.CurSkillConfigId == message.ConfigId)//在释放技能
                {
                    spell.Interrupt();
                }
                else//客户端已经释放结束了
                {
                    spell.CurSkillConfigId = message.ConfigId;
                    spell.GetComponent<SkillPara>().CurGroup = spell.GetComponent<SkillPara>().SkillConfig.InterruptGroup;
                    TimerComponent.Instance.Remove(ref spell.TimerId);
                    spell.PlayNextSkillStep(0);
                }
            }
        }

    }
}