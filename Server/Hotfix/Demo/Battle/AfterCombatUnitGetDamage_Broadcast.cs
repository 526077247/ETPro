namespace ET
{
    [FriendClass(typeof(SkillAbility))]
    public class AfterCombatUnitGetDamage_Broadcast: AEvent<EventType.AfterCombatUnitGetDamage>
    {
        protected override void Run(EventType.AfterCombatUnitGetDamage args)
        {
            var unit = args.Unit.unit;
            M2C_Damage msg = new M2C_Damage {  FromId = args.From.Id, ToId = unit.Id,Damage = args.DamageValue,NowBase = args.NowBaseValue};
            Log.Info(msg.FromId+"对"+ msg.ToId+"造成"+msg.Damage+"点伤害");
            MessageHelper.Broadcast(unit,msg,args.Ghost);
        }
    }
}