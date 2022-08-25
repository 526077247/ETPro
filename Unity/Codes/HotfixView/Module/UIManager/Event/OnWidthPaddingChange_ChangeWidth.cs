using UnityEngine.UI;
using UnityEngine;
namespace ET
{
    [FriendClass(typeof(UIManagerComponent))]
    public class OnWidthPaddingChange_ChangeWidth: AEvent<UIEventType.OnWidthPaddingChange>
    {
        protected override void Run(UIEventType.OnWidthPaddingChange args)
        {
            var res = UIWatcherComponent.Instance.OnWidthPaddingChange(args.entity as IOnWidthPaddingChange);
            if (!res)
            {
                OnWidthPaddingChange(args.entity);
            }
        }
    
        public void OnWidthPaddingChange(Entity entity)
        {
            var rectTrans = entity.GetTransform().GetComponent<RectTransform>();
            var pandding = UIManagerComponent.Instance.WidthPadding;
            rectTrans.offsetMin = new Vector2(pandding * (1 - rectTrans.anchorMin.x), 0);
            rectTrans.offsetMax = new Vector2(-pandding * rectTrans.anchorMax.x, 0);

        }
    }
}