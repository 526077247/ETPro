using System.Collections.Generic;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
	public partial class AIConfigCategory
	{
		[ProtoIgnore]
		[BsonIgnore]
		public Dictionary<int, SortedDictionary<int, AIConfig>> AIConfigs = new Dictionary<int, SortedDictionary<int, AIConfig>>();

		public SortedDictionary<int, AIConfig> GetAI(int aiConfigId)
		{
			return this.AIConfigs[aiConfigId];
		}
		
		public override void AfterEndInit()
		{
			for (int i = 0; i < this.GetAllList().Count; i++)
			{
				var kv = this.GetAllList()[i];
				SortedDictionary<int, AIConfig> aiNodeConfig;
				if (!this.AIConfigs.TryGetValue(kv.AIConfigId, out aiNodeConfig))
				{
					aiNodeConfig = new SortedDictionary<int, AIConfig>();
					this.AIConfigs.Add(kv.AIConfigId, aiNodeConfig);
				}
				
				aiNodeConfig.Add(kv.Id, kv);
			}
		}
	}
}
