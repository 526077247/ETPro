using SuperScrollView;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET
{
    [UISystem]
    [FriendClass(typeof (UILoopListView2))]
    public class UILoopListView2DestorySystem: OnDestroySystem<UILoopListView2>
    {
        public override void OnDestroy(UILoopListView2 self)
        {
            self.loopListView?.ClearListView();
            self.loopListView = null;
        }
    }

    [FriendClass(typeof (UILoopListView2))]
    public static class UILoopListView2System
    {
        public static void ActivatingComponent(this UILoopListView2 self)
        {
            if (self.loopListView == null)
            {
                self.loopListView = self.GetGameObject().GetComponent<LoopListView2>();
                if (self.loopListView == null)
                {
                    Log.Error($"添加UI侧组件UILoopListView2时，物体{self.GetGameObject().name}上没有找到LoopListView2组件");
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemTotalCount"></param>
        /// <param name="onGetItemByIndex"></param>
        /// <param name="initParam"></param>
        public static void InitListView(this UILoopListView2 self, int itemTotalCount,
        System.Func<LoopListView2, int, LoopListViewItem2> onGetItemByIndex,
        LoopListViewInitParam initParam = null)
        {
            self.ActivatingComponent();
            self.loopListView.InitListView(itemTotalCount, onGetItemByIndex, initParam);
        }

        /// <summary>
        /// item是Unity侧的item对象，在这里创建相应的UI对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddItemViewComponent<T>(this UILoopListView2 self, LoopListViewItem2 item) where T : Entity, IOnCreate, IOnEnable, IAwake
        {
            //保证名字不能相同 不然没法cache
            item.gameObject.name = item.gameObject.name + item.ItemId;
            T t = self.AddUIComponentNotCreate<T>(item.gameObject.name);
            t.AddUIComponent<UITransform, Transform>("", item.transform);
            UIWatcherComponent.Instance.OnCreate(t);
            return t;
        }

        /// <summary>
        /// 根据Unity侧item获取UI侧的item
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetUIItemView<T>(this UILoopListView2 self, LoopListViewItem2 item) where T : Entity
        {
            return self.GetUIComponent<T>(item.gameObject.name);
        }

        /// <summary>
        /// itemCount重设item的数量，resetPos是否刷新当前显示的位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemCount">重设item的数量</param>
        /// <param name="resetPos">是否刷新当前显示的位置</param>
        public static void SetListItemCount(this UILoopListView2 self, int itemCount, bool resetPos = true)
        {
            self.ActivatingComponent();
            self.loopListView.SetListItemCount(itemCount, resetPos);
        }

        /// <summary>
        /// 获取当前index对应的item 没有显示的话返回null
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        public static LoopListViewItem2 GetShownItemByItemIndex(this UILoopListView2 self, int itemIndex)
        {
            self.ActivatingComponent();
            return self.loopListView.GetShownItemByItemIndex(itemIndex);
        }

        /// <summary>
        /// 刷新当前能看到的所有元素
        /// </summary>
        /// <param name="self"></param>
        public static void RefreshAllShownItem(this UILoopListView2 self)
        {
            self.ActivatingComponent();
            self.loopListView.RefreshAllShownItem();
        }

        public static void SetOnBeginDragAction(this UILoopListView2 self, Action callback)
        {
            self.ActivatingComponent();
            self.loopListView.mOnBeginDragAction = callback;
        }

        public static void SetOnDragingAction(this UILoopListView2 self, Action callback)
        {
            self.ActivatingComponent();
            self.loopListView.mOnDragingAction = callback;
        }

        public static void SetOnEndDragAction(this UILoopListView2 self, Action callback)
        {
            self.ActivatingComponent();
            self.loopListView.mOnEndDragAction = callback;
        }

        /// <summary>
        /// 跳转到指定index的item位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        public static void MovePanelToItemIndex(this UILoopListView2 self, int index, float offset = 0)
        {
            self.ActivatingComponent();
            self.loopListView.MovePanelToItemIndex(index, offset);
        }

        /// <summary>
        /// 需要开启Snap，当前聚焦元素切换时
        /// </summary>
        /// <param name="self"></param>
        /// <param name="callback"></param>
        public static void SetOnSnapChange(this UILoopListView2 self, Action<LoopListView2, LoopListViewItem2> callback)
        {
            self.ActivatingComponent();
            self.loopListView.mOnSnapNearestChanged = callback;
        }
    }
}