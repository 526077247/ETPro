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
    [FriendClass(typeof (UIRawImage))]
    public class UIRawImageOnCreateSystem: OnCreateSystem<UIRawImage, string>
    {
        public override void OnCreate(UIRawImage self, string path)
        {
            self.SetSpritePath(path).Coroutine();
        }
    }

    [UISystem]
    [FriendClass(typeof (UIRawImage))]
    public class UIRawImageOnDestroySystem: OnDestroySystem<UIRawImage>
    {
        public override void OnDestroy(UIRawImage self)
        {
            if (!string.IsNullOrEmpty(self.spritePath))
            {
                self.image.texture = null;
                ImageLoaderComponent.Instance?.ReleaseImage(self.spritePath);
                self.spritePath = null;
            }
        }
    }

    [FriendClass(typeof (UIRawImage))]
    public static class UIRawImageSystem
    {
        static void ActivatingComponent(this UIRawImage self)
        {
            if (self.image == null)
            {
                self.image = self.GetGameObject().GetComponent<RawImage>();
                if (self.image == null)
                {
                    Log.Error($"添加UI侧组件UIRawImage时，物体{self.GetGameObject().name}上没有找到RawImage组件");
                }

                self.bgRawAutoFit = self.GetGameObject().GetComponent<BgRawAutoFit>();
            }
        }

        public static async ETTask SetSpritePath(this UIRawImage self, string sprite_path)
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIImage, self.Id);
                if (sprite_path == self.spritePath) return;
                self.ActivatingComponent();
                if (self.bgRawAutoFit != null) self.bgRawAutoFit.enabled = false;
                self.image.enabled = false;
                var base_sprite_path = self.spritePath;
                self.spritePath = sprite_path;
                if (string.IsNullOrEmpty(sprite_path))
                {
                    self.image.texture = null;
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

                    self.image.texture = sprite.texture;
                    if (self.bgRawAutoFit != null)
                    {
                        self.bgRawAutoFit.bgSprite = sprite.texture;
                        self.bgRawAutoFit.enabled = true;
                    }
                }

                if (!string.IsNullOrEmpty(base_sprite_path))
                    ImageLoaderComponent.Instance.ReleaseImage(base_sprite_path);
            }
            finally
            {
                coroutine?.Dispose();
            }
        }

        public static string GetSpritePath(this UIRawImage self)
        {
            return self.spritePath;
        }

        public static void SetImageColor(this UIRawImage self, Color color)
        {
            self.ActivatingComponent();
            self.image.color = color;
        }

        public static void SetImageAlpha(this UIRawImage self, float a)
        {
            self.ActivatingComponent();
            self.image.color = new Color(self.image.color.r, self.image.color.g,
                self.image.color.b, a);
        }

        public static void SetEnabled(this UIRawImage self, bool flag)
        {
            self.ActivatingComponent();
            self.image.enabled = flag;
        }

        public static async ETTask SetImageGray(this UIRawImage self,bool isGray)
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
    }
}