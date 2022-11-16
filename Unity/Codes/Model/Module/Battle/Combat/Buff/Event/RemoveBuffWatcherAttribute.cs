namespace ET
{
    public class RemoveBuffWatcherAttribute: BaseAttribute
    {
        public int BuffSubType { get; }

        public RemoveBuffWatcherAttribute(int subType)
        {
            this.BuffSubType = subType;
        }
    }
}