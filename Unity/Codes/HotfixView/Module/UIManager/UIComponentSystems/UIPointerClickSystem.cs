using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace ET
{
    [UISystem]
    [FriendClass(typeof (UIPointerClick))]
    public class UIPointerClickDestorySystem: OnDestroySystem<UIPointerClick>
    {
        public override void OnDestroy(UIPointerClick self)
        {
            if (self.onClick != null)
                self.pointerClick.onClick.RemoveListener(self.onClick);
            self.onClick = null;
        }
    }

    [FriendClass(typeof (UIPointerClick))]
    public static class UIPointerClickSystem
    {
        static void ActivatingComponent(this UIPointerClick self)
        {
            if (self.pointerClick == null)
            {
                self.pointerClick = self.GetGameObject().GetComponent<PointerClick>();
                if (self.pointerClick == null)
                {
                    self.pointerClick = self.GetGameObject().AddComponent<PointerClick>();
                    Log.Info($"添加UI侧组件UIPointerClick时，物体{self.GetGameObject().name}上没有找到PointerClick组件");
                }
            }
        }

        /// <summary>
        /// 虚拟点击
        /// </summary>
        /// <param name="self"></param>
        public static void Click(this UIPointerClick self)
        {
            self.onClick?.Invoke();
        }

        public static void SetOnClick(this UIPointerClick self, UnityAction callback)
        {
            self.ActivatingComponent();
            self.RemoveOnClick();
            self.onClick = () =>
            {
                //AkSoundEngine.PostEvent("ConFirmation", Camera.main.gameObject);
                callback?.Invoke();
            };
            self.pointerClick.onClick.AddListener(self.onClick);
        }

        public static void RemoveOnClick(this UIPointerClick self)
        {
            if (self.onClick != null)
                self.pointerClick.onClick.RemoveListener(self.onClick);
            self.onClick = null;
        }

        public static void SetEnabled(this UIPointerClick self, bool flag)
        {
            self.ActivatingComponent();
            self.pointerClick.enabled = flag;
        }
    }
}