using SuperScrollView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET
{
    [UISystem]
    [FriendClass(typeof (UILoopGridView))]
    public class UILoopGridViewDestorySystem: OnDestroySystem<UILoopGridView>
    {
        public override void OnDestroy(UILoopGridView self)
        {
            self.loopGridView?.ClearListView();
            self.loopGridView = null;
        }
    }

    [FriendClass(typeof (UILoopGridView))]
    public static class UILoopGridViewSystem
    {
        public static void ActivatingComponent(this UILoopGridView self)
        {
            if (self.loopGridView == null)
            {
                self.loopGridView = self.GetGameObject().GetComponent<LoopGridView>();
                if (self.loopGridView == null)
                {
                    Log.Error($"添加UI侧组件UILoopGridView时，物体{self.GetGameObject().name}上没有找到LoopGridView组件");
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemTotalCount"></param>
        /// <param name="onGetItemByRowColumn"></param>
        /// <param name="settingParam"></param>
        /// <param name="initParam"></param>
        public static void InitGridView(this UILoopGridView self, int itemTotalCount,
        System.Func<LoopGridView, int, int, int, LoopGridViewItem> onGetItemByRowColumn,
        LoopGridViewSettingParam settingParam = null,
        LoopGridViewInitParam initParam = null)
        {
            self.ActivatingComponent();
            self.loopGridView.InitGridView(itemTotalCount, onGetItemByRowColumn, settingParam, initParam);
        }

        /// <summary>
        /// item是Unity侧的item对象，在这里创建相应的UI对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddItemViewComponent<T>(this UILoopGridView self, LoopGridViewItem item) where T : Entity, IAwake, IOnEnable
        {
            //保证名字不能相同 不然没法cache
            item.gameObject.name = item.gameObject.name + item.ItemId;
            T t = self.AddUIComponentNotCreate<T>(item.gameObject.name);
            t.AddUIComponent<UITransform, Transform>("", item.transform);
            UIWatcherComponent.Instance.OnCreate(t);
        }

        /// <summary>
        /// 根据Unity侧item获取UI侧的item
        /// </summary>
        /// <param name="self"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetUIItemView<T>(this UILoopGridView self, LoopGridViewItem item) where T : Entity, IAwake
        {
            return self.GetUIComponent<T>(item.gameObject.name);
        }

        /// <summary>
        /// itemCount重设item的数量，resetPos是否刷新当前显示的位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemCount">重设item的数量</param>
        /// <param name="resetPos">是否刷新当前显示的位置</param>
        public static void SetListItemCount(this UILoopGridView self, int itemCount, bool resetPos = true)
        {
            self.ActivatingComponent();
            self.loopGridView.SetListItemCount(itemCount, resetPos);
        }

        /// <summary>
        /// 获取当前index对应的item 没有显示的话返回null
        /// </summary>
        /// <param name="self"></param>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        public static LoopGridViewItem GetShownItemByItemIndex(this UILoopGridView self, int itemIndex)
        {
            self.ActivatingComponent();
            return self.loopGridView.GetShownItemByItemIndex(itemIndex);
        }

        /// <summary>
        /// 跳转到指定行列的元素位置
        /// </summary>
        /// <param name="self"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        public static void MovePanelToItemByRowColumn(this UILoopGridView self, int row, int column, int offsetX = 0, int offsetY = 0)
        {
            self.ActivatingComponent();
            self.loopGridView.MovePanelToItemByRowColumn(row, column, offsetX, offsetY);
        }

        /// <summary>
        /// 刷新当前能看到的所有元素
        /// </summary>
        /// <param name="self"></param>
        public static void RefreshAllShownItem(this UILoopGridView self)
        {
            self.ActivatingComponent();
            self.loopGridView.RefreshAllShownItem();
        }

        /// <summary>
        /// 重设子元素大小
        /// </summary>
        /// <param name="self"></param>
        /// <param name="sizeDelta"></param>
        public static void SetItemSize(this UILoopGridView self, Vector2 sizeDelta)
        {
            self.ActivatingComponent();
            self.loopGridView.SetItemSize(sizeDelta);
        }

        public static void SetOnBeginDragAction(this UILoopGridView self, Action<PointerEventData> callback)
        {
            self.ActivatingComponent();
            self.loopGridView.mOnBeginDragAction = callback;
        }

        public static void SetOnDragingAction(this UILoopGridView self, Action<PointerEventData> callback)
        {
            self.ActivatingComponent();
            self.loopGridView.mOnDragingAction = callback;
        }

        public static void SetOnEndDragAction(this UILoopGridView self, Action<PointerEventData> callback)
        {
            self.ActivatingComponent();
            self.loopGridView.mOnEndDragAction = callback;
        }
    }
}