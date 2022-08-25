using System;
using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class AreaConfigComponent:Entity,IAwake
    {
        public static AreaConfigComponent Instance;
        
        public IConfigLoader Loader => ConfigComponent.Instance.ConfigLoader;
        
        public Dictionary<string, AreaConfigCategory> AreaConfigCategorys = new Dictionary<string, AreaConfigCategory>();
    }
}