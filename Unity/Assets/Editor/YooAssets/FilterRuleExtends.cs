using System.IO;
using YooAsset.Editor;

namespace ET
{
	
    [DisplayName("收集Unit")]
    public class CollectUnit : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("/Edit/")) return false;
            var ext = Path.GetExtension(data.AssetPath);
            return ext == ".prefab" || ext == ".bytes"|| ext == ".json" || ext == ".controller" || 
                   data.AssetPath.Contains("Common");
        }
    }
	
    [DisplayName("收集UI")]
    public class CollectUI : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            if (data.AssetPath.Contains("/Atlas/")) return false;
            return true;
        }
    }
    
    [DisplayName("收集AOT")]
    public class CollectAOT : IFilterRule
    {
        public bool IsCollectAsset(FilterRuleData data)
        {
            return data.AssetPath.Contains(Define.AOTDir);
        }
    }
}