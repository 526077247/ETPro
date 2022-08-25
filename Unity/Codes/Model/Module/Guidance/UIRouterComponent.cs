namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class UIRouterComponent:Entity,IAwake
    {
        public static UIRouterComponent Instance;
        public MultiDictionary<string, string, UIRouterConfig> RouterMapPath;
    }
}