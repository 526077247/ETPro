namespace ET
{
    public class SetActive_SetTransformActive : AEvent<UIEventType.SetActive>
    {
        protected override void Run(UIEventType.SetActive args)
        {
            var obj = args.entity.GetGameObject();
            if(obj!=null&&obj.activeSelf!=args.Active)
                obj.SetActive(args.Active);
        }
    }
}