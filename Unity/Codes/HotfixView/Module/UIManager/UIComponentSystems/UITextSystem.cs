using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    [FriendClass(typeof (UIText))]
    public class UITextOnCreateSystem: OnCreateSystem<UIText>
    {
        public override void OnCreate(UIText self)
        {
            I18NComponent.Instance.RegisterI18NEntity(self);
        }
    }

    [UISystem]
    [FriendClass(typeof (UIText))]
    public class UITextOnCreateSystem1: OnCreateSystem<UIText, string>
    {
        public override void OnCreate(UIText self, string key)
        {
            I18NComponent.Instance.RegisterI18NEntity(self);
            self.SetI18NKey(key);
        }
    }

    [UISystem]
    [FriendClass(typeof (UIText))]
    public class UITextOnDestroySystem: OnDestroySystem<UIText>
    {
        public override void OnDestroy(UIText self)
        {
            I18NComponent.Instance.RemoveI18NEntity(self);
            self.i18nCompTouched = null;
            self.keyParams = null;
        }
    }

    [UISystem]
    [FriendClass(typeof (UIText))]
    public class UITextI18NSystem: I18NSystem<UIText>
    {
        public override void OnLanguageChange(UIText self)
        {
            self.OnLanguageChange();
        }
    }

    [FriendClass(typeof (UIText))]
    public static class UITextSystem
    {
        static void ActivatingComponent(this UIText self)
        {
            if (self.text == null)
            {
                self.text = self.GetGameObject().GetComponent<Text>();
                if (self.text == null)
                {
                    self.text = self.GetGameObject().AddComponent<Text>();
                    Log.Info($"添加UI侧组件UIText时，物体{self.GetGameObject().name}上没有找到Text组件");
                }

                self.i18nCompTouched = self.GetGameObject().GetComponent<I18NText>();
            }
        }

        /// <summary>
        /// 当手动修改text的时候，需要将mono的i18textcomponent给禁用掉
        /// </summary>
        /// <param name="self"></param>
        /// <param name="enable"></param>
        static void DisableI18Component(this UIText self, bool enable = false)
        {
            self.ActivatingComponent();
            if (self.i18nCompTouched != null)
            {
                self.i18nCompTouched.enabled = enable;
                if (!enable)
                    Log.Warning($"组件{self.GetGameObject().name}, text在逻辑层进行了修改，所以应该去掉去预设里面的I18N组件，否则会被覆盖");
            }
        }

        public static string GetText(this UIText self)
        {
            self.ActivatingComponent();
            return self.text.text;
        }

        public static void SetText(this UIText self, string text)
        {
            self.DisableI18Component();
            self.textKey = null;
            self.text.text = text;
        }

        public static void SetI18NKey(this UIText self, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                self.SetText("");
                return;
            }

            self.DisableI18Component();
            self.textKey = key;
            self.SetI18NText(null);
        }

        public static void SetI18NKey(this UIText self, string key, params object[] paras)
        {
            if (string.IsNullOrEmpty(key))
            {
                self.SetText("");
                return;
            }

            self.DisableI18Component();
            self.textKey = key;
            self.SetI18NText(paras);
        }

        public static void SetI18NText(this UIText self, params object[] paras)
        {
            if (string.IsNullOrEmpty(self.textKey))
            {
                Log.Error("there is not key ");
            }
            else
            {
                self.DisableI18Component();
                self.keyParams = paras;
                if (I18NComponent.Instance.I18NTryGetText(self.textKey, out var text) && paras != null)
                    text = string.Format(text, paras);
                self.text.text = text;
            }
        }

        public static void OnLanguageChange(this UIText self)
        {
            self.ActivatingComponent();
            {
                if (self.textKey != null)
                {
                    if (I18NComponent.Instance.I18NTryGetText(self.textKey, out var text) && self.keyParams != null)
                        text = string.Format(text, self.keyParams);
                    self.text.text = text;
                }
            }
        }

        public static void SetTextColor(this UIText self, Color color)
        {
            self.ActivatingComponent();
            self.text.color = color;
        }

        public static void SetTextWithColor(this UIText self, string text, string colorstr)
        {
            if (string.IsNullOrEmpty(colorstr))
                self.SetText(text);
            else
                self.SetText($"<color={colorstr}>{text}</color>");
        }
    }
}