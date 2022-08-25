using SuperScrollView;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UILoopListView2))]
    public class UILoopListView2DestorySystem : OnDestroySystem<UILoopListView2>
    {
        public override void OnDestroy(UILoopListView2 self)
        {
            self.unity_uilooplistview?.ClearListView();
            self.unity_uilooplistview = null;
        }
    }
    [FriendClass(typeof(UILoopListView2))]
    public static class UILoopListView2System
    {
        public static void ActivatingComponent(this UILoopListView2 self)
        {
            if (self.unity_uilooplistview == null)
            {
                self.unity_uilooplistview = self.GetGameObject().GetComponent<LoopListView2>();
                if (self.unity_uilooplistview == null)
                {
                    Log.Error($"添加UI侧组件UILoopListView2时，物体{self.GetGameObject().name}上没有找到LoopListView2组件");
                }
            }
        }

        public static void InitListView(this UILoopListView2 self, int itemTotalCount,
            System.Func<LoopListView2, int, LoopListViewItem2> onGetItemByIndex,
            LoopListViewInitParam initParam = null)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.InitListView(itemTotalCount, onGetItemByIndex, initParam);
        }


        //item是Unity侧的item对象，在这里创建相应的UI对象
        public static T AddItemViewComponent<T>(this UILoopListView2 self, LoopListViewItem2 item) where T : Entity,IOnCreate,IOnEnable,IAwake
        {
            //保证名字不能相同 不然没法cache
            item.gameObject.name = item.gameObject.name + item.ItemId;
            T t = self.AddUIComponentNotCreate<T>(item.gameObject.name);
            t.AddUIComponent<UITransform,Transform>("",item.transform);
            UIWatcherComponent.Instance.OnCreate(t);
            return t;
        }

        //根据Unity侧item获取UI侧的item
        public static T GetUIItemView<T>(this UILoopListView2 self, LoopListViewItem2 item) where T : Entity
        {
            return self.GetUIComponent<T>(item.gameObject.name);
        }
        //itemCount重设item的数量，resetPos是否刷新当前显示的位置
        public static void SetListItemCount(this UILoopListView2 self, int itemCount, bool resetPos = true)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.SetListItemCount(itemCount, resetPos);
        }

        //获取当前index对应的item 没有显示的话返回null
        public static LoopListViewItem2 GetShownItemByItemIndex(this UILoopListView2 self, int itemIndex)
        {
            self.ActivatingComponent();
            return self.unity_uilooplistview.GetShownItemByItemIndex(itemIndex);
        }

        public static void MovePanelToItemByRowColumn(this UILoopListView2 self, int itemIndex, float offset)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.MovePanelToItemIndex(itemIndex, offset);
        }


        public static void RefreshAllShownItem(this UILoopListView2 self)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.RefreshAllShownItem();
        }


        public static void SetOnBeginDragAction(this UILoopListView2 self, Action callback)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.mOnBeginDragAction = callback;
        }

        public static void SetOnDragingAction(this UILoopListView2 self, Action callback)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.mOnDragingAction = callback;
        }

        public static void SetOnEndDragAction(this UILoopListView2 self, Action callback)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.mOnEndDragAction = callback;
        }

        public static void MovePanelToItemIndex(this UILoopListView2 self, int index, float offset=0)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.MovePanelToItemIndex(index,offset);
        }
        
        public static void SetOnSnapChange(this UILoopListView2 self,Action<LoopListView2, LoopListViewItem2> callback)
        {
            self.ActivatingComponent();
            self.unity_uilooplistview.mOnSnapNearestChanged = callback;
        }

    }
}
