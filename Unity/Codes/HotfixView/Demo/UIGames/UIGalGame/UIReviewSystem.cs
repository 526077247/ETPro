using SuperScrollView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UITextMeshPro = TMPro.TMP_Text;
namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIReview))]
    public class UIReviewOnCreateSystem:OnCreateSystem<UIReview>
    {
        public override void OnCreate(UIReview self)
        {
            self.ListView = self.AddUIComponent<UILoopListView2>("ScrollView");
            self.ListView.InitListView(0, (a,b)=>
            {
                return self.GetItemByIndex(a,b);
            });
            self.bgclick = self.AddUIComponent<UIPointerClick>("ScreenDimmed");
            self.bgclick.SetOnClick(()=>
            {
                self.Close();
            });
        }
    }
    [UISystem]
    [FriendClass(typeof(UIReview))]
    public class UIReviewOnEnableSystem: OnEnableSystem<UIReview,bool,List<GalGameEngineComponent.ReviewItem>>
    {
        public override void OnEnable(UIReview self, bool a, List<GalGameEngineComponent.ReviewItem> b)
        {
            self.LastAutoPlayState = a;
            self.ReviewItems = b;
            self.ListView.SetListItemCount(self.ReviewItems.Count);
            self.ListView.MovePanelToItemIndex(self.ReviewItems.Count-1, 0);
        }
        
    }
    [FriendClass(typeof(UIReview))]
    [FriendClass(typeof(GalGameEngineComponent))]
    public static class UIReviewSystem
    {
        

        public static LoopListViewItem2 GetItemByIndex(this UIReview self,LoopListView2 listView, int index)
        {
            if (index < 0 || index > self.ReviewItems?.Count)
                return null;

            var data = self.ReviewItems[index];
            var item = listView.NewListViewItem("ReviewItem");
            if (!item.IsInitHandlerCalled)
            {
                item.IsInitHandlerCalled = true;
                self.ListView.AddItemViewComponent<UIReviewItem>(item);
            }
            var uiitemview = self.ListView.GetUIItemView<UIReviewItem>(item);
            
            I18NComponent.Instance.I18NTryGetText(data.Name, out data.Name);
            var contents = data.Content.Split(',');
            string showtext = "";
            for (int i = 0; i < contents.Length; i++)
            {
                I18NComponent.Instance.I18NTryGetText(contents[i], out var temp);
                showtext += temp;
            }
            uiitemview.SetData(data.Name,showtext);
            return item;

        }

        public static void Close(this UIReview self)
        {
            if (self.LastAutoPlayState)
            {
                GalGameEngineComponent.Instance.AutoPlay = self.LastAutoPlayState;
            }
            UIManagerComponent.Instance.OpenWindow<UIGalGameHelper>(UIGalGameHelper.PrefabPath).Coroutine();
            self.CloseSelf().Coroutine();
        }
    }
}
