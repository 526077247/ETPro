namespace ET
{
    public class AddBuffWatcherAttribute : BaseAttribute
    {
        public int BuffSubType { get; }

        public AddBuffWatcherAttribute(int subType)
        {
            this.BuffSubType = subType;
        }
    }
}