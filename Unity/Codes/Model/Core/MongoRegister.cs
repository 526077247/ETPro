using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using UnityEngine;

namespace ET
{
    public static class MongoRegister
    {
        public static void Init()
        {
        }
        
        static MongoRegister()
        {
            // 自动注册IgnoreExtraElements

            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };

            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
            try
            {
                BsonSerializer.RegisterSerializer(TypeInfo<Vector3>.Type, new StructBsonSerialize<Vector3>());
                BsonSerializer.RegisterSerializer(TypeInfo<Vector4>.Type, new StructBsonSerialize<Vector4>());
                BsonSerializer.RegisterSerializer(TypeInfo<Quaternion>.Type, new StructBsonSerialize<Quaternion>());
            }catch(Exception ex){Log.Info(ex);}

            Dictionary<string, Type> types = EventSystem.Instance.GetTypes();
            foreach (Type type in types.Values)
            {
                if (!type.IsSubclassOf(TypeInfo<Object>.Type))
                {
                    continue;
                }

                if (type.IsGenericType)
                {
                    continue;
                }

                BsonClassMap.LookupClassMap(type);
            }
        }
    }
}