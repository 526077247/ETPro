using System;
using System.Collections.Generic;
using MongoDB.Bson;
using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    public class M2M_UnitAreaRemoveHandler : AMActorLocationHandler<Scene, M2M_UnitAreaRemove>
    {
        protected override async ETTask Run(Scene scene, M2M_UnitAreaRemove request)
        {
            await ETTask.CompletedTask;
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            var unit = unitComponent.Get(request.UnitId);
            if (unit!=null)
            {
                Log.Info(unit.Id+" M2M_UnitAreaRemove "+ unit.DomainScene().Id);
                unit.Dispose();
            }
        }
    }
}