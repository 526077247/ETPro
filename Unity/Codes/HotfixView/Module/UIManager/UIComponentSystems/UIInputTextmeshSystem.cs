using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [FriendClass(typeof(UIInputTextmesh))]
    public static class UIInputTextmeshSystem
    {
        static void ActivatingComponent(this UIInputTextmesh self)
        {
            if (self.unity_uiinput == null)
            {
                self.unity_uiinput = self.GetGameObject().GetComponent<TMPro.TMP_InputField>();
                if (self.unity_uiinput == null)
                {
                    Log.Error($"添加UI侧组件UIInputTextmesh时，物体{self.GetGameObject().name}上没有找到TMPro.TMP_InputField组件");
                }
            }
        }
        public static string GetText(this UIInputTextmesh self)
        {
            self.ActivatingComponent();
            return self.unity_uiinput.text;
        }

        public static void SetText(this UIInputTextmesh self,string text)
        {
            self.ActivatingComponent();
            self.unity_uiinput.text = text;
        }

        public static void SetOnValueChanged(this UIInputTextmesh self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnValueChanged();
            self.__OnValueChange = (a) =>
            {
                func?.Invoke();
            };
            self.unity_uiinput.onValueChanged.AddListener(self.__OnValueChange);
        }

        public static void RemoveOnValueChanged(this UIInputTextmesh self)
        {
            if(self.__OnValueChange!=null)
                self.unity_uiinput.onValueChanged.RemoveListener(self.__OnValueChange);
        }
        
        
        public static void SetOnEndEdit(this UIInputTextmesh self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnEndEdit();
            self.__OnEndEdit = (a) =>
            {
                func?.Invoke();
            };
            self.unity_uiinput.onEndEdit.AddListener(self.__OnEndEdit);
        }
        
        public static void RemoveOnEndEdit(this UIInputTextmesh self)
        {
            if(self.__OnEndEdit!=null)
                self.unity_uiinput.onEndEdit.RemoveListener(self.__OnEndEdit);
        }
    }
}
