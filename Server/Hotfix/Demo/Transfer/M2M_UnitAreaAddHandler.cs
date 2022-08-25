using System;
using System.Collections.Generic;
using MongoDB.Bson;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class M2M_UnitAreaAddHandler : AMActorLocationHandler<Scene, M2M_UnitAreaAdd>
    {
        protected override async ETTask Run(Scene scene, M2M_UnitAreaAdd request)
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
            
            UnitFactory.AfterCreateUnitFromMsg(unit,CreateUnitFromMsgType.Add);
            if (request.MoveInfo != null)
            {
                if (request.MoveInfo.X.Count > 0)
                {
                    using (ListComponent<Vector3> list = ListComponent<Vector3>.Create())
                    {
                        list.Add(unit.Position);
                        for (int i = 0; i < request.MoveInfo.X.Count; ++i)
                        {
                            list.Add(new Vector3(request.MoveInfo.X[i], request.MoveInfo.Y[i], request.MoveInfo.Z[i]));
                        }

                        unit.MoveToAsync(list).Coroutine();
                    }
                }
            }
        }
    }
}