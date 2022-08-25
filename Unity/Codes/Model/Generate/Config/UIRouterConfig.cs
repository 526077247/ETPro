using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class UIRouterConfigCategory : ProtoObject, IMerge
    {
        public static UIRouterConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, UIRouterConfig> dict = new Dictionary<int, UIRouterConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<UIRouterConfig> list = new List<UIRouterConfig>();
		
        public UIRouterConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            UIRouterConfigCategory s = o as UIRouterConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                UIRouterConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public UIRouterConfig Get(int id)
        {
            this.dict.TryGetValue(id, out UIRouterConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (UIRouterConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, UIRouterConfig> GetAll()
        {
            return this.dict;
        }
        public List<UIRouterConfig> GetAllList()
        {
            return this.list;
        }
        public UIRouterConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class UIRouterConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>从哪个界面</summary>
		[ProtoMember(2)]
		public string From { get; set; }
		/// <summary>到哪个界面</summary>
		[ProtoMember(3)]
		public string To { get; set; }
		/// <summary>点击节点位置</summary>
		[ProtoMember(4)]
		public string Path { get; set; }
		/// <summary>聚焦方式(0矩形，1圆形)</summary>
		[ProtoMember(5)]
		public int Type { get; set; }

	}
}
