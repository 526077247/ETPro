using System;
using UnityEngine;

namespace ET
{
    
    [Timer(TimerType.DestroyGameObject)]
    public class DestroyGameObject: ATimer<GameObjectComponent>
    {
        public override void Run(GameObjectComponent self)
        {
            try
            {
                self.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    [ObjectSystem]
    public class GameObjectComponentAwakeSystem: AwakeSystem<GameObjectComponent,GameObject>
    {
        public override void Awake(GameObjectComponent self,GameObject a)
        {
            self.GameObject = a;
            self.IsDebug = false;
        }
    }
    [ObjectSystem]
    public class GameObjectComponentAwakeSystem1: AwakeSystem<GameObjectComponent,GameObject,Action>
    {
        public override void Awake(GameObjectComponent self,GameObject a,Action b)
        {
            self.OnDestroyAction = b;
            self.GameObject = a;
            self.IsDebug = false;
        }
    }
    [ObjectSystem]
    public class GameObjectComponentDestroySystem: DestroySystem<GameObjectComponent>
    {
        public override void Destroy(GameObjectComponent self)
        {
            if (self.OnDestroyAction != null)
            {
                self.OnDestroyAction();
            }
            else
            {
                UnityEngine.Object.Destroy(self.GameObject);
            }
            self.GameObject = null;
        }
    }
    [FriendClass(typeof(GameObjectComponent))]
    public static class GameObjectComponentSystem
    {
        /// <summary>
        /// 获取ReferenceCollector里的物体
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetCollectorObj<T>(this GameObjectComponent self,string name)where T:UnityEngine.Object
        {
            if (self.Collector == null)
            {
                self.Collector = self.GameObject.GetComponent<ReferenceCollector>();
            }

            return self.Collector?.Get<T>(name);
        }
    }
}