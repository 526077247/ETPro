namespace ET
{
    public class ActionControlActiveWatcherAttribute : BaseAttribute
    {
        public int ActionControlType { get; }
        public bool IsAdd{ get; }

        public ActionControlActiveWatcherAttribute(int subType,bool isAdd)
        {
            this.ActionControlType = subType;
            this.IsAdd = isAdd;
        }
    }
}