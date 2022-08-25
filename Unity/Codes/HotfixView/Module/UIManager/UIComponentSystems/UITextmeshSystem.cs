using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UITextmesh))]
    public class UITextmeshOnCreateSystem : OnCreateSystem<UITextmesh, string>
    {
        public override void OnCreate(UITextmesh self, string key)
        {
            self.SetI18NKey(key);
        }

    }
    [UISystem]
    [FriendClass(typeof(UITextmesh))]
    public class UITextmeshOnDestroySystem : OnDestroySystem<UITextmesh>
    {
        public override void OnDestroy(UITextmesh self)
        {
            self.unity_i18ncomp_touched = null;
            self.keyParams = null;
        }
    }
    [UISystem]
    [FriendClass(typeof(UITextmesh))]
    public class UITextmeshI18NSystem: I18NSystem<UITextmesh>
    {
        public override void OnLanguageChange(UITextmesh self)
        {
            self.OnLanguageChange();
        }
    }
    [FriendClass(typeof(UITextmesh))]
    public static class UITextmeshSystem
    {
        static void ActivatingComponent(this UITextmesh self)
        {
            if (self.unity_uitextmesh == null)
            {
                self.unity_uitextmesh = self.GetGameObject().GetComponent<TMPro.TMP_Text>();
                if (self.unity_uitextmesh == null)
                {
                    self.unity_uitextmesh = self.GetGameObject().AddComponent<TMPro.TMP_Text>();
                    Log.Info($"添加UI侧组件UITextmesh时，物体{self.GetGameObject().name}上没有找到TMPro.TMP_Text组件");
                }
                self.unity_i18ncomp_touched = self.GetGameObject().GetComponent<I18NText>();
            }
        }
        //当手动修改text的时候，需要将mono的i18textcomponent给禁用掉
        static void __DisableI18Component(this UITextmesh self,bool enable = false)
        {
            self.ActivatingComponent();
            if (self.unity_i18ncomp_touched != null)
            {
                self.unity_i18ncomp_touched.enabled = enable;
                if(!enable)
                    Log.Warning($"组件{self.GetGameObject().name}, text在逻辑层进行了修改，所以应该去掉去预设里面的I18N组件，否则会被覆盖");
            }
        }

        public static string GetText(this UITextmesh self)
        {
            self.ActivatingComponent();
            return self.unity_uitextmesh.text;
        }

        public static void SetText(this UITextmesh self, string text)
        {
            self.__DisableI18Component();
            self.__text_key = null;
            self.unity_uitextmesh.text = text;
        }
        public static void SetI18NKey(this UITextmesh self, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                self.SetText("");
                return;
            }
            self.__DisableI18Component();
            self.__text_key = key;
            self.SetI18NText(null);
        }
        public static void SetI18NKey(this UITextmesh self, string key,params object[] paras)
        {
            if (string.IsNullOrEmpty(key))
            {
                self.SetText("");
                return;
            }
            self.__DisableI18Component();
            self.__text_key = key;
            self.SetI18NText(paras);
        }

        public static void SetI18NText(this UITextmesh self, params object[] paras)
        {
            if (string.IsNullOrEmpty(self.__text_key))
            {
                Log.Error("there is not key ");
            }
            else
            {
                self.__DisableI18Component();
                self.keyParams = paras;
                if(I18NComponent.Instance.I18NTryGetText(self.__text_key, out var text)&& paras != null)
                    text =string.Format(text, paras);
                self.unity_uitextmesh.text = text;
            }
        }

        public static void OnLanguageChange(this UITextmesh self)
        {
            self.ActivatingComponent();
            if (self.__text_key != null)
            {
                if (I18NComponent.Instance.I18NTryGetText(self.__text_key, out var text) && self.keyParams != null)
                    text = string.Format(text, self.keyParams);
                self.unity_uitextmesh.text = text;
            }
        }

        public static void SetTextColor(this UITextmesh self, Color color)
        {
            self.ActivatingComponent();
            self.unity_uitextmesh.color = color;
        }

        public static void SetColor(this UITextmesh self, string colorStr)
        {
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                self.ActivatingComponent();
                self.unity_uitextmesh.color = color;
            }
            else
            {
                Log.Info(colorStr);
            }
        }
        
        public static void SetTextWithColor(this UITextmesh self, string text,string colorstr)
        {
            if (string.IsNullOrEmpty(colorstr))
                self.SetText(text);
            else
            {
                if (!colorstr.StartsWith("#")) colorstr = "#" + colorstr;
                self.SetText($"<color={colorstr}>{text}</color>");
            }
        }

        public static int GetCharacterCount(this UITextmesh self)
        {
            self.ActivatingComponent();
            return self.unity_uitextmesh.CharacterCount;
        }

        public static void SetMaxVisibleCharacters(this UITextmesh self,int count)
        {
            self.ActivatingComponent();
            self.unity_uitextmesh.maxVisibleCharacters = count;
        }
        
        public static Vector3 GetLastCharacterLocalPosition(this UITextmesh self)
        {
            self.ActivatingComponent();
            if (self.unity_uitextmesh.m_textInfo.characterInfo != null && self.unity_uitextmesh.m_textInfo.characterInfo.Length > 0)
            {
                var info = self.unity_uitextmesh.m_textInfo.characterInfo[self.unity_uitextmesh.m_textInfo.characterCount-1];
                return info.vertex_BR.position;
            }
            var rect = self.unity_uitextmesh.rectTransform.rect;
            return new Vector3(-rect.width / 2, -rect.height / 2, 0);
        }
    }
}
