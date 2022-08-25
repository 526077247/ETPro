using System;
using System.Collections.Generic;
using MongoDB.Bson;
using UnityEngine;

namespace ET
{
	[ActorMessageHandler]
	public class M2M_UnitTransferRequestHandler : AMActorRpcHandler<Scene, M2M_UnitTransferRequest, M2M_UnitTransferResponse>
	{
		protected override async ETTask Run(Scene scene, M2M_UnitTransferRequest request, M2M_UnitTransferResponse response, Action reply)
		{
			await ETTask.CompletedTask;
			UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
			Unit unit = request.Unit;
			
			unitComponent.AddChild(unit);
			unitComponent.Add(unit);

			foreach (var item in request.Map)
			{
				var entity = request.Entitys[item.ChildIndex];
				Entity parent;
				if (item.ParentIndex == -1)//父组件为自己
					parent = unit;
				else
					parent = request.Entitys[item.ParentIndex];
				
				if (item.IsChild == 0)
					parent.AddComponent(entity);
				else
					parent.AddChild(entity);
			}
			unit.AddComponent<MoveComponent>();
			var conf = MapSceneConfigCategory.Instance.Get((int)scene.Id);
			unit.AddComponent<PathfindingComponent, string>(conf.Name);
			unit.Position = new Vector3(-10, 0, -10);
			
			unit.AddComponent<MailBoxComponent>();
			
			// 通知客户端创建My Unit
			M2C_CreateMyUnit m2CCreateUnits = new M2C_CreateMyUnit();
			m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
			
			MessageHelper.SendToClient(unit, m2CCreateUnits);
			
			var numericComponent = unit.GetComponent<NumericComponent>();
			
			// 加入aoi
			var aoiu = unit.AddComponent<AOIUnitComponent,Vector3,Quaternion, UnitType,int>
					(unit.Position,unit.Rotation,unit.Type,numericComponent.GetAsInt(NumericType.AOI));
			
			aoiu.AddSphereCollider(0.5f);
			response.NewInstanceId = unit.InstanceId;
			
			reply();
		}
	}
}