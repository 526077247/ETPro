using System;
using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 监视数值变化组件,分发监听
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class CommandWatcherComponent : Entity,IAwake,ILoad
    {
        public static CommandWatcherComponent Instance { get; set; }
		
        public Dictionary<string, List<ICommandWatcher>> allWatchers;
        
    }
}
