namespace ET
{
    [ActionControlActiveWatcher(ActionControlType.BanSpell,true)]
    public class ActionControlActiveWatcherSpellPreviewComponent_AddAttackBanned:IActionControlActiveWatcher
    {
        public void SetActionControlActive(Unit unit)
        {
            var cc = unit.GetComponent<CombatUnitComponent>();
            if (cc!=null)
            {
                cc.GetComponent<SpellPreviewComponent>()?.SetEnable(false);
            }
        }
    }
    
    
    [ActionControlActiveWatcher(ActionControlType.BanSpell,false)]
    public class ActionControlActiveWatcherSpellPreviewComponent_RemoveAddAttackBanned:IActionControlActiveWatcher
    {
        public void SetActionControlActive(Unit unit)
        {
            var cc = unit.GetComponent<CombatUnitComponent>();
            if (cc!=null)
            {
                cc.GetComponent<SpellPreviewComponent>()?.SetEnable(true);
                
            }
        }
    }
}