using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class MapSceneConfigCategory : ProtoObject, IMerge
    {
        public static MapSceneConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, MapSceneConfig> dict = new Dictionary<int, MapSceneConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<MapSceneConfig> list = new List<MapSceneConfig>();
		
        public MapSceneConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            MapSceneConfigCategory s = o as MapSceneConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                MapSceneConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public MapSceneConfig Get(int id)
        {
            this.dict.TryGetValue(id, out MapSceneConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (MapSceneConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, MapSceneConfig> GetAll()
        {
            return this.dict;
        }
        public List<MapSceneConfig> GetAllList()
        {
            return this.list;
        }
        public MapSceneConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class MapSceneConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>UnityScene名字</summary>
		[ProtoMember(2)]
		public string Name { get; set; }
		/// <summary>加载的Area数据表名（不需要做无缝则不填）</summary>
		[ProtoMember(3)]
		public string Area { get; set; }
		/// <summary>寻路文件名</summary>
		[ProtoMember(4)]
		public string Recast { get; set; }

	}
}
