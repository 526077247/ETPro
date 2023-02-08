using System.Collections.Generic;

namespace ET
{
    public partial class SceneConfigCategory
    {
        private Dictionary<string, SceneConfig> _map;

        public override void AfterEndInit()
        {
            base.AfterEndInit();
            _map =new Dictionary<string, SceneConfig>();
            for (int i = 0; i < list.Count; i++)
            {
                _map.Add(list[i].Name,list[i]);
            }
        }

        public Dictionary<string, SceneConfig> GetSceneConfig()
        {
            return _map;
        }
    }
}