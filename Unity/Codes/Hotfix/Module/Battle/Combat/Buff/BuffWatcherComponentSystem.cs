using System;
using System.Collections.Generic;

namespace ET
{
    [FriendClass(typeof(BuffWatcherComponent))]
    public static class BuffWatcherComponentSystem
    {
        [ObjectSystem]
        public class BuffWatcherComponentAwakeSystem : AwakeSystem<BuffWatcherComponent>
        {
            public override void Awake(BuffWatcherComponent self)
            {
                BuffWatcherComponent.Instance = self;
                self.Init();
            }
        }

	
        public class BuffWatcherComponentLoadSystem : LoadSystem<BuffWatcherComponent>
        {
            public override void Load(BuffWatcherComponent self)
            {
                self.Init();
            }
        }

        private static void Init(this BuffWatcherComponent self)
        {
            self.allActiveWatchers = new Dictionary<int, List<IActionControlActiveWatcher>>();
            self.allDamageWatchers = new Dictionary<int, List<IDamageBuffWatcher>>();
            self.allAddBuffWatchers = new Dictionary<int, List<IAddBuffWatcher>>();
            
            List<Type> types = Game.EventSystem.GetTypes(typeof(ActionControlActiveWatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ActionControlActiveWatcherAttribute), false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    ActionControlActiveWatcherAttribute item = (ActionControlActiveWatcherAttribute)attrs[i];
                    IActionControlActiveWatcher obj = (IActionControlActiveWatcher)Activator.CreateInstance(type);
                    var key = item.IsAdd ? item.ActionControlType : -item.ActionControlType;
                    if (!self.allActiveWatchers.ContainsKey(key))
                    {
                        self.allActiveWatchers.Add(key, new List<IActionControlActiveWatcher>());
                    }
                    self.allActiveWatchers[key].Add(obj);
                }
            }
            
            types = Game.EventSystem.GetTypes(typeof(BuffDamageWatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(BuffDamageWatcherAttribute), false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    BuffDamageWatcherAttribute item = (BuffDamageWatcherAttribute)attrs[i];
                    IDamageBuffWatcher obj = (IDamageBuffWatcher)Activator.CreateInstance(type);
                    var key = item.BuffSubType;
                    if (!self.allActiveWatchers.ContainsKey(key))
                    {
                        self.allDamageWatchers.Add(key, new List<IDamageBuffWatcher>());
                    }
                    self.allDamageWatchers[key].Add(obj);
                }
            }
            
            types = Game.EventSystem.GetTypes(typeof(AddBuffWatcherAttribute));
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(AddBuffWatcherAttribute), false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    AddBuffWatcherAttribute item = (AddBuffWatcherAttribute)attrs[i];
                    IAddBuffWatcher obj = (IAddBuffWatcher)Activator.CreateInstance(type);
                    var key = item.BuffSubType;
                    if (!self.allActiveWatchers.ContainsKey(key))
                    {
                        self.allAddBuffWatchers.Add(key, new List<IAddBuffWatcher>());
                    }
                    self.allAddBuffWatchers[key].Add(obj);
                }
            }
        }

        public static void SetActionControlActive(this BuffWatcherComponent self, int type,bool isAdd,Unit unit)
        {
            var key = isAdd ? type : -type;
            List<IActionControlActiveWatcher> list;
            if (!self.allActiveWatchers.TryGetValue(key, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IActionControlActiveWatcher numericWatcher = list[i];
                numericWatcher.SetActionControlActive(unit);
            }
        }
        
        public static void BeforeDamage(this BuffWatcherComponent self, int type,Unit attacker,Unit target,Buff buff,DamageInfo info)
        {
            List<IDamageBuffWatcher> list;
            if (!self.allDamageWatchers.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IDamageBuffWatcher numericWatcher = list[i];
                numericWatcher.BeforeDamage(attacker,target,buff,info);
            }
        }
        
        public static void AfterDamage(this BuffWatcherComponent self, int type,Unit attacker,Unit target,Buff buff,DamageInfo info)
        {
            List<IDamageBuffWatcher> list;
            if (!self.allDamageWatchers.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IDamageBuffWatcher numericWatcher = list[i];
                numericWatcher.AfterDamage(attacker,target,buff,info);
            }
        }
        
        public static bool BeforeAddBuff(this BuffWatcherComponent self, int type,Unit attacker,Unit target,int buffId)
        {
            List<IAddBuffWatcher> list;
            if (!self.allAddBuffWatchers.TryGetValue(type, out list))
            {
                return true;
            }

            bool res = true;
            for (int i = 0; i < list.Count; i++)
            {
                IAddBuffWatcher numericWatcher = list[i];
                numericWatcher.BeforeAdd(attacker,target,buffId,ref res);
                if (!res)
                {
                    return false;
                }
            }
            return true;
        }
        
        public static void AfterAddBuff(this BuffWatcherComponent self, int type,Unit attacker,Unit target,Buff buff)
        {
            List<IAddBuffWatcher> list;
            if (!self.allAddBuffWatchers.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IAddBuffWatcher numericWatcher = list[i];
                numericWatcher.AfterAdd(attacker,target,buff);
            }
        }
    }
}