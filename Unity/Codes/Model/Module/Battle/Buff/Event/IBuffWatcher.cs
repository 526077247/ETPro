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
    
    public interface IAddBuffWatcher
    {
        void BeforeAdd(Unit attacker,Unit target,int id,ref bool canAdd);
        
        void AfterAdd(Unit attacker,Unit target,Buff buff);
    }

    public interface IRemoveBuffWatcher
    {
        void BeforeRemove(Unit target,Buff buff,ref bool canRemove);
        
        void AfterRemove(Unit target,Buff buff);
    }
    
    public interface IMoveBuffWatcher
    {
        void AfterMove(Unit target,Buff buff,WrapVector3 before);
    }
}