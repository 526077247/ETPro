using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    [Config]
    public partial class RoleExpressionConfigCategory : ProtoObject, IMerge
    {
        public static RoleExpressionConfigCategory Instance;
		
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, RoleExpressionConfig> dict = new Dictionary<int, RoleExpressionConfig>();
		
        [BsonElement]
        [ProtoMember(1)]
        private List<RoleExpressionConfig> list = new List<RoleExpressionConfig>();
		
        public RoleExpressionConfigCategory()
        {
            Instance = this;
        }
        
        public void Merge(object o)
        {
            RoleExpressionConfigCategory s = o as RoleExpressionConfigCategory;
            this.list.AddRange(s.list);
        }
		
        public override void EndInit()
        {
            for(int i =0 ;i<list.Count;i++)
            {
                RoleExpressionConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
		
        public RoleExpressionConfig Get(int id)
        {
            this.dict.TryGetValue(id, out RoleExpressionConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，配置表名: {nameof (RoleExpressionConfig)}，配置id: {id}");
            }

            return item;
        }
		
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, RoleExpressionConfig> GetAll()
        {
            return this.dict;
        }
        public List<RoleExpressionConfig> GetAllList()
        {
            return this.list;
        }
        public RoleExpressionConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
	public partial class RoleExpressionConfig: ProtoObject, IConfig
	{
		/// <summary>Id</summary>
		[ProtoMember(1)]
		public int Id { get; set; }
		/// <summary>角色名多语言不能重复）</summary>
		[ProtoMember(2)]
		public string NameKey { get; set; }
		/// <summary>表情类型</summary>
		[ProtoMember(3)]
		public string Expression { get; set; }
		/// <summary>图片ab包路径</summary>
		[ProtoMember(4)]
		public string Path { get; set; }

	}
}
