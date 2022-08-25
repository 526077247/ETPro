using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class M2M_StopHandler : AMActorLocationHandler<Scene, M2M_Stop>
    {
        protected override async ETTask Run(Scene scene, M2M_Stop message)
        {
            var uc = scene.GetComponent<UnitComponent>();
            if (uc != null)
            {
                var unit = uc.Get(message.Id);
                if (unit != null)
                {
                    unit.GetComponent<MoveComponent>().Stop();
                    unit.Position = new Vector3(message.X, message.Y, message.Z);
                    unit.Rotation = new Quaternion(message.A, message.B, message.C,message.W);
                }
            }
            await ETTask.CompletedTask;
        }
    }
}