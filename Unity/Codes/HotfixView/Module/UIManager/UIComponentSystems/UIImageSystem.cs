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
            if (!string.IsNullOrEmpty(self.spritePath))
                ImageLoaderComponent.Instance?.ReleaseImage(self.spritePath);
        }
    }
    [FriendClass(typeof(UIImage))]
    public static class UIImageSystem
    {
        static void ActivatingComponent(this UIImage self)
        {
            if (self.image == null)
            {
                self.image = self.GetGameObject().GetComponent<Image>();
                if (self.image == null)
                {
                    Log.Error($"添加UI侧组件UIImage时，物体{self.GetGameObject().name}上没有找到Image组件");
                }
                self.bgAutoFit =  self.GetGameObject().GetComponent<BgAutoFit>();
            }
        }
        public static async ETTask SetSpritePath(this UIImage self,string sprite_path,bool setNativeSize = false)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIImage, self.Id);
                if (sprite_path == self.spritePath) return;
                self.ActivatingComponent();
                if (self.bgAutoFit != null) self.bgAutoFit.enabled = false;
                self.image.enabled = false;
                var base_sprite_path = self.spritePath;
                self.spritePath = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    self.image.sprite = null;
                    self.image.enabled = true;
                }
                else
                {
                    var sprite = await ImageLoaderComponent.Instance.LoadImageAsync(sprite_path);
                    self.image.enabled = true;
                    if (sprite == null)
                    {
                        ImageLoaderComponent.Instance.ReleaseImage(sprite_path);
                        return;
                    }
                    self.image.sprite = sprite;
                    if(setNativeSize)
                        self.SetNativeSize();
                    if (self.bgAutoFit != null)
                    {
                        self.bgAutoFit.bgSprite = sprite;
                        self.bgAutoFit.enabled = true;
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
            self.ActivatingComponent();
            self.image.SetNativeSize();
        }

        public static string GetSpritePath(this UIImage self)
        {
            return self.spritePath;
        }
        public static void SetColor(this UIImage self, string colorStr)
        {
            if (!colorStr.StartsWith("#")) colorStr = "#" + colorStr;
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                self.ActivatingComponent();
                self.image.color = color;
            }
            else
            {
                Log.Info(colorStr);
            }
        }
        public static void SetImageColor(this UIImage self,Color color)
        {
            self.ActivatingComponent();
            self.image.color = color;
        }

        public static Color GetImageColor(this UIImage self)
        {
            self.ActivatingComponent();
            return self.image.color;
        }
        public static void SetImageAlpha(this UIImage self,float a,bool changeChild=false)
        {
            self.ActivatingComponent();
            self.image.color = new Color(self.image.color.r,self.image.color.g,
                self.image.color.b,a);
            if (changeChild)
            {
                var images = self.image.GetComponentsInChildren<Image>(false);
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].color = new Color(images[i].color.r,images[i].color.g, images[i].color.b,a);
                }
                var texts = self.image.GetComponentsInChildren<TMPro.TMP_Text>(false);
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].color = new Color(texts[i].color.r,texts[i].color.g, texts[i].color.b,a);
                }
            }
        }
        public static void SetEnabled(this UIImage self,bool flag)
        {
            self.ActivatingComponent();
            self.image.enabled = flag;
        }
        public static async ETTask SetImageGray(this UIImage self,bool isGray)
        {
            if (self.grayState == isGray) return;
            self.ActivatingComponent();
            self.grayState = isGray;
            Material mt = null;
            if (isGray)
            {
                mt = await MaterialComponent.Instance.LoadMaterialAsync("UI/UICommon/Materials/uigray.mat");
                if (!self.grayState)
                {
                    mt = null;
                }
            }
            self.image.material = mt;
        }
        public static void SetFillAmount(this UIImage self, float value)
        {
            self.ActivatingComponent();
            self.image.fillAmount = value;
        }
        public static void DoSetFillAmount(this UIImage self, float newValue, float duration)
        {
            self.ActivatingComponent();
            DOTween.To(() => self.image.fillAmount,x=> self.image.fillAmount=x, newValue, duration);
        }

        public static Material GetMaterial(this UIImage self)
        {
            self.ActivatingComponent();
            return self.image.material;
        }
    }
}
