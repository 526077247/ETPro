using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

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

                if (type.IsSubclassOf(TypeInfo<Entity>.Type))
                {
                    EntityView item = new EntityView();
                    item.Name = kv.Key;
                    object[] objects = type.GetCustomAttributes(TypeInfo<ChildOfAttribute>.Type, true);
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
                    objects = type.GetCustomAttributes(TypeInfo<ComponentOfAttribute>.Type, true);
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
            GetProperty(entity, data.DataProperties,0,new HashSet<object>(){entity});

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

        private static void GetProperty(object obj,List<EntityDataProperty> dataProperty,int intent,HashSet<object> temp)
        {
            var filds = obj.GetType().GetFields();
            for (int i = 0; i < filds.Length; i++)
            {
                bool needAdd = true;
                var attr = filds[i].GetCustomAttributes(true);
                for (int j = 0; j < attr.Length; j++)
                {
                    if (attr[j] is IgnoreDataMemberAttribute || attr[j] is BsonIgnoreAttribute)
                    {
                        needAdd = false;
                        break;
                    }
                }

                if (!needAdd) continue;
                var value = filds[i].GetValue(obj);
                if (value==null||filds[i].IsStatic||value is string||!filds[i].FieldType.IsClass||filds[i].FieldType.IsArray||
                    string.IsNullOrEmpty(filds[i].FieldType.Namespace)||filds[i].FieldType.Namespace.Contains("System.Collections")
                    ||value is Component||temp.Contains(value)||intent>5)
                {
                    if (intent > 5)
                    {
                        Log.Error($"深度>5  type={obj.GetType().Name}");
                    }
                    dataProperty.Add(new EntityDataProperty() { type = filds[i].FieldType, value = value, name = filds[i].Name ,indent = intent});
                }
                else
                {
                    var list = new List<EntityDataProperty>();
                    temp.Add(value);
                    GetProperty(value, list,intent+1,temp);
                    temp.Remove(value);
                    dataProperty.Add(new EntityDataProperty() { type = filds[i].FieldType, value = list, name = filds[i].Name ,indent = intent});
                }
            }
        }
    }
}