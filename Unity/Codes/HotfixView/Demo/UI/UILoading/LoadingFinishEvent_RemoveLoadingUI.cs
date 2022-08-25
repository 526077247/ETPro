namespace ET
{
    public class LoadingFinishEvent_RemoveLoadingUI : AEventAsync<UIEventType.LoadingFinish>
    {
        protected override async ETTask Run(UIEventType.LoadingFinish args)
        {
            //await UIHelper.Remove(args.Scene, UIType.UILoading);
            //UIManagerComponent.Instance.DestroyWindow<UILoadingView>();//Destroy掉的才能被销毁
            await ETTask.CompletedTask;
        }
    }
}
