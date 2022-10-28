using System;
using System.Collections.Generic;

namespace ET
{
    public class EntityData
    {
        public long Id;
        public List<EntityData> Childs = new List<EntityData>();
        public List<EntityData> Components = new List<EntityData>();

        public List<EntityDataProperty> DataProperties = new List<EntityDataProperty>();
        public Type Type;

        public bool IsSelected = false;
        
    }

    public class EntityDataProperty
    {
        public string name;
        public Type type;
        public object value;
        public int indent;
    }
}