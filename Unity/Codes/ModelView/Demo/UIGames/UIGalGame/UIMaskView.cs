using UnityEngine;
namespace ET
{
    public class UIMaskView:Entity,IAwake,IOnCreate,IOnEnable<string,float,bool>
    {
        public static string PrefabPath => "UIGames/UIGalGame/Prefabs/UIMaskView.prefab";
        public UIImage bg;
        public UIImage bg2;
        
        public Vector2 MaxSize;
    }
}