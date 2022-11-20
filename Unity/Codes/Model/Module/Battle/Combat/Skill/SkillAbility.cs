using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace ET
{
    [ChildOf(typeof(CombatUnitComponent))]
    public class SkillAbility:Entity,IAwake<int>,IDestroy,ITransfer
    {
        public int ConfigId;
        [BsonIgnore]
        public SkillConfig SkillConfig => SkillConfigCategory.Instance.Get(ConfigId);

        public long LastSpellTime;//上次施法时间
        public long LastSpellOverTime;//上次施法完成时间


        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, long> Groups { get; set; }
    }
}