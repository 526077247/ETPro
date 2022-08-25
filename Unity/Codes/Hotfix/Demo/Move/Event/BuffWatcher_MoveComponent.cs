namespace ET
{
    [ActionControlActiveWatcher(ActionControlType.BanMove,true)]
    public class ActionControlActiveWatcherMoveComponent_AddMoveBand:IActionControlActiveWatcher
    {
        public void SetActionControlActive(Unit unit)
        {
            var mc = unit.GetComponent<MoveComponent>();
            if (mc!=null)
            {
                mc.Enable = false;
                Log.Info(unit.Id+" Enable = false");
            }
        }
    }
    
    
    [ActionControlActiveWatcher(ActionControlType.BanMove,false)]
    public class ActionControlActiveWatcherMoveComponent_RemoveMoveBand:IActionControlActiveWatcher
    {
        public void SetActionControlActive(Unit unit)
        {
            var mc = unit.GetComponent<MoveComponent>();
            if (mc!=null)
            {
                mc.Enable = true;
                Log.Info(unit.Id+" Enable = true");
            }
        }
    }
}