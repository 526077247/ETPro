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
            if (self.dropdown == null)
            {
                self.dropdown = self.GetGameObject().GetComponent<Dropdown>();
                if (self.dropdown == null)
                {
                    Log.Error($"添加UI侧组件UIDropdown时，物体{self.GetGameObject().name}上没有找到Dropdown组件");
                }
            }
        }
        public static void SetOnValueChanged(this UIDropdown self, UnityAction<int> callback)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            self.onValueChanged = callback;
            self.dropdown.onValueChanged.AddListener(self.onValueChanged);
        }
        public static void RemoveOnValueChanged(this UIDropdown self)
        {
            if (self.onValueChanged != null)
            {
                self.dropdown.onValueChanged.RemoveListener(self.onValueChanged);
                self.onValueChanged = null;
            }
        }
        public static int GetValue(this UIDropdown self)
        {
            self.ActivatingComponent();
            return self.dropdown.value;
        }
        public static void SetValue(this UIDropdown self, int value)
        {
            self.ActivatingComponent();
            self.dropdown.value = value;
        }
    }
}