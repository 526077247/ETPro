using System;

namespace ET
{
	[ActorMessageHandler]
	public class C2M_TransferMapHandler : AMActorLocationRpcHandler<Unit, C2M_TransferMap, M2C_TransferMap>
	{
		protected override async ETTask Run(Unit unit, C2M_TransferMap request, M2C_TransferMap response, Action reply)
		{
			await ETTask.CompletedTask;

			string currentMap = unit.DomainScene().Name;
			StartSceneConfig currentMapConfig = StartSceneConfigCategory.Instance.GetBySceneName(unit.DomainScene().Zone, currentMap);
			MapSceneConfig currentMapSceneConfig = MapSceneConfigCategory.Instance.Get(currentMapConfig.Id);
			string toMapArea = null;
			if (currentMapSceneConfig.Name == "Map1")
			{
				toMapArea = "Map2AreaConfigCategory";
			}
			else
			{
				toMapArea = "Map1AreaConfigCategory";
			}
			var cellId = AOIHelper.CreateCellId(unit.Position, Define.CellLen);
			var area = AreaConfigComponent.Instance.Get(toMapArea).Get(cellId);
			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get(area.SceneId);
			MapSceneConfig startMapSceneConfig = MapSceneConfigCategory.Instance.Get(startSceneConfig.Id);
			TransferHelper.Transfer(unit, startSceneConfig.InstanceId, startMapSceneConfig.Name).Coroutine();
			
			reply();
		}
	}
}