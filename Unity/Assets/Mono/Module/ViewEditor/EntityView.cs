using System.Collections.Generic;

namespace ET
{
    public class EntityView
    {
        public List<string> ComponentOf = new List<string>();
        public List<string> ChildOf = new List<string>();
        public bool ChildOfAll;
        public bool ComponentOfAll;
        public string Name;
        public List<EntityView> Components = new List<EntityView>();
        public List<EntityView> Childs = new List<EntityView>();

    }
}