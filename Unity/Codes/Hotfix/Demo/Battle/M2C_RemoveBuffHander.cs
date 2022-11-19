namespace ET
{
    [MessageHandler]
    public class M2C_RemoveBuffHander : AMHandler<M2C_RemoveBuff>
    {
        protected override void Run(Session session, M2C_RemoveBuff message)
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
                var buff = buffC.GetByConfigId(message.ConfigId);
                if(buff!=null)buffC.RemoveByOther(buff.Id,true);
            }
        }

    }
}