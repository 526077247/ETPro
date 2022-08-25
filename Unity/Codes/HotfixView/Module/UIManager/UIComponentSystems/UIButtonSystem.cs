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
    [FriendClass(typeof(UIButton))]
    public class UIButtonOnCreateSystem : OnCreateSystem<UIButton>
    {
        public override void OnCreate(UIButton self)
        {
            self.gray_state = false;
        }
    }
    [UISystem]
    [FriendClass(typeof(UIButton))]
    public class UIButtonOnDestroySystem : OnDestroySystem<UIButton>
    {
        public override void OnDestroy(UIButton self)
        {
            if (self.__onclick != null)
                self.unity_uibutton.onClick.RemoveListener(self.__onclick);
            if (!string.IsNullOrEmpty(self.sprite_path))
                ImageLoaderComponent.Instance?.ReleaseImage(self.sprite_path);
            self.__onclick = null;
        }
    }
    [FriendClass(typeof(UIButton))]
    public static class UIButtonSystem
    {
        static void ActivatingComponent(this UIButton self)
        {
            if (self.unity_uibutton == null)
            {
                self.unity_uibutton = self.GetGameObject().GetComponent<Button>();
                if (self.unity_uibutton == null)
                {
                    Log.Error($"添加UI侧组件UIButton时，物体{self.GetGameObject().name}上没有找到Button组件");
                }
            }
            if (self.unity_uiimage == null)
            {
                self.unity_uiimage = self.GetGameObject().GetComponent<Image>();
                if (self.unity_uiimage == null)
                {
                    Log.Error($"添加UI侧组件UIButton时，物体{self.GetGameObject().name}上没有找到Image组件");
                }
            }
        }
        //虚拟点击
        public static void Click(this UIButton self)
        {
            self.__onclick?.Invoke();
        }

        public static void SetOnClick(this UIButton self,Action callback)
        {
            self.ActivatingComponent();
            self.RemoveOnClick();
            self.__onclick = () =>
            {
                //AkSoundEngine.PostEvent("ConFirmation", Camera.main.gameObject);
                callback?.Invoke();
            };
            self.unity_uibutton.onClick.AddListener(self.__onclick);
        }

        public static void RemoveOnClick(this UIButton self)
        {
            if (self.__onclick != null)
                self.unity_uibutton.onClick.RemoveListener(self.__onclick);
            self.__onclick = null;
        }

        public static void SetEnabled(this UIButton self,bool flag)
        {
            self.ActivatingComponent();
            self.unity_uibutton.enabled = flag;
        }

        public static void SetInteractable(this UIButton self,bool flag)
        {
            self.ActivatingComponent();
            self.unity_uibutton.interactable = flag;
        }
        /// <summary>
        /// 设置按钮变灰
        /// </summary>
        /// <param name="isGray">是否变灰</param>
        /// <param name="includeText">是否包含字体, 不填的话默认为true</param>
        /// <param name="affectInteractable">是否影响交互, 不填的话默认为true</param>
        public static async void SetBtnGray(this UIButton self,bool isGray, bool includeText = true, bool affectInteractable = true)
        {
            if (self.gray_state == isGray) return;
            self.ActivatingComponent();
            self.gray_state = isGray;
            var mat = await MaterialComponent.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
            if (affectInteractable)
            {
                self.unity_uiimage.raycastTarget = !isGray;
            }
            self.SetBtnGray(mat, isGray, includeText);
        }

        public static void SetBtnGray(this UIButton self,Material grayMaterial, bool isGray, bool includeText)
        {
            self.ActivatingComponent();
            GameObject go = self.GetGameObject();
            if (go == null)
            {
                return;
            }
            Material mt = null;
            if (isGray)
            {
                mt = grayMaterial;
            }
            var coms = go.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < coms.Length; i++)
            {
                coms[i].material = mt;
            }

            if (includeText)
            {
                var textComs = go.GetComponentsInChildren<Text>();
                for (int i = 0; i < textComs.Length; i++)
                {
                    var uITextColorCtrl = TextColorCtrl.Get(textComs[i].gameObject);
                    if (isGray)
                    {
                        uITextColorCtrl.SetTextColor(new Color(89 / 255f, 93 / 255f, 93 / 255f));
                    }
                    else
                    {
                        uITextColorCtrl.ClearTextColor();
                    }
                }
            }
        }
        public static async void SetSpritePath(this UIButton self,string sprite_path)
        {
            if (string.IsNullOrEmpty(sprite_path)) return;
            if (sprite_path == self.sprite_path) return;
            self.ActivatingComponent();
            var base_sprite_path = self.sprite_path;
            self.sprite_path = sprite_path;
            var sprite =await ImageLoaderComponent.Instance.LoadImageAsync(sprite_path);
            if (sprite == null)
            {
                ImageLoaderComponent.Instance.ReleaseImage(sprite_path);
                return;
            }

            if (!string.IsNullOrEmpty(base_sprite_path))
                ImageLoaderComponent.Instance.ReleaseImage(base_sprite_path);

            self.unity_uiimage.sprite = sprite;

        }

        public static string GetSpritePath(this UIButton self)
        {
            return self.sprite_path;
        }

        public static void SetImageColor(this UIButton self,Color color)
        {
            self.ActivatingComponent();
            self.unity_uiimage.color = color;
        }

    }
}