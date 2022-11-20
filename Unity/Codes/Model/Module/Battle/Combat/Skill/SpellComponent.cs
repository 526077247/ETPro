
using MongoDB.Bson.Serialization.Attributes;
namespace ET
{
    /// <summary>
    /// 技能施法组件
    /// </summary>
    [ComponentOf(typeof(CombatUnitComponent))]
    public class SpellComponent : Entity,IAwake,IDestroy,ITransfer
    {
        /// <summary>
        /// 下一步骤
        /// </summary>
        public int NextSkillStep;

        public int CurSkillConfigId;//当前技能Id

        public long TimerId;

        public bool Enable { get; set; } = true;

        public int WaitStep = SkillStepType.None;//等待的步骤
    }
}