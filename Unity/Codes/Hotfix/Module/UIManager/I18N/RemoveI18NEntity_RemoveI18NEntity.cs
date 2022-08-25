namespace ET
{
    public class RemoveI18NEntity_RemoveI18NEntity: AEvent<UIEventType.RemoveI18NEntity>
    {
        protected override void Run(UIEventType.RemoveI18NEntity args)
        {
            I18NComponent.Instance.RemoveI18NEntity(args.entity);
        }
    }
}