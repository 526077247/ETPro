using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{

    [ComponentOf(typeof(Scene))]
    public sealed class UpdateTaskWatcherComponent:Entity,IAwake,ILoad
    {
        public static UpdateTaskWatcherComponent Instance;

        public Dictionary<int, IUpdateProcess> allWatchers;
		
    }
}
