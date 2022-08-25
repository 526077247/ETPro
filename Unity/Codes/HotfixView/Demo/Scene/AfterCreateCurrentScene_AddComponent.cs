namespace ET
{
    public class AfterCreateCurrentScene_AddComponent: AEvent<EventType.AfterCreateCurrentScene>
    {
        protected override void Run(EventType.AfterCreateCurrentScene args)
        {
            Scene zoneScene = args.CurrentScene;

        }
    }
}