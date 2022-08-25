namespace ET
{
    [MessageHandler]
    public class M2C_AddBuffHandler : AMHandler<M2C_AddBuff>
    {
        protected override void Run(Session session, M2C_AddBuff message)
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

                var buffC = combatU.GetComponent<BuffComponent>();
                buffC.AddBuff(message.ConfigId, message.Timestamp,message.SourceId);
            }
        }

    }
}