namespace ET
{
    public class SkillWatcherAttribute : BaseAttribute
    {
        public int SkillStepType { get; }

        public SkillWatcherAttribute(int type)
        {
            this.SkillStepType = type;
        }
    }
}