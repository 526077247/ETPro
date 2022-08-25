using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class GuidanceConfigCategory : ProtoObject, IMerge
    {
        public static GuidanceConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, GuidanceConfig> dict = new Dictionary<int, GuidanceConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<GuidanceConfig> list = new List<GuidanceConfig>();
		
        public GuidanceConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            GuidanceConfigCategory s = o as GuidanceConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                GuidanceConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public GuidanceConfig Get(int id)
        {
            this.dict.TryGetValue(id, out GuidanceConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (GuidanceConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, GuidanceConfig> GetAll()
        {
            return this.dict;
        }
        public List<GuidanceConfig> GetAllList()
        {
            return this.list;
        }
        public GuidanceConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class GuidanceConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>引导组</summary>
		[ProtoMember(2)]
		public int Group { get; set; }
		/// <summary>步骤类型（0路由打开某界面1聚焦游戏物体）</summary>
		[ProtoMember(3)]
		public int Steptype { get; set; }
		/// <summary>引导步骤优先级</summary>
		[ProtoMember(4)]
		public int Steporder { get; set; }
		/// <summary>通知完成事件名</summary>
		[ProtoMember(5)]
		public string Event { get; set; }
		/// <summary>参数1</summary>
		[ProtoMember(6)]
		public string Value1 { get; set; }
		/// <summary>参数2</summary>
		[ProtoMember(7)]
		public string Value2 { get; set; }
		/// <summary>参数3</summary>
		[ProtoMember(8)]
		public string Value3 { get; set; }
		/// <summary>是否关键步骤（每组都有且只能有1个）</summary>
		[ProtoMember(9)]
		public int KeyStep { get; set; }
		/// <summary>开启条件（关键步骤配此字段生效）</summary>
		[ProtoMember(10)]
		public string Condition { get; set; }
		/// <summary>引导组优先级（关键步骤配此字段生效）</summary>
		[ProtoMember(11)]
		public int Grouporder { get; set; }
		/// <summary>服务端需下发道具（关键步骤配此字段生效）</summary>
		[ProtoMember(12)]
		public int[] PreAward { get; set; }
		/// <summary>是否开启（关键步骤配此字段生效）</summary>
		[ProtoMember(13)]
		public int Open { get; set; }
		/// <summary>是否所有玩家共享（关键步骤配此字段生效）</summary>
		[ProtoMember(14)]
		public int Share { get; set; }

	}
}
