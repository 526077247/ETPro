namespace ET
{
    public class LoadingProgressEvent_RefreshLoadingUI : AEvent<UIEventType.LoadingProgress>
    {
        protected override void Run(UIEventType.LoadingProgress args)
        {
            if(UILoadingView.Instance!=null)
                UILoadingView.Instance.SetSlidValue(args.Progress);
        }
    }
}
