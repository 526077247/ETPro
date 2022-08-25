using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BuffBleedConfigCategory : ProtoObject, IMerge
    {
        public static BuffBleedConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BuffBleedConfig> dict = new Dictionary<int, BuffBleedConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BuffBleedConfig> list = new List<BuffBleedConfig>();
		
        public BuffBleedConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            BuffBleedConfigCategory s = o as BuffBleedConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                BuffBleedConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public BuffBleedConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffBleedConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffBleedConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffBleedConfig> GetAll()
        {
            return this.dict;
        }
        public List<BuffBleedConfig> GetAllList()
        {
            return this.list;
        }
        public BuffBleedConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BuffBleedConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>计算公式</summary>
		[ProtoMember(2)]
		public int FormulaId { get; set; }
		/// <summary>出血间隔</summary>
		[ProtoMember(3)]
		public int CD { get; set; }

	}
}
