namespace ET
{
    [ComponentOf(typeof(AOISceneComponent))]
    public class AreaComponent:Entity,IAwake<string>
    {
        public AreaConfigCategory AreaConfigCategory;
    }
}