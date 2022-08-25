namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class PlayerComponent: Entity, IAwake,IDestroy
    {

        public static PlayerComponent Instance;
        public long MyId { get; set; }
        
        public string Account { get; set; }
    }
}