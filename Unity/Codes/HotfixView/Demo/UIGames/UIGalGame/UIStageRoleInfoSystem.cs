namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIStageRoleInfo))]
    public class UIStageRoleInfoOnCreateSystem:OnCreateSystem<UIStageRoleInfo>
    {
        public override void OnCreate(UIStageRoleInfo self)
        {
            self.image = self.AddUIComponent<UIImage>("Image");
        }
    }
}