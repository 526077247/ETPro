using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class GuidanceComponent:Entity,IAwake
    {
        public static GuidanceComponent Instance;

        public int Group;
        [BsonIgnore]
        public GuidanceGroupConfig Config => GuidanceConfigCategory.Instance.GetGroup(Group);
        
        public int CurIndex;
        
        [BsonIgnore]
        public GuidanceConfig StepConfig => Config?.Steps[this.CurIndex];

        [BsonIgnore]
        public Dictionary<string, int> CacheValues;
    }
}