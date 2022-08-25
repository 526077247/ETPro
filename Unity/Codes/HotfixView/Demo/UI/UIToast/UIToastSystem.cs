using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIToast))]
    public class UIToastOnCreateSystem : OnCreateSystem<UIToast>
    {
        public override void OnCreate(UIToast self)
        {
            self.Text = self.AddUIComponent<UITextmesh>("Content");
        }
    }
    [UISystem]
    [FriendClass(typeof(UIToast))]
    public class UIToastOnEnableSystem : OnEnableSystem<UIToast, string>
    {
        public override void OnEnable(UIToast self, string param1)
        {
            self.Text.SetText(param1);
        }
    }
}
