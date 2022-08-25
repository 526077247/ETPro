namespace ET
{
    public static class UnitHelper
    {
        public static UnitComponent GetUnitComponentFromZoneScene(this Entity entity)
        {
            var zoneScene = entity.ZoneScene();
            Scene currentScene = zoneScene.GetComponent<CurrentScenesComponent>().Scene;
            return currentScene.GetComponent<UnitComponent>();
        }
        public static Unit GetMyUnitFromZoneScene(this Entity entity)
        {
            var zoneScene = entity.ZoneScene();
            PlayerComponent playerComponent = zoneScene.GetComponent<PlayerComponent>();
            Scene currentScene = zoneScene.GetComponent<CurrentScenesComponent>().Scene;
            return currentScene.GetComponent<UnitComponent>().Get(playerComponent.MyId);
        }
        public static long GetMyUnitIdFromZoneScene(this Entity entity)
        {
            var zoneScene = entity.ZoneScene();
            PlayerComponent playerComponent = zoneScene?.GetComponent<PlayerComponent>();
            if (playerComponent == null) return 0;
            return playerComponent.MyId;
        }
        public static Unit GetMyUnitFromCurrentScene(Scene currentScene)
        {
            PlayerComponent playerComponent = currentScene.Parent.Parent.GetComponent<PlayerComponent>();
            return currentScene.GetComponent<UnitComponent>().Get(playerComponent.MyId);
        }
    }
}