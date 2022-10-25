using System;
using System.Collections.Generic;

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
    }
}