using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class RedDotComponent: Entity,IAwake,IDestroy
    {
        public static RedDotComponent Instance;
        
        public Dictionary<string, ListComponent<string>> RedDotNodeParentsDict = new Dictionary<string, ListComponent<string>>();

        public Dictionary<string, int> RetainViewCount = new Dictionary<string, int>();
        
        public Dictionary<string, string> ToParentDict = new Dictionary<string, string>();

        public Dictionary<string, Entity> RedDotMonoViewDict = new Dictionary<string, Entity>();
    }
}