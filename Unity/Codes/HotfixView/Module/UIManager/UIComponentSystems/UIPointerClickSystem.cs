using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIPointerClick))]
    public class UIPointerClickDestorySystem : OnDestroySystem<UIPointerClick>
    {
        public override void OnDestroy(UIPointerClick self)
        {
            if (self.__onclick != null)
                self.unity_pointerclick.onClick.RemoveListener(self.__onclick);
            self.__onclick = null;
        }
    }
    [FriendClass(typeof(UIPointerClick))]
    public static class UIPointerClickSystem
    {
        static void ActivatingComponent(this UIPointerClick self)
        {
            if (self.unity_pointerclick == null)
            {
                self.unity_pointerclick = self.GetGameObject().GetComponent<PointerClick>();
                if (self.unity_pointerclick == null)
                {
                    self.unity_pointerclick = self.GetGameObject().AddComponent<PointerClick>();
                    Log.Info($"添加UI侧组件UIPointerClick时，物体{self.GetGameObject().name}上没有找到PointerClick组件");
                }
            }
        }
        //虚拟点击
        public static void Click(this UIPointerClick self)
        {
            self.__onclick?.Invoke();
        }

        public static void SetOnClick(this UIPointerClick self,UnityAction callback)
        {
            self.ActivatingComponent();
            self.RemoveOnClick();
            self.__onclick = () =>
            {
                //AkSoundEngine.PostEvent("ConFirmation", Camera.main.gameObject);
                callback();
            };
            self.unity_pointerclick.onClick.AddListener(self.__onclick);
        }

        public static void RemoveOnClick(this UIPointerClick self)
        {
            if (self.__onclick != null)
                self.unity_pointerclick.onClick.RemoveListener(self.__onclick);
            self.__onclick = null;
        }

        public static void SetEnabled(this UIPointerClick self,bool flag)
        {
            self.ActivatingComponent();
            self.unity_pointerclick.enabled = flag;
        }

    }
}