using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ET
{
    public static class ViewEditorHelper
    {
        public static Dictionary<string, EntityView> GetAllEntitys()
        {
            Dictionary<string, EntityView> res = new Dictionary<string, EntityView>();
            foreach (var kv in CodeLoader.Instance.GetHotfixTypes())
            {
                Type type = kv.Value;
                if (type.IsAbstract)
                {
                    continue;
                }

                if (type.IsSubclassOf(typeof (Entity)))
                {
                    EntityView item = new EntityView();
                    item.Name = kv.Key;
                    object[] objects = type.GetCustomAttributes(typeof(ChildOfAttribute), true);
                    if (objects.Length != 0)
                    {
                        foreach (ChildOfAttribute childof in objects)
                        {
                            if (childof.type == null)
                            {
                                item.ChildOfAll = true;
                            }
                            else
                            {
                                item.ChildOf.Add(childof.type.FullName);
                            }
                           
                        }
                    }
                    objects = type.GetCustomAttributes(typeof(ComponentOfAttribute), true);
                    if (objects.Length != 0)
                    {
                        foreach (ComponentOfAttribute childof in objects)
                        {
                            if (childof.type == null)
                            {
                                item.ComponentOfAll = true;
                            }
                            else
                            {
                                item.ComponentOf.Add(childof.type.FullName);
                            }
                            
                        }
                    }
                    res.Add(item.Name,item);
                }
                
            }

            return res;
        }

        public static EntityData GetEntityData()
        {
            EntityData root = new EntityData();
            GetEntityData(Game.Scene, root);
            return root;
        }

        private static void GetEntityData(Entity entity,EntityData data)
        {
            data.Id = entity.Id;
            data.Type = entity.GetType();
            var filds = data.Type.GetFields();
            for (int i = 0; i < filds.Length; i++)
            {
                bool needAdd = true;
                var attr = filds[i].GetCustomAttributes(true);
                for (int j = 0; j < attr.Length; j++)
                {
                    if (attr[j] is IgnoreDataMemberAttribute)
                    {
                        needAdd = false;
                        break;
                    }
                }
                if(!needAdd) continue;
                data.DataProperties.Add(new EntityDataProperty()
                {
                    type = filds[i].FieldType,
                    value = filds[i].GetValue(entity),
                    name = filds[i].Name
                });
            }

            foreach (var item in entity.Children)
            {
                var child = new EntityData();
                GetEntityData(item.Value, child);
                data.Childs.Add(child);
            }
            
            foreach (var item in entity.Components)
            {
                var comp = new EntityData();
                GetEntityData(item.Value, comp);
                data.Components.Add(comp);
            }
        }
    }
}