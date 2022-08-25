namespace ET
{
    [FriendClass(typeof(Unit))]
    public static class UnitSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<Unit, int>
        {
            public override void Awake(Unit self, int configId)
            {
                self.ConfigId = configId;
            }
        }

        public static bool IsGhost(this Unit self)
        {
            var aoi = self.GetComponent<AOIUnitComponent>();
            if (aoi != null)
            {
                var ghost = aoi.GetComponent<GhostComponent>();
                if (ghost != null)
                {
                    return ghost.IsGoast;
                }
            }
            return false;
        }
    }
    
}