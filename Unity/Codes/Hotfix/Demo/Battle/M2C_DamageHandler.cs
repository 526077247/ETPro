namespace ET
{
    [MessageHandler]
    public class M2C_DamageHandler : AMHandler<M2C_Damage>
    {
        protected override void Run(Session session, M2C_Damage message)
        {
            UnitComponent uc = session.DomainScene().CurrentScene().GetComponent<UnitComponent>();
            Log.Info(message.FromId+"对"+ message.ToId+"造成"+message.Damage+"点伤害");
            var unit = uc.Get(message.ToId);
            if (unit != null)
            {
                var from = uc.Get(message.FromId);
                BattleHelper.Damage(from?.GetComponent<CombatUnitComponent>(),unit.GetComponent<CombatUnitComponent>(),message.Damage);
            }
        }

    }
}