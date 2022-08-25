using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ChildOf(typeof(BuffComponent))]
    public class Buff:Entity,IAwake<int,long,long>,IAwake<int,long,bool,long>,IDestroy,ITransfer
    {
        public int ConfigId;
        [BsonIgnore]
        public BuffConfig Config
        {
            get => BuffConfigCategory.Instance.Get(ConfigId);
        }

        [BsonIgnore]
        public BuffAttrConfig AttrConfig
        {
            get => BuffAttrConfigCategory.Instance.Get(ConfigId);
        }
        [BsonIgnore]
        public BuffActionControlConfig ActionControlConfig
        {
            get => BuffActionControlConfigCategory.Instance.Get(ConfigId);
        }
        
        [BsonIgnore]
        public BuffBleedConfig BleedConfig
        {
            get => BuffBleedConfigCategory.Instance.Get(ConfigId);
        }
        /// <summary>
        /// 持续到什么时间
        /// </summary>
        public long Timestamp;
        
        [BsonIgnore]
        public long TimerId;

        /// <summary>
        /// 来源
        /// </summary>
        public long FromUnitId;
    }
}