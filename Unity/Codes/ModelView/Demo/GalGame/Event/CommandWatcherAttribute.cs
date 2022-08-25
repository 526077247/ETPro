namespace ET
{
    public class CommandWatcherAttribute : BaseAttribute
    {
        public string Command { get; }

        public CommandWatcherAttribute(string type)
        {
            this.Command = type;
        }
    }
}