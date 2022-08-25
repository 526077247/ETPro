namespace ET
{
    public interface IActionControlActiveWatcher
    {
        void SetActionControlActive(Unit unit);
    }
    
    public interface IDamageBuffWatcher
    {
        void BeforeDamage(Unit attacker,Unit target,Buff buff,DamageInfo info);
        
        void AfterDamage(Unit attacker,Unit target,Buff buff,DamageInfo info);
    }
}