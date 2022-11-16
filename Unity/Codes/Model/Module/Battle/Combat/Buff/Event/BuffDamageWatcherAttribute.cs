namespace ET
{
    public class BuffDamageWatcherAttribute : BaseAttribute
    {
        public int BuffSubType { get; }

        public BuffDamageWatcherAttribute(int subType)
        {
            this.BuffSubType = subType;
        }
    }
}