using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ComponentOf(typeof(Buff))]
    public class BuffBleedComponent:Entity,IAwake<int>,IDestroy,ITransfer,ISerializeToEntity,IUpdate
    {
        public int ConfigId;

        [BsonIgnore]
        public BuffBleedConfig Config => BuffBleedConfigCategory.Instance.Get(this.ConfigId);

        /// <summary>
        /// 上次掉血时间
        /// </summary>
        public long LastBleedTime;
        
    }
}