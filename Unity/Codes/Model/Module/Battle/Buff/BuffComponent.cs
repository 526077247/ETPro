using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace ET
{
    [ComponentOf(typeof(CombatUnitComponent))]
    public class BuffComponent:Entity, IAwake, IDestroy,ITransfer
    {
        [BsonIgnore]
        public Unit unit => this.GetParent<CombatUnitComponent>().unit;

        public List<long> AllBuff;
        public Dictionary<int, long> Groups;
        public Dictionary<int, int> ActionControls;
    }
}