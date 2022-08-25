namespace ET
{
    [FriendClass(typeof(AreaComponent))]
    public static class AreaComponentSystem
    {
        public class AwakeSystem: AwakeSystem<AreaComponent,string>
        {
            public override void Awake(AreaComponent self,string name)
            {
                self.AreaConfigCategory = AreaConfigComponent.Instance.Get(name);
            }
        }

        public static bool TryGetCellMap(this AreaComponent self,long cellId, out int sceneId)
        {
            if (self.AreaConfigCategory.GetAll() != null && self.AreaConfigCategory.GetAll().TryGetValue(cellId, out var conf))
            {
                var scene = StartSceneConfigCategory.Instance.Get(conf.SceneId);
                sceneId = scene.Id;
                return true;
            }
            sceneId = 0;
            return false;
        }
    }
}