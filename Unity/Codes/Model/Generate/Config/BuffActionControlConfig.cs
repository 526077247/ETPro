using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BuffActionControlConfigCategory : ProtoObject, IMerge
    {
        public static BuffActionControlConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BuffActionControlConfig> dict = new Dictionary<int, BuffActionControlConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BuffActionControlConfig> list = new List<BuffActionControlConfig>();
		
        public BuffActionControlConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            BuffActionControlConfigCategory s = o as BuffActionControlConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                BuffActionControlConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public BuffActionControlConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffActionControlConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffActionControlConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffActionControlConfig> GetAll()
        {
            return this.dict;
        }
        public List<BuffActionControlConfig> GetAllList()
        {
            return this.list;
        }
        public BuffActionControlConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BuffActionControlConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>行为禁制</summary>
		[ProtoMember(2)]
		public int[] ActionControl { get; set; }

	}
}
