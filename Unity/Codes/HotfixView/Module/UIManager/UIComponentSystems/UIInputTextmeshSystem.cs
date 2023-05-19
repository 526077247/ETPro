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
            if (self.input == null)
            {
                self.input = self.GetGameObject().GetComponent<TMPro.TMP_InputField>();
                if (self.input == null)
                {
                    Log.Error($"添加UI侧组件UIInputTextmesh时，物体{self.GetGameObject().name}上没有找到TMPro.TMP_InputField组件");
                }
            }
        }
        public static string GetText(this UIInputTextmesh self)
        {
            self.ActivatingComponent();
            return self.input.text;
        }

        public static void SetText(this UIInputTextmesh self,string text)
        {
            self.ActivatingComponent();
            self.input.text = text;
        }

        public static void SetOnValueChanged(this UIInputTextmesh self, Action func)
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

        public static void RemoveOnValueChanged(this UIInputTextmesh self)
        {
            if(self.onValueChange!=null)
                self.input.onValueChanged.RemoveListener(self.onValueChange);
        }
        
        
        public static void SetOnEndEdit(this UIInputTextmesh self, Action func)
        {
            self.ActivatingComponent();
            self.RemoveOnEndEdit();
            self.onEndEdit = (a) =>
            {
                func?.Invoke();
            };
            self.input.onEndEdit.AddListener(self.onEndEdit);
        }
        
        public static void RemoveOnEndEdit(this UIInputTextmesh self)
        {
            if(self.onEndEdit!=null)
                self.input.onEndEdit.RemoveListener(self.onEndEdit);
        }
    }
}
