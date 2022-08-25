using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class M2M_PathfindingResultHandler : AMActorLocationHandler<Scene, M2M_PathfindingResult>
    {
        protected override async ETTask Run(Scene scene, M2M_PathfindingResult message)
        {
            var uc = scene.GetComponent<UnitComponent>();
            if (uc != null)
            {
                var unit = uc.Get(message.Id);
                if (unit != null)
                {
                    unit.Position = new Vector3(message.X, message.Y, message.Z);
                    float speed = unit.GetComponent<NumericComponent>().GetAsFloat(NumericType.Speed);

                    using (ListComponent<Vector3> list = ListComponent<Vector3>.Create())
                    {
                        for (int i = 0; i < message.Xs.Count; ++i)
                        {
                            list.Add(new Vector3(message.Xs[i], message.Ys[i], message.Zs[i]));
                        }

                        unit.GetComponent<MoveComponent>().MoveToAsync(list, speed).Coroutine();
                    }
                }
            }
            await ETTask.CompletedTask;
        }
    }
}