namespace ET
{
    public class PlaySound_PlaySound : AEvent<EventType.PlaySound>
    {
        protected override void Run(EventType.PlaySound args)
        {
            SoundComponent.Instance.PlaySound(args.Path);
        }
    }
}