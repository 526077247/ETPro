namespace ET
{
    [ActionControlActiveWatcher(ActionControlType.BanSpell,true)]
    public class ActionControlActiveWatcherSpellComponent_AddAttackBanned:IActionControlActiveWatcher
    {
        public void SetActionControlActive(Unit unit)
        {
            var cc = unit.GetComponent<CombatUnitComponent>();
            if (cc!=null)
            {
                cc.GetComponent<SpellComponent>()?.SetEnable(false);
            }
        }
    }
    
    
    [ActionControlActiveWatcher(ActionControlType.BanSpell,false)]
    public class ActionControlActiveWatcherSpellComponent_RemoveAttackBanned:IActionControlActiveWatcher
    {
        public void SetActionControlActive(Unit unit)
        {
            var cc = unit.GetComponent<CombatUnitComponent>();
            if (cc!=null)
            {
                cc.GetComponent<SpellComponent>()?.SetEnable(true);
                
            }
        }
    }
}