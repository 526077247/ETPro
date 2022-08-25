using UnityEngine;

namespace ET
{
    [FriendClass(typeof(AOICell))]
    public class ChangePosition_RefreshSceneView: AEventClass<EventType.ChangePosition>
    {
        protected override void Run(object changePosition)
        {
            EventType.ChangePosition args = changePosition as EventType.ChangePosition;
            if (args.Unit.Id == args.Unit.GetMyUnitIdFromZoneScene())
            {
                var nc =args.Unit.GetComponent<NumericComponent>();
                if(nc==null) return;
                ChangePosition(args.Unit.ZoneScene(), args.Unit.Position,nc.GetAsInt(NumericType.AOI)).Coroutine();
            }
        }

        public async ETTask ChangePosition(Scene scene,Vector3 pos, int view)
        {
            await AOISceneViewComponent.Instance.ChangePosition(scene, pos,view);
            await UIManagerComponent.Instance.CloseWindow<UILoadingView>();
        }
    }
}