using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class SkillColliderComponent:Entity,IAwake<SkillPara>,IAwake<SkillPara,long>,IAwake<SkillPara,Vector3>,ITransfer
    {
        public int ConfigId;
        [BsonIgnore]
        public SkillJudgeConfig Config => SkillJudgeConfigCategory.Instance.Get(ConfigId);
        
        /// <summary>
        /// 来源Id
        /// </summary>
        public long FromId { get; set; }

        /// <summary>
        /// 目标Id
        /// </summary>
        public long ToId{ get; set; }

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 Position{ get; set; }
        /// <summary>
        /// 创建逻辑触发器时间，非显示View的时间
        /// </summary>
        public long CreateColliderTime { get; set; }
        /// <summary>
        /// 创建View的时间
        /// </summary>
        public long CreateViewTime { get; set; }
        [BsonIgnore]
        public Unit Unit => this.GetParent<Unit>();

        public SkillStepPara Para;
        
        public List<int> CostId;
        public List<int> Cost;

        public int SkillConfigId;
        [BsonIgnore] 
        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(SkillConfigId);
    }
}