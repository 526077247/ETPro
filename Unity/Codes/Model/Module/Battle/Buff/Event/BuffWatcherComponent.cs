using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 监视Buff组件,分发监听
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class BuffWatcherComponent : Entity, IAwake, ILoad
    {
        public static BuffWatcherComponent Instance { get; set; }
		
        public Dictionary<int, List<IActionControlActiveWatcher>> allActiveWatchers;
        
        public Dictionary<int, List<IDamageBuffWatcher>> allDamageWatchers;
    }
}