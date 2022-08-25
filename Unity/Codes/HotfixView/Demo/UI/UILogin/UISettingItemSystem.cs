using System;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UISettingItem))]
    public class UISettingItemOnCreateSystem : OnCreateSystem<UISettingItem>
    {
        public override void OnCreate(UISettingItem self)
        {
            self.Text = self.AddUIComponent<UIText>("Text");
            self.Button = self.AddUIComponent<UIButton>("");
            self.Button.SetOnClick(() => { self.OnClick(); });
            
        }
    }
    [FriendClass(typeof(UISettingItem))]
    public static class UISettingItemSystem
    {
        public static void SetData(this UISettingItem self, ServerConfig data,Action<int> callback)
        {
            self.Data = data;
            self.Text.SetText(data.Name);
            self.Callback = callback;
        }

        public static void OnClick(this UISettingItem self)
        {
            self.Callback?.Invoke(self.Data.Id);
        }
    }
}