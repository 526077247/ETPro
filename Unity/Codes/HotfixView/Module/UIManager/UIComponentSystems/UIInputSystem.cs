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
            if (self.unity_uiinput == null)
            {
                self.unity_uiinput = self.GetGameObject().GetComponent<InputField>();
                if (self.unity_uiinput == null)
                {
                    Log.Error($"添加UI侧组件UIInput时，物体{self.GetGameObject().name}上没有找到InputField组件");
                }
            }
        }
        public static string GetText(this UIInput self)
        {
            self.ActivatingComponent();
            return self.unity_uiinput.text;
        }

        public static void SetText(this UIInput self,string text)
        {
            self.ActivatingComponent();
            self.unity_uiinput.text = text;
        }
        public static void SetOnValueChanged(this UIInput self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            self.__OnValueChange = (a) =>
            {
                func?.Invoke();
            };
            self.unity_uiinput.onValueChanged.AddListener(self.__OnValueChange);
        }

        public static void RemoveOnValueChanged(this UIInput self)
        {
            if(self.__OnValueChange!=null)
                self.unity_uiinput.onValueChanged.RemoveListener(self.__OnValueChange);
        }
        
        
        public static void SetOnEndEdit(this UIInput self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnEndEdit();
            self.__OnEndEdit = (a) =>
            {
                func?.Invoke();
            };
            self.unity_uiinput.onEndEdit.AddListener(self.__OnEndEdit);
        }
        
        public static void RemoveOnEndEdit(this UIInput self)
        {
            if(self.__OnEndEdit!=null)
                self.unity_uiinput.onEndEdit.RemoveListener(self.__OnEndEdit);
        }
    }
}
