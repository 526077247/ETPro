using UnityEngine;

namespace ET
{
    public class FocuGameObejct_FocuGameObejct : AEvent<UIEventType.FocuGameObejct>
    {
        protected override void Run(UIEventType.FocuGameObejct args)
        {
            if (args.Win == null)
            {
                UIManagerComponent.Instance.CloseWindow<UIGuidanceView>().Coroutine();
            }
            else
            {
                var view = args.Win.GetComponent(args.Win.ViewType);
                if (view != null)
                {
                    UIManagerComponent.Instance.OpenWindow<UIGuidanceView, GameObject, int>(UIGuidanceView.PrefabPath,
                        view.GetTransform().Find(args.Path).gameObject, args.Type, UILayerNames.TopLayer).Coroutine();
                }
            }
        }
    }
}