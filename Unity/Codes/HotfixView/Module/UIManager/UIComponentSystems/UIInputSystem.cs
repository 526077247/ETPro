using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace ET
{
    [FriendClass(typeof(UIInput))]
    public static class UIInputSystem
    {
        static void ActivatingComponent(this UIInput self)
        {
            if (self.input == null)
            {
                self.input = self.GetGameObject().GetComponent<InputField>();
                if (self.input == null)
                {
                    Log.Error($"添加UI侧组件UIInput时，物体{self.GetGameObject().name}上没有找到InputField组件");
                }
            }
        }
        public static string GetText(this UIInput self)
        {
            self.ActivatingComponent();
            return self.input.text;
        }

        public static void SetText(this UIInput self,string text)
        {
            self.ActivatingComponent();
            self.input.text = text;
        }
        public static void SetOnValueChanged(this UIInput self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            void OnValueChanged(string val)
            {
                func?.Invoke();
            }

            self.onValueChange = OnValueChanged;
            self.input.onValueChanged.AddListener(self.onValueChange);
        }

        public static void RemoveOnValueChanged(this UIInput self)
        {
            if(self.onValueChange!=null)
                self.input.onValueChanged.RemoveListener(self.onValueChange);
        }
        
        
        public static void SetOnEndEdit(this UIInput self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnEndEdit();
            self.onEndEdit = (a) =>
            {
                func?.Invoke();
            };
            self.input.onEndEdit.AddListener(self.onEndEdit);
        }
        
        public static void RemoveOnEndEdit(this UIInput self)
        {
            if(self.onEndEdit!=null)
                self.input.onEndEdit.RemoveListener(self.onEndEdit);
        }
    }
}
