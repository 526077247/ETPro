
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
        /// 当前步骤
        /// </summary>
        public int NextSkillStep;

        public int CurSkillConfigId;//当前技能Id
        /// <summary>
        /// 当前参数
        /// </summary>
        public SkillPara Para;

        public long TimerId;

        public bool Enable { get; set; } = true;
    }
}