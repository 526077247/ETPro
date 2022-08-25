using System;
using System.Collections.Generic;

namespace ET
{

    /// <summary>
    /// 监视输入组件,分发监听
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class InputWatcherComponent : Entity, IAwake, ILoad
    {
        public static InputWatcherComponent Instance { get; set; }
        public TypeSystems typeSystems;
        public UnOrderMultiMap<object, InputSystemAttribute> typeMapAttr;

        public List<Entity> InputEntitys;
        
        public MultiDictionary<int,int, List<object>> allSystem;

        public LinkedList<Tuple<object,Entity,int,int[],int[]>> sortList;
    }
}