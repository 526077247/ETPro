using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 监视条件判断组件,分发监听
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ConditionWatcherComponent : Entity, IAwake, ILoad
    {
        public static ConditionWatcherComponent Instance { get; set; }
		
        public Dictionary<string, List<IConditionWatcher>> allWatchers;
        
    }
}