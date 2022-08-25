using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SceneManagerComponent:Entity,IAwake,IDestroy
    {
        public List<string> ScenesChangeIgnoreClean { get; set; }
        public List<string> DestroyWindowExceptNames{ get; set; }
        public static SceneManagerComponent Instance;

        public Dictionary<string, SceneConfig> SceneConfigs;
        //当前场景
        public string CurrentScene;
        //是否忙
        public bool Busing = false;

    }
}
