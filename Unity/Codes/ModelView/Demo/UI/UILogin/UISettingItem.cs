using System;
namespace ET
{
    [UIComponent]
    public class UISettingItem:Entity,IAwake,IOnCreate,IOnEnable
    {
        public UIButton Button;
        public UIText Text;
        public ServerConfig Data;
        public Action<int> Callback;
    }
}