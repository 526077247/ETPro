namespace ET
{
    /// <summary>
    /// 登场角色信息
    /// </summary>
    public class UIStageRoleInfo:Entity,IOnCreate,IOnEnable,IAwake
    {
        public UIImage image;
        public string path;
        public bool active;
    }
}