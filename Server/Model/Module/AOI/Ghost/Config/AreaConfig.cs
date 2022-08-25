using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    public partial class AreaConfigCategory : ProtoObject
    {
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<long, AreaConfig> dict = new Dictionary<long, AreaConfig>();

        [BsonElement]
        [ProtoMember(1)]
        private List<AreaConfig> list = new List<AreaConfig>();

        [ProtoIgnore]
        [BsonIgnore]
        public bool IsOrdered;
        public AreaConfigCategory()
        {
            IsOrdered = false;
        }

        public override void EndInit()
        {
            for (int i = 0; i < list.Count; i++)
            {
                AreaConfig config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
        
        public AreaConfig Get(long id)
        {
            this.dict.TryGetValue(id, out AreaConfig item);

            if (item == null)
            {
                throw new Exception($"配置找不到，{nameof(AreaConfig)}，配置id: {id}");
            }

            return item;
        }
        public bool TryGet(long id, out AreaConfig item)
        {
            return this.dict.TryGetValue(id, out item);
        }
        public bool Contain(long id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<long, AreaConfig> GetAll()
        {
            return this.dict;
        }
        public List<AreaConfig> GetAllList()
        {
            return this.list;
        }
        public AreaConfig GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
    public partial class AreaConfig : ProtoObject
    {
        [ProtoMember(1)]
        public long Id { get; set; }
        [ProtoMember(2)]
        public int SceneId { get; set; }

    }
}
