using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class BuffAttrConfigCategory : ProtoObject, IMerge
    {
        public static BuffAttrConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, BuffAttrConfig> dict = new Dictionary<int, BuffAttrConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<BuffAttrConfig> list = new List<BuffAttrConfig>();
		
        public BuffAttrConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            BuffAttrConfigCategory s = o as BuffAttrConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                BuffAttrConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public BuffAttrConfig Get(int id)
        {
            this.dict.TryGetValue(id, out BuffAttrConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (BuffAttrConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, BuffAttrConfig> GetAll()
        {
            return this.dict;
        }
        public List<BuffAttrConfig> GetAllList()
        {
            return this.list;
        }
        public BuffAttrConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class BuffAttrConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>属性修饰</summary>
		[ProtoMember(2)]
		public string[] AttributeType { get; set; }
		/// <summary>修饰参数</summary>
		[ProtoMember(3)]
		public int[] AttributePct { get; set; }
		/// <summary>修饰参数</summary>
		[ProtoMember(4)]
		public int[] AttributeAdd { get; set; }
		/// <summary>修饰参数</summary>
		[ProtoMember(5)]
		public int[] AttributeFinalAdd { get; set; }
		/// <summary>修饰参数</summary>
		[ProtoMember(6)]
		public int[] AttributeFinalPct { get; set; }
		/// <summary>结束后是否移除属性加成（0:是）</summary>
		[ProtoMember(7)]
		public int IsRemove { get; set; }

	}
}
