using SuperScrollView;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UITextMeshPro = TMPro.TMP_Text;
namespace ET
{
    public class UIReview : Entity,IOnWidthPaddingChange,IAwake,IOnCreate,IOnEnable<bool, List<GalGameEngineComponent.ReviewItem>>
    {

        public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIReview.prefab";
        public List<GalGameEngineComponent.ReviewItem> ReviewItems;
        public UILoopListView2 ListView;
        public bool LastAutoPlayState;
        public UIPointerClick bgclick;
        
    }
}
