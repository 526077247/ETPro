using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BuffChantConfigCategory : ProtoObject, IMerge
    {
        public static BuffChantConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BuffChantConfig> dict = new Dictionary<int, BuffChantConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BuffChantConfig> list = new List<BuffChantConfig>();
		
        public BuffChantConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            BuffChantConfigCategory s = o as BuffChantConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                BuffChantConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public BuffChantConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffChantConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffChantConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffChantConfig> GetAll()
        {
            return this.dict;
        }
        public List<BuffChantConfig> GetAllList()
        {
            return this.list;
        }
        public BuffChantConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BuffChantConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>是否移动打断</summary>
		[ProtoMember(2)]
		public int MoveInterrupt { get; set; }
		/// <summary>是否受击打断</summary>
		[ProtoMember(3)]
		public int DamageInterrupt { get; set; }
		/// <summary>打断返回冷却千分比</summary>
		[ProtoMember(4)]
		public int BackCoolingPermillage { get; set; }
		/// <summary>打断返回消耗千分比</summary>
		[ProtoMember(5)]
		public int BackCostPermillage { get; set; }

	}
}
