using System;
using System.Collections.Generic;

namespace ET
{
    [FriendClass(typeof(CommandWatcherComponent))]
    public static class CommandWatcherComponentSystem
    {
        [ObjectSystem]
        public class CommandWatcherComponentAwakeSystem : AwakeSystem<CommandWatcherComponent>
        {
            public override void Awake(CommandWatcherComponent self)
            {
                CommandWatcherComponent.Instance = self;
                self.Init();
            }
        }

	
        public class CommandWatcherComponentLoadSystem : LoadSystem<CommandWatcherComponent>
        {
            public override void Load(CommandWatcherComponent self)
            {
                self.Init();
            }
        }

        private static void Init(this CommandWatcherComponent self)
        {
            self.allWatchers = new Dictionary<string, List<ICommandWatcher>>();

            List<Type> types = Game.EventSystem.GetTypes(typeof(CommandWatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(CommandWatcherAttribute), false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    CommandWatcherAttribute numericWatcherAttribute = (CommandWatcherAttribute)attrs[i];
                    ICommandWatcher obj = (ICommandWatcher)Activator.CreateInstance(type);
                    if (!self.allWatchers.ContainsKey(numericWatcherAttribute.Command))
                    {
                        self.allWatchers.Add(numericWatcherAttribute.Command, new List<ICommandWatcher>());
                    }
                    self.allWatchers[numericWatcherAttribute.Command].Add(obj);
                }
            }
        }

        public static async ETTask Run(this CommandWatcherComponent self,string command, GalGameEngineComponent engine, GalGameEnginePara para)
        {
            List<ICommandWatcher> list;
            if (!self.allWatchers.TryGetValue(command, out list))
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                ICommandWatcher watcher = list[i];
                await watcher.Run(engine, para);
            }
        }
    }
}