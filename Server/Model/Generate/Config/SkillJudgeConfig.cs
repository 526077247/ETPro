using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class SkillJudgeConfigCategory : ProtoObject, IMerge
    {
        public static SkillJudgeConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, SkillJudgeConfig> dict = new Dictionary<int, SkillJudgeConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<SkillJudgeConfig> list = new List<SkillJudgeConfig>();
		
        public SkillJudgeConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            SkillJudgeConfigCategory s = o as SkillJudgeConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                SkillJudgeConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public SkillJudgeConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SkillJudgeConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillJudgeConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SkillJudgeConfig> GetAll()
        {
            return this.dict;
        }
        public List<SkillJudgeConfig> GetAllList()
        {
            return this.list;
        }
        public SkillJudgeConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class SkillJudgeConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>碰撞体类型类型(0固定位置碰撞体1固定朝向碰撞体2指定位置飞行碰撞体3朝向飞行碰撞体（锁定）4目标立刻结算)</summary>
		[ProtoMember(2)]
		public int ColliderType { get; set; }
		/// <summary>起始位置（1自身，2目标，3鼠标位置）</summary>
		[ProtoMember(3)]
		public int StartPosType { get; set; }
		/// <summary>碰撞体持续时间（单位:毫秒）</summary>
		[ProtoMember(4)]
		public int Time { get; set; }
		/// <summary>碰撞体形状(0.立即判断;1.矩形;2.圆形；3.扇形)</summary>
		[ProtoMember(5)]
		public int ColliderShape { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.Double, AllowTruncation = true)]
		/// <summary>碰撞体形状参数(m)</summary>
		[ProtoMember(6)]
		public float[] ColliderPara { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.Double, AllowTruncation = true)]
		/// <summary>速度（m/s）</summary>
		[ProtoMember(7)]
		public float Speed { get; set; }

	}
}
