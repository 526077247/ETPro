using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using ProtoBuf;

namespace ET
{
    public class GuidanceGroupConfig
    {
        public List<GuidanceConfig> Steps;
        /// <summary>引导组</summary>
        public int Group { get; set; }
        /// <summary>开启条件（关键步骤配此字段生效）</summary>
        public string Condition { get; set; }
        /// <summary>引导组优先级（关键步骤配此字段生效）</summary>
        public int Grouporder { get; set; }
        /// <summary>服务端需下发道具（关键步骤配此字段生效）</summary>
        public int[] PreAward { get; set; }
        /// <summary>是否开启（关键步骤配此字段生效）</summary>
        public int Open { get; set; }
        /// <summary>是否所有玩家共享（关键步骤配此字段生效）</summary>
        public int Share { get; set; }
        
    }
    public partial class GuidanceConfigCategory
    {
        [ProtoIgnore]
        [BsonIgnore]
        public Dictionary<int, GuidanceGroupConfig> GroupConfigs = new Dictionary<int, GuidanceGroupConfig>();
        [ProtoIgnore]
        [BsonIgnore]
        public List<GuidanceGroupConfig> GroupConfigList = new List<GuidanceGroupConfig>();
        public override void AfterEndInit()
        {
            for (int i = 0; i < this.GetAllList().Count; i++)
            {
                var item = this.GetAllList()[i];
                GuidanceGroupConfig groupConfig;
                if (!this.GroupConfigs.TryGetValue(item.Group, out groupConfig))
                {
                    groupConfig = new GuidanceGroupConfig();
                    groupConfig.Steps = new List<GuidanceConfig>();
                    this.GroupConfigs.Add(item.Group, groupConfig);
                    GroupConfigList.Add(groupConfig);
                }
				
                groupConfig.Steps.Add(item);
                if (item.KeyStep == 1)
                {
                    groupConfig.Group = item.Group;
                    groupConfig.Condition = item.Condition;
                    groupConfig.Grouporder = item.Grouporder;
                    groupConfig.PreAward = item.PreAward;
                    groupConfig.Open = item.Open;
                    groupConfig.Share = item.Share;
                }
            }
            //优先级排序
            GroupConfigList.Sort((a, b) => { return a.Grouporder - b.Grouporder;});
            for (int i = GroupConfigList.Count-1; i >=0 ; i--)
            {
                if (GroupConfigList[i].Open == 0)
                {
                    GroupConfigList.RemoveAt(i);
                    GroupConfigs.Remove(GroupConfigList[i].Group);
                    continue;
                }
                GroupConfigList[i].Steps.Sort((a, b) => { return a.Steporder - b.Steporder;});
            }
        }


        public GuidanceGroupConfig GetGroup(int id)
        {
            GroupConfigs.TryGetValue(id, out var res);
            return res;
        }
        public List<GuidanceGroupConfig> GetAllGroupList()
        {
            return GroupConfigList;
        }
    }
}