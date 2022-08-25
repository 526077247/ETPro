using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    [ProtoContract]
    public partial class ChapterCategory : ProtoObject
    {
        public static ChapterCategory Instance;
        
        [ProtoIgnore]
        [BsonIgnore]
        private Dictionary<int, Chapter> dict = new Dictionary<int, Chapter>();

        [BsonElement]
        [ProtoMember(1)]
        private List<Chapter> list = new List<Chapter>();

        [ProtoIgnore]
        [BsonIgnore]
        public bool IsOrdered;
        public ChapterCategory()
        {
            Instance = this;
            IsOrdered = false;
        }

        public override void EndInit()
        {
            for (int i = 0; i < list.Count; i++)
            {
                Chapter config = list[i];
                config.EndInit();
                this.dict.Add(config.Id, config);
            }            
            this.AfterEndInit();
        }
        
        public Chapter Get(int id)
        {
            this.dict.TryGetValue(id, out Chapter item);

            if (item == null)
            {
                throw new Exception($"配置找不到，{nameof(Chapter)}，配置id: {id}");
            }

            return item;
        }
        public bool TryGet(int id, out Chapter item)
        {
            return this.dict.TryGetValue(id, out item);
        }
        public bool Contain(int id)
        {
            return this.dict.ContainsKey(id);
        }

        public Dictionary<int, Chapter> GetAll()
        {
            return this.dict;
        }
        public List<Chapter> GetAllList()
        {
            return this.list;
        }
        public Chapter GetOne()
        {
            if (this.dict == null || this.dict.Count <= 0)
            {
                return null;
            }
            return this.dict.Values.GetEnumerator().Current;
        }
    }

    [ProtoContract]
    public partial class Chapter : ProtoObject, IConfig
    {
        [ProtoMember(1)]
        public int Id { get; set; }
        [ProtoMember(2)]
        public string Command { get; set; }
        [ProtoMember(3)]
        public string Arg1 { get; set; }
        [ProtoMember(4)]
        public string Arg2 { get; set; }
        [ProtoMember(5)]
        public string Arg3 { get; set; }
        [ProtoMember(6)]
        public string Arg4 { get; set; }
        [ProtoMember(7)]
        public string Arg5 { get; set; }
        [ProtoMember(8)]
        public string Arg6 { get; set; }
        [ProtoMember(9)]
        public string WaitType { get; set; }
        [ProtoMember(10)]
        public string PageCtrl { get; set; }
        [ProtoMember(11)]
        public string Voice { get; set; }
        [ProtoMember(12)]
        public string WindowType { get; set; }
        [ProtoMember(13)]
        public string Chinese { get; set; }
        [ProtoMember(14)]
        public string English { get; set; }
        
    }
}
