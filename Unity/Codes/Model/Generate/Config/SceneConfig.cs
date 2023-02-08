using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class SceneConfigCategory : ProtoObject, IMerge
    {
        public static SceneConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, SceneConfig> dict = new Dictionary<int, SceneConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<SceneConfig> list = new List<SceneConfig>();
		
        public SceneConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            SceneConfigCategory s = o as SceneConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                SceneConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public SceneConfig Get(int id)
        {
            this.dict.TryGetValue(id, out SceneConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (SceneConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, SceneConfig> GetAll()
        {
            return this.dict;
        }
        public List<SceneConfig> GetAllList()
        {
            return this.list;
        }
        public SceneConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class SceneConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>Name</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>SceneAddress</summary>
		[ProtoMember(3)]
		public string SceneAddress { get; set; }

	}
}
