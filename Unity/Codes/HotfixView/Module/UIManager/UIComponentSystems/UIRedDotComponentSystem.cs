using System;
using UnityEngine;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIRedDotComponent))]
    [FriendClass(typeof(RedDotComponent))]
    public class UIRedDotComponentOnCreateSystem: OnCreateSystem<UIRedDotComponent,string>
    {
        public override void OnCreate(UIRedDotComponent self,string target)
        {
            self.scaler = Vector3.one;
            self.positionOffset =Vector2.zero;
            self.target = target;
            RedDotComponent.Instance.AddUIRedDotComponent(target,self);
            self.OnRefreshCount(RedDotComponent.Instance.RetainViewCount[self.target]);
        }
    }
    [UISystem]
    [FriendClass(typeof(UIRedDotComponent))]
    [FriendClass(typeof(RedDotComponent))]
    public class UIRedDotComponentOnCreateSystem1: OnCreateSystem<UIRedDotComponent,string,Vector2>
    {
        public override void OnCreate(UIRedDotComponent self,string target,Vector2 positionOffset)
        {
            self.scaler = Vector3.one;
            self.positionOffset = positionOffset;
            self.target = target;
            RedDotComponent.Instance.AddUIRedDotComponent(target,self);
            self.OnRefreshCount(RedDotComponent.Instance.RetainViewCount[self.target]);
        }
    }
    [UISystem]
    [FriendClass(typeof(UIRedDotComponent))]
    public class UIRedDotComponentOnDestroySystem: OnDestroySystem<UIRedDotComponent>
    {
        public override void OnDestroy(UIRedDotComponent self)
        {
            RedDotComponent.Instance.RemoveUIRedDotComponent(self.target,out _);
            if(self.tempObj!=null)
                GameObjectPoolComponent.Instance?.RecycleGameObject(self.tempObj);
        }
    }
    [UISystem]
    [FriendClass(typeof(UIRedDotComponent))]
    [FriendClass(typeof(RedDotComponent))]
    public class UIRedDotComponentOnEnableSystem: OnEnableSystem<UIRedDotComponent>
    {
        public override void OnEnable(UIRedDotComponent self)
        {
            self.OnRefreshCount(RedDotComponent.Instance.RetainViewCount[self.target]);
        }
    }
    [UISystem]
    [FriendClass(typeof(UIRedDotComponent))]
    public class UIRedDotComponentRedDotSystem: RedDotSystem<UIRedDotComponent>
    {
        public override void OnRefreshCount(UIRedDotComponent self,int count)
        {
            self.OnRefreshCount(count);
        }
    }
    [FriendClass(typeof(UIRedDotComponent))]
    public static class UIRedDotComponentSystem
    {
        public static async ETTask ActivatingComponent(this UIRedDotComponent self)
        {
            if (self.reddot == null)
            {
                self.reddot = self.GetGameObject().GetComponentInChildren<RedDotMonoView>();
                if (self.reddot == null)
                {
                    string path = "UI/UICommon/Prefabs/UIRedDot.prefab";
                    var obj = await GameObjectPoolComponent.Instance.GetGameObjectAsync(path);
                    self.tempObj = obj;
                    obj.transform.SetParent(self.GetTransform(),false);
                    obj.transform.localScale = self.scaler;
                    obj.transform.GetComponent<RectTransform>().anchoredPosition = self.positionOffset;
                    self.reddot = obj.GetComponent<RedDotMonoView>();
                    if (self.reddot == null)
                    {
                        Log.Error($"添加UI侧组件UIRedDotComponent时，物体{self.GetGameObject().name}上实例化{path}失败");
                    }
                }
            }
        }

        public static async void OnRefreshCount(this UIRedDotComponent self,int count)
        {
            if (!self.isRedDotActive && count > 0)
            {
                await self.ActivatingComponent();
                self.reddot.gameObject.SetActive(true);
                self.isRedDotActive = true;
            }
            else if(self.isRedDotActive && count <= 0)
            {
                await self.ActivatingComponent();
                self.reddot.gameObject.SetActive(false);
                self.isRedDotActive = false;
            }
        }
    }
}
