using UnityEngine;

namespace ET
{
    public class ChangePosition_MoveAOIUnit: AEventClass<EventType.ChangePosition>
    {
        protected override void Run(object changePosition)
        {
            EventType.ChangePosition args = changePosition as EventType.ChangePosition;
            AOIUnitComponent aoiUnitComponent = args.Unit.GetComponent<AOIUnitComponent>();
            if (aoiUnitComponent == null || aoiUnitComponent.IsDisposed) return;
            aoiUnitComponent.Move(args.Unit.Position).Coroutine();
        }
    }
}