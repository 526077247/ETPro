namespace ET
{
    public class AddComponent_SetUITransform : AEvent<UIEventType.AddComponent>
    {
        protected override void Run(UIEventType.AddComponent args)
        {
            if (!(args.entity is UITransform))
            {
                args.entity.AddUIComponent<UITransform>("");
            }
        }
    }
}