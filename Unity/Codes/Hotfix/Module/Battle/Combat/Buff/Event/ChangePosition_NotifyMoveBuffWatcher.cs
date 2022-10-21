namespace ET
{
    public class ChangePosition_NotifyMoveBuffWatcher: AEventClass<EventType.ChangePosition>
    {
        protected override void Run(object changePosition)
        {
            EventType.ChangePosition args = changePosition as EventType.ChangePosition;
            BuffComponent bc = args.Unit.GetComponent<CombatUnitComponent>()?.GetComponent<BuffComponent>();
            if (bc != null)
            {
                bc.AfterMove(args.Unit,args.OldPos);
            }
            
        }
    }
}