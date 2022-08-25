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
    [FriendClass(typeof(UILoopGridView))]
    public class UILoopGridViewDestorySystem : OnDestroySystem<UILoopGridView>
    {
        public override void OnDestroy(UILoopGridView self)
        {
            self.unity_uiloopgridview?.ClearListView();
            self.unity_uiloopgridview = null;
        }
    }
    [FriendClass(typeof(UILoopGridView))]
    public static class UILoopGridViewSystem
    {
        public static void ActivatingComponent(this UILoopGridView self)
        {
            if (self.unity_uiloopgridview == null)
            {
                self.unity_uiloopgridview = self.GetGameObject().GetComponent<LoopGridView>();
                if (self.unity_uiloopgridview == null)
                {
                    Log.Error($"添加UI侧组件UILoopGridView时，物体{ self.GetGameObject().name}上没有找到LoopGridView组件");
                }
            }
        }
        public static void InitGridView(this UILoopGridView self,int itemTotalCount,
                System.Func<LoopGridView, int, int, int, LoopGridViewItem> onGetItemByRowColumn,
                LoopGridViewSettingParam settingParam = null,
                LoopGridViewInitParam initParam = null)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.InitGridView(itemTotalCount, onGetItemByRowColumn, settingParam, initParam);
        }

        //item是Unity侧的item对象，在这里创建相应的UI对象
        public static void AddItemViewComponent<T>(this UILoopGridView self, LoopGridViewItem item) where T : Entity,IAwake,IOnEnable
        {
            //保证名字不能相同 不然没法cache
            item.gameObject.name = item.gameObject.name + item.ItemId;
            T t = self.AddUIComponentNotCreate<T>(item.gameObject.name);
            t.AddUIComponent<UITransform,Transform>("",item.transform);
            UIWatcherComponent.Instance.OnCreate(t);
        }

        //根据Unity侧item获取UI侧的item
        public static T GetUIItemView<T>(this UILoopGridView self, LoopGridViewItem item) where T : Entity,IAwake
        {
            return self.GetUIComponent<T>(item.gameObject.name);
        }
        //itemCount重设item的数量，resetPos是否刷新当前显示的位置
        public static void SetListItemCount(this UILoopGridView self, int itemCount, bool resetPos = true)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.SetListItemCount(itemCount, resetPos);
        }

        //获取当前index对应的item 没有显示的话返回null
        public static LoopGridViewItem GetShownItemByItemIndex(this UILoopGridView self, int itemIndex)
        {
            self.ActivatingComponent();
            return self.unity_uiloopgridview.GetShownItemByItemIndex(itemIndex);
        }

        public static void MovePanelToItemByRowColumn(this UILoopGridView self, int row, int column, int offsetX = 0, int offsetY = 0)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.MovePanelToItemByRowColumn(row, column, offsetX, offsetY);
        }


        public static void RefreshAllShownItem(this UILoopGridView self)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.RefreshAllShownItem();
        }

        public static void SetItemSize(this UILoopGridView self, Vector2 sizeDelta)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.SetItemSize(sizeDelta);
        }

        public static void SetOnBeginDragAction(this UILoopGridView self, Action<PointerEventData> callback)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.mOnBeginDragAction = callback;
        }

        public static void SetOnDragingAction(this UILoopGridView self, Action<PointerEventData> callback)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.mOnDragingAction = callback;
        }

        public static void SetOnEndDragAction(this UILoopGridView self, Action<PointerEventData> callback)
        {
            self.ActivatingComponent();
            self.unity_uiloopgridview.mOnEndDragAction = callback;
        }

    }
}
