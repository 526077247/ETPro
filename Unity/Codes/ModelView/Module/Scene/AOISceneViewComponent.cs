using System.Collections.Generic;
using UnityEngine;
namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class AOISceneViewComponent:Entity,IAwake
    {
        public static AOISceneViewComponent Instance;
        public AssetsScene CurMap;
        public Dictionary<string, AssetsScene> NameMapScene;
        public int CellLen
        {
            get
            {
                if (this.CurMap == null) return 1;
                return this.CurMap.CellLen;
            }
        }

        public class DynamicSceneViewObj
        {
            public GameObject Obj;
            public bool IsLoading;
            public string Type;
        }

        /// <summary>
        /// 当前需加载的场景物体
        /// </summary>
        public Dictionary<int, int> DynamicSceneObjectMapCount;
        
        /// <summary>
        /// 当前已加载的场景物体
        /// </summary>
        public Dictionary<int, DynamicSceneViewObj> DynamicSceneObjectMapObj;
        public int? LastGridX = null;
        public int? LastGridY = null;
        
        //是否忙
        public bool Busing { get; set; }= false;
    }
}