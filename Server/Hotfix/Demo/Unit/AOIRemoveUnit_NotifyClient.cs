namespace ET
{
    // 离开视野
    [Event]
    public class AOIRemoveUnit_NotifyClient: AEvent<EventType.AOIRemoveUnit>
    {
        protected override void Run(EventType.AOIRemoveUnit args)
        {
            AOIUnitComponent a = args.Receive;
            if (a.GetParent<Unit>().Type != UnitType.Player||args.Units==null||args.Units.Count==0)
            {
                return;
            }
            ListComponent<long> temp = ListComponent<long>.Create();
            for (int i = 0; i < args.Units.Count; i++)
            {
                temp.Add(args.Units[i].Id);
            }
            UnitHelper.NoticeUnitsRemove(a.GetParent<Unit>(),temp);
            temp.Dispose();
        }
    }
}