namespace ET
{
    public class MoveBuffWatcherAttribute: BaseAttribute
    {
        public int BuffSubType { get; }

        public MoveBuffWatcherAttribute(int subType)
        {
            this.BuffSubType = subType;
        }
    }
}