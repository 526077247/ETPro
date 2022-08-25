using UnityEngine;
namespace ET
{
    [ActorMessageHandler]
    public class M2M_UnitAreaCreateHandler : AMActorLocationHandler<Scene, M2M_UnitAreaCreate>
    {
        protected override async ETTask Run(Scene scene, M2M_UnitAreaCreate request)
        {
            await ETTask.CompletedTask;
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            Unit unit = request.Unit;
            var oldUnit = unitComponent.Get(unit.Id);
            if (oldUnit==null)
            {
                unitComponent.AddChild(unit);
                unitComponent.Add(unit);

                foreach (var item in request.Map)
                {
                    var entity = request.Entitys[item.ChildIndex];
                    Entity parent;
                    if (item.ParentIndex == -1) //父组件为自己
                        parent = unit;
                    else
                        parent = request.Entitys[item.ParentIndex];

                    if (item.IsChild == 0)
                        parent.AddComponent(entity);
                    else
                        parent.AddChild(entity);
                }
            }
            else
            {
                unit = oldUnit;
            }
            
            UnitFactory.AfterCreateUnitFromMsg(unit,CreateUnitFromMsgType.Create);
        }
    }
}