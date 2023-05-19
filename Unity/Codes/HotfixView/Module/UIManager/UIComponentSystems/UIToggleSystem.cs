using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    public class UIToggleDestorySystem : OnDestroySystem<UIToggle>
    {
        public override void OnDestroy(UIToggle self)
        {
            self.RemoveOnValueChanged();
        }
    }
    [FriendClass(typeof(UIToggle))]
    public static class UIToggleSystem
    {
        static void ActivatingComponent(this UIToggle self)
        {
            if (self.toggle == null)
            {
                self.toggle = self.GetGameObject().GetComponent<Toggle>();
                if (self.toggle == null)
                {
                    Log.Error($"添加UI侧组件UIToggle时，物体{self.GetGameObject().name}上没有找到Toggle组件");
                }
            }
        }
        public static bool GetIsOn(this UIToggle self)
        {
            self.ActivatingComponent();
            return self.toggle.isOn;
        }

        public static void SetIsOn(this UIToggle self,bool ison,bool broadcast = true)
        {
            self.ActivatingComponent();
            if(broadcast)
                self.toggle.isOn = ison;
            else
                self.toggle.SetIsOnWithoutNotify(ison);
        }
        
        public static void SetOnValueChanged(this UIToggle self,Action<bool> cb)
        {
            self.ActivatingComponent();
            void OnValueChanged(bool val)
            {
                cb?.Invoke(val);
            }

            self.onValueChange = OnValueChanged;
            self.toggle.onValueChanged.AddListener(self.onValueChange);
        }
        
        public static void RemoveOnValueChanged(this UIToggle self)
        {
            if (self.onValueChange != null)
            {
                self.toggle.onValueChanged.RemoveListener(self.onValueChange);
            }
            
        }
    }
}