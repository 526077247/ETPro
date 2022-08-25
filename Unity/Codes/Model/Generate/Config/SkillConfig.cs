using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class SkillConfigCategory : ProtoObject, IMerge
    {
        public static SkillConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, SkillConfig> dict = new Dictionary<int, SkillConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<SkillConfig> list = new List<SkillConfig>();
		
        public SkillConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            SkillConfigCategory s = o as SkillConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                SkillConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public SkillConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SkillConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SkillConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SkillConfig> GetAll()
        {
            return this.dict;
        }
        public List<SkillConfig> GetAllList()
        {
            return this.list;
        }
        public SkillConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class SkillConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>伤害作用对象(0自身1己方2敌方)</summary>
		[ProtoMember(2)]
		public int DamageTarget { get; set; }
		/// <summary>名字</summary>
		[ProtoMember(3)]
		public string Name { get; set; }
		/// <summary>图标</summary>
		[ProtoMember(4)]
		public string Icon { get; set; }
		/// <summary>稀有度</summary>
		[ProtoMember(5)]
		public int RareLv { get; set; }
		/// <summary>可用等级</summary>
		[ProtoMember(6)]
		public int Lv { get; set; }
		/// <summary>描述</summary>
		[ProtoMember(7)]
		public string Description { get; set; }
		/// <summary>冷却时间</summary>
		[ProtoMember(8)]
		public int CDTime { get; set; }
		/// <summary>施法模式（0：距离不够则选最大施法范围ps选目标的则不施法;1:距离不够走到最远距离施法）</summary>
		[ProtoMember(9)]
		public int Mode { get; set; }
		/// <summary>技能预览类型(0大圈选一个目标，1大圈选小圈，2从脚底出发指向型……)</summary>
		[ProtoMember(10)]
		public int PreviewType { get; set; }
		/// <summary>技能预览释放范围（0半径；1半径，小圈半径；2，长度，宽度）</summary>
		[ProtoMember(11)]
		public int[] PreviewRange { get; set; }

	}
}
