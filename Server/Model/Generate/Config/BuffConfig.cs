using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BuffConfigCategory : ProtoObject, IMerge
    {
        public static BuffConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BuffConfig> dict = new Dictionary<int, BuffConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BuffConfig> list = new List<BuffConfig>();
		
        public BuffConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            BuffConfigCategory s = o as BuffConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                BuffConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public BuffConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffConfig> GetAll()
        {
            return this.dict;
        }
        public List<BuffConfig> GetAllList()
        {
            return this.list;
        }
        public BuffConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BuffConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>状态名称</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>状态描述</summary>
		[ProtoMember(3)]
		public string Description { get; set; }
		/// <summary>图标路径</summary>
		[ProtoMember(4)]
		public string Icon { get; set; }
		/// <summary>显示在状态栏</summary>
		[ProtoMember(5)]
		public int StatusSlot { get; set; }
		/// <summary>游戏特效表现（0表示无）</summary>
		[ProtoMember(6)]
		public int EffectId { get; set; }
		/// <summary>类型（1属性变化2行为禁制3持续掉血）</summary>
		[ProtoMember(7)]
		public int[] Type { get; set; }
		/// <summary>叠加判别组(同组只取最高优先级)</summary>
		[ProtoMember(8)]
		public int Group { get; set; }
		/// <summary>优先级（数字越小越优先，相同则Id最小优先）</summary>
		[ProtoMember(9)]
		public int Priority { get; set; }

	}
}
