using System;
using UnityEngine;
namespace ET
{
    [UISystem]
    [FriendClass(typeof(UICopyGameObject))]
    public class UICopyGameObjectOnDestroySystem : OnDestroySystem<UICopyGameObject>
    {
        public override void OnDestroy(UICopyGameObject self)
        {
            self.unity_comp.Clear();
        }
    }
    [FriendClass(typeof(UICopyGameObject))]
    public static class UICopyGameObjectSystem
    {
        static void ActivatingComponent(this UICopyGameObject self)
        {
            if (self.unity_comp == null)
            {
                self.unity_comp = self.GetGameObject().GetComponent<CopyGameObject>();
                if (self.unity_comp == null)
                {
                    self.unity_comp = self.GetGameObject().AddComponent<CopyGameObject>();
                    Log.Error($"添加UI侧组件UICopyGameObject时，物体{self.GetGameObject().name}上没有找到CopyGameObject组件");
                }
            }
        }

        public static void InitListView(this UICopyGameObject self,int total_count, Action<int, GameObject> ongetitemcallback = null, int? start_sibling_index = null)
        {
            self.ActivatingComponent();
            self.unity_comp.InitListView(total_count, ongetitemcallback, start_sibling_index);
        }
        
        //item是Unity侧的item对象，在这里创建相应的UI对象
        public static T AddItemViewComponent<T>(this UICopyGameObject self, GameObject item) where T : Entity,IAwake,IOnCreate,IOnEnable
        {
            //保证名字不能相同 不然没法cache
            T t = self.AddUIComponentNotCreate<T>(item.gameObject.name);
            t.AddUIComponent<UITransform,Transform>("",item.transform);
            UIWatcherComponent.Instance.OnCreate(t);
            return t;
        }
        //根据Unity侧item获取UI侧的item
        public static T GetUIItemView<T>(this UICopyGameObject self, GameObject item) where T : Entity
        {
            return self.GetUIComponent<T>(item.name);
        }
        
        public static void SetListItemCount(this UICopyGameObject self, int total_count, int? start_sibling_index = null)
        {
            self.unity_comp.SetListItemCount(total_count, start_sibling_index);
        }

        public static void RefreshAllShownItem(this UICopyGameObject self, int? start_sibling_index = null)
        {
            self.unity_comp.RefreshAllShownItem(start_sibling_index);
        }
        
        public static GameObject GetItemByIndex(this UICopyGameObject self,int index)
        {
            return self.unity_comp.GetItemByIndex(index);
        }

        public static int GetListItemCount(this UICopyGameObject self)
        {
            return self.unity_comp.GetListItemCount();
        }
    }
}