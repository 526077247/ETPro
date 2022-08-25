using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    public class UIImageOnCreateSystem : OnCreateSystem<UIImage,string>
    {
        public override void OnCreate(UIImage self, string path)
        {
            self.SetSpritePath(path).Coroutine();
        }
    }
    [UISystem]
    [FriendClass(typeof(UIImage))]
    public class UIImageOnDestroySystem : OnDestroySystem<UIImage>
    {
        public override void OnDestroy(UIImage self)
        {
            if (!string.IsNullOrEmpty(self.sprite_path))
                ImageLoaderComponent.Instance?.ReleaseImage(self.sprite_path);
        }
    }
    [FriendClass(typeof(UIImage))]
    public static class UIImageSystem
    {
        static void ActivatingComponent(this UIImage self)
        {
            if (self.unity_uiimage == null)
            {
                self.unity_uiimage = self.GetGameObject().GetComponent<Image>();
                if (self.unity_uiimage == null)
                {
                    Log.Error($"添加UI侧组件UIImage时，物体{self.GetGameObject().name}上没有找到Image组件");
                }
                self.BgAutoFit =  self.GetGameObject().GetComponent<BgAutoFit>();
            }
        }
        public static async ETTask SetSpritePath(this UIImage self,string sprite_path,bool setNativeSize = false)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIImage, self.Id);
                if (sprite_path == self.sprite_path) return;
                self.ActivatingComponent();
                if (self.BgAutoFit != null) self.BgAutoFit.enabled = false;
                self.unity_uiimage.enabled = false;
                var base_sprite_path = self.sprite_path;
                self.sprite_path = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    self.unity_uiimage.sprite = null;
                    self.unity_uiimage.enabled = true;
                }
                else
                {
                    var sprite = await ImageLoaderComponent.Instance.LoadImageAsync(sprite_path);
                    self.unity_uiimage.enabled = true;
                    if (sprite == null)
                    {
                        ImageLoaderComponent.Instance.ReleaseImage(sprite_path);
                        return;
                    }
                    self.unity_uiimage.sprite = sprite;
                    if(setNativeSize)
                        self.SetNativeSize();
                    if (self.BgAutoFit != null)
                    {
                        self.BgAutoFit.bgSprite = sprite;
                        self.BgAutoFit.enabled = true;
                    }
                }
                if(!string.IsNullOrEmpty(base_sprite_path))
                    ImageLoaderComponent.Instance.ReleaseImage(base_sprite_path);
            }
            finally
            {
                coroutine?.Dispose();
            }
        }

        public static void SetNativeSize(this UIImage self)
        {
            self.unity_uiimage.SetNativeSize();
        }

        public static string GetSpritePath(this UIImage self)
        {
            return self.sprite_path;
        }
        public static void SetColor(this UIImage self, string colorStr)
        {
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                self.ActivatingComponent();
                self.unity_uiimage.color = color;
            }
            else
            {
                Log.Info(colorStr);
            }
        }
        public static void SetImageColor(this UIImage self,Color color)
        {
            self.ActivatingComponent();
            self.unity_uiimage.color = color;
        }

        public static Color GetImageColor(this UIImage self)
        {
            self.ActivatingComponent();
            return self.unity_uiimage.color;
        }
        public static void SetImageAlpha(this UIImage self,float a,bool changeChild=false)
        {
            self.ActivatingComponent();
            self.unity_uiimage.color = new Color(self.unity_uiimage.color.r,self.unity_uiimage.color.g,
                self.unity_uiimage.color.b,a);
            if (changeChild)
            {
                var images = self.unity_uiimage.GetComponentsInChildren<Image>(false);
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].color = new Color(images[i].color.r,images[i].color.g, images[i].color.b,a);
                }
                var texts = self.unity_uiimage.GetComponentsInChildren<TMPro.TMP_Text>(false);
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].color = new Color(texts[i].color.r,texts[i].color.g, texts[i].color.b,a);
                }
            }
        }
        public static void SetEnabled(this UIImage self,bool flag)
        {
            self.ActivatingComponent();
            self.unity_uiimage.enabled = flag;
        }
        public static async ETTask SetImageGray(this UIImage self,bool isGray)
        {
            self.ActivatingComponent();
            Material mt = null;
            if (isGray)
            {
                mt = await MaterialComponent.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
            }
            self.unity_uiimage.material = mt;
        }
        public static void SetFillAmount(this UIImage self, float value)
        {
            self.ActivatingComponent();
            self.unity_uiimage.fillAmount = value;
        }
        public static void DoSetFillAmount(this UIImage self, float newValue, float duration)
        {
            self.ActivatingComponent();
            DOTween.To(() => self.unity_uiimage.fillAmount,x=> self.unity_uiimage.fillAmount=x, newValue, duration);
        }

        public static Material GetMaterial(this UIImage self)
        {
            self.ActivatingComponent();
            return self.unity_uiimage.material;
        }
    }
}
