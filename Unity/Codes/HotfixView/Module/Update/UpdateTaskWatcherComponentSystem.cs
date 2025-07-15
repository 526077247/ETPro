using System;
using System.Collections.Generic;

namespace ET
{
    [FriendClass(typeof(UpdateProcessAttribute))]
    [FriendClass(typeof(UpdateTaskWatcherComponent))]
    public static class UpdateTaskWatcherComponentSystem
    {
        public class UpdateTaskWatcherComponentAwakeSystem:AwakeSystem<UpdateTaskWatcherComponent>
        {
            public override void Awake(UpdateTaskWatcherComponent self)
            {
                UpdateTaskWatcherComponent.Instance = self;
                self.Init();
            }
        }
        
        public class UpdateTaskWatcherComponentLoadSystem:LoadSystem<UpdateTaskWatcherComponent>
        {
            public override void Load(UpdateTaskWatcherComponent self)
            {
                self.Init();
            }
        }
        
        private static void Init(this UpdateTaskWatcherComponent self)
        {
            self.allWatchers = new Dictionary<int, IUpdateProcess>();

            var types = Game.EventSystem.GetTypes(TypeInfo<UpdateProcessAttribute>.Type);
            for (int j = 0; j < types.Count; j++)
            {
                Type type = types[j];
                object[] attrs = type.GetCustomAttributes(TypeInfo<UpdateProcessAttribute>.Type, false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    UpdateProcessAttribute numericWatcherAttribute = (UpdateProcessAttribute)attrs[i];
                    IUpdateProcess obj = (IUpdateProcess)Activator.CreateInstance(type);
                    if (!self.allWatchers.ContainsKey(numericWatcherAttribute.UpdateStep))
                    {
                        self.allWatchers[numericWatcherAttribute.UpdateStep] = obj;
                    }
                    else
                    {
                        Log.Error(numericWatcherAttribute.UpdateStep+"重复定义");
                    }
                }
            }
        }

        public static async ETTask<UpdateRes> Process(this UpdateTaskWatcherComponent self, UpdateTaskStep type, UpdateTask para)
        {
            IUpdateProcess process;
            if (!self.allWatchers.TryGetValue((int)type, out process))
            {
                return UpdateRes.Over;
            }
            return await process.Process(para);
        }
    }
}