namespace ET
{
    public class SceneChangeFinishEventAsyncCreateUIHelp : AEventAsync<EventType.SceneChangeFinish>
    {
        protected override async ETTask Run(EventType.SceneChangeFinish args)
        {
            await UIManagerComponent.Instance.OpenWindow<UIHelpWin>(UIHelpWin.PrefabPath);
            await UIManagerComponent.Instance.DestroyWindow<UILoadingView>();
            args.CurrentScene.AddComponent<OperaComponent>();
        }
    }
}
