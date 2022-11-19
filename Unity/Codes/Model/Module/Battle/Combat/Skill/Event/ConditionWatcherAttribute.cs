namespace ET
{
    public class ConditionWatcherAttribute : BaseAttribute
    {
        public string ConditionType { get; }

        public ConditionWatcherAttribute(string type)
        {
            this.ConditionType = type;
        }
    }
}