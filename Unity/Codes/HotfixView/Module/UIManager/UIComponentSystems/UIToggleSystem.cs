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
            if (self.unity_uitoggle == null)
            {
                self.unity_uitoggle = self.GetGameObject().GetComponent<Toggle>();
                if (self.unity_uitoggle == null)
                {
                    Log.Error($"添加UI侧组件UIToggle时，物体{self.GetGameObject().name}上没有找到Toggle组件");
                }
            }
        }
        public static bool GetIsOn(this UIToggle self)
        {
            self.ActivatingComponent();
            return self.unity_uitoggle.isOn;
        }

        public static void SetIsOn(this UIToggle self,bool ison,bool broadcast = true)
        {
            self.ActivatingComponent();
            if(broadcast)
                self.unity_uitoggle.isOn = ison;
            else
                self.unity_uitoggle.SetIsOnWithoutNotify(ison);
        }
        
        public static void SetOnValueChanged(this UIToggle self,Action<bool> cb)
        {
            self.ActivatingComponent();
            self.CallBack = (a)=>
            {
                cb?.Invoke(a);
            };
            self.unity_uitoggle.onValueChanged.AddListener(self.CallBack);
        }
        
        public static void RemoveOnValueChanged(this UIToggle self)
        {
            if (self.CallBack != null)
            {
                self.unity_uitoggle.onValueChanged.RemoveListener(self.CallBack);
            }
            
        }
    }
}