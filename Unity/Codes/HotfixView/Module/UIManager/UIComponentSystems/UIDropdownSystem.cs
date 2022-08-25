using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIDropdown))]
    public class UIDropdownDestorySystem : OnDestroySystem<UIDropdown>
    {
        public override void OnDestroy(UIDropdown self)
        {
            self.RemoveOnValueChanged();
        }
    }
    [FriendClass(typeof(UIDropdown))]
    public static class UIDropdownSystem
    {
        static void ActivatingComponent(this UIDropdown self)
        {
            if (self.unity_uidropdown == null)
            {
                self.unity_uidropdown = self.GetGameObject().GetComponent<Dropdown>();
                if (self.unity_uidropdown == null)
                {
                    Log.Error($"添加UI侧组件UIDropdown时，物体{self.GetGameObject().name}上没有找到Dropdown组件");
                }
            }
        }
        public static void SetOnValueChanged(this UIDropdown self, UnityAction<int> callback)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            self.__onValueChanged = callback;
            self.unity_uidropdown.onValueChanged.AddListener(self.__onValueChanged);
        }
        public static void RemoveOnValueChanged(this UIDropdown self)
        {
            if (self.__onValueChanged != null)
            {
                self.unity_uidropdown.onValueChanged.RemoveListener(self.__onValueChanged);
                self.__onValueChanged = null;
            }
        }
        public static int GetValue(this UIDropdown self)
        {
            self.ActivatingComponent();
            return self.unity_uidropdown.value;
        }
        public static void SetValue(this UIDropdown self, int value)
        {
            self.ActivatingComponent();
            self.unity_uidropdown.value = value;
        }
    }
}