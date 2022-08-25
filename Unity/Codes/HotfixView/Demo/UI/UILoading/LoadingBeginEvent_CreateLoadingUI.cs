using UnityEngine;

namespace ET
{
    public class LoadingBeginEvent_CreateLoadingUI : AEventAsync<UIEventType.LoadingBegin>
    {
        protected override async ETTask Run(UIEventType.LoadingBegin args)
        {
            //await UIHelper.Create(args.Scene, UIType.UILoading);
            await UIManagerComponent.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
        }
    }
}
