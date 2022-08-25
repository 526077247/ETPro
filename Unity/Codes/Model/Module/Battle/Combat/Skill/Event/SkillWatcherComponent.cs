using System;
using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 监视技能变化组件,分发监听
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class SkillWatcherComponent : Entity, IAwake, ILoad
    {
        public static SkillWatcherComponent Instance { get; set; }
		
        public Dictionary<int, List<ISkillWatcher>> allWatchers;
        
    }
}