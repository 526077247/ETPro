using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class EffectConfigCategory : ProtoObject, IMerge
    {
        public static EffectConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, EffectConfig> dict = new Dictionary<int, EffectConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<EffectConfig> list = new List<EffectConfig>();
		
        public EffectConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            EffectConfigCategory s = o as EffectConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                EffectConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public EffectConfig Get(int id)
        {
            this.dict.TryGetValue(id, out EffectConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (EffectConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, EffectConfig> GetAll()
        {
            return this.dict;
        }
        public List<EffectConfig> GetAllList()
        {
            return this.list;
        }
        public EffectConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class EffectConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>路径</summary>
		[ProtoMember(2)]
		public string Prefab { get; set; }
		/// <summary>播放时间（-1表示无限，由其他逻辑控制）</summary>
		[ProtoMember(3)]
		public int PlayTime { get; set; }
		/// <summary>特效挂点的索引</summary>
		[ProtoMember(4)]
		public string MountPoint { get; set; }
		/// <summary>是否挂载，为0的话在被挂载者挂载当时所在位置播放</summary>
		[ProtoMember(5)]
		public int IsMount { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.Double, AllowTruncation = true)]
		/// <summary>相对位置</summary>
		[ProtoMember(6)]
		public float[] RelativePos { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.Double, AllowTruncation = true)]
		/// <summary>偏转角度</summary>
		[ProtoMember(7)]
		public float[] RelativeRotation { get; set; }

	}
}
