using System;
using UnityEngine;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    public class SkillStepPara
    {
        public int Index;
        public object[] Paras;
        public int Interval;
        public int Count;//作用单位数
    }

    [ComponentOf(typeof(SpellComponent))]
    public class SkillPara:Entity,IDestroy,IAwake,ITransfer
    {

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        [BsonIgnore]
        public CombatUnitComponent From => this.Parent.Parent.Parent.Parent.GetChild<Unit>(FromId)?.GetComponent<CombatUnitComponent>();
        [BsonIgnore]
        public CombatUnitComponent To => this.Parent.Parent.Parent.Parent.GetChild<Unit>(ToId)?.GetComponent<CombatUnitComponent>();
        public long FromId;
        public long ToId;
        public List<int> CostId { get; } = new List<int>();
        public List<int> Cost { get; } = new List<int>();
        public int SkillConfigId { get; set; }

        [BsonIgnore]
        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(this.SkillConfigId);
        public int CurIndex{ get; set; }
        public string CurGroup{ get; set; }
        #region 步骤参数
        [BsonIgnore]
        public MultiDictionary<string,int,SkillStepPara> GroupStepPara { get; }= new MultiDictionary<string,int,SkillStepPara>();
        #endregion

    }
}