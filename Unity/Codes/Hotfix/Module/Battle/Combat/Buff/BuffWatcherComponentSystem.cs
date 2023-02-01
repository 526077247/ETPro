using System;
using System.Collections.Generic;
using UnityEngine;

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
            self.allRemoveBuffWatchers = new Dictionary<int, List<IRemoveBuffWatcher>>();
            self.allMoveBuffWatchers = new Dictionary<int, List<IMoveBuffWatcher>>();
            List<Type> types = Game.EventSystem.GetTypes(TypeInfo<ActionControlActiveWatcherAttribute>.Type);
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(TypeInfo<ActionControlActiveWatcherAttribute>.Type, false);

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
            
            types = Game.EventSystem.GetTypes(TypeInfo<BuffDamageWatcherAttribute>.Type);
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(TypeInfo<BuffDamageWatcherAttribute>.Type, false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    BuffDamageWatcherAttribute item = (BuffDamageWatcherAttribute)attrs[i];
                    IDamageBuffWatcher obj = (IDamageBuffWatcher)Activator.CreateInstance(type);
                    var key = item.BuffSubType;
                    if (!self.allDamageWatchers.ContainsKey(key))
                    {
                        self.allDamageWatchers.Add(key, new List<IDamageBuffWatcher>());
                    }
                    self.allDamageWatchers[key].Add(obj);
                }
            }
            
            types = Game.EventSystem.GetTypes(TypeInfo<AddBuffWatcherAttribute>.Type);
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(TypeInfo<AddBuffWatcherAttribute>.Type, false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    AddBuffWatcherAttribute item = (AddBuffWatcherAttribute)attrs[i];
                    IAddBuffWatcher obj = (IAddBuffWatcher)Activator.CreateInstance(type);
                    var key = item.BuffSubType;
                    if (!self.allAddBuffWatchers.ContainsKey(key))
                    {
                        self.allAddBuffWatchers.Add(key, new List<IAddBuffWatcher>());
                    }
                    self.allAddBuffWatchers[key].Add(obj);
                }
            }
            
            types = Game.EventSystem.GetTypes(TypeInfo<RemoveBuffWatcherAttribute>.Type);
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(TypeInfo<RemoveBuffWatcherAttribute>.Type, false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    RemoveBuffWatcherAttribute item = (RemoveBuffWatcherAttribute)attrs[i];
                    IRemoveBuffWatcher obj = (IRemoveBuffWatcher)Activator.CreateInstance(type);
                    var key = item.BuffSubType;
                    if (!self.allRemoveBuffWatchers.ContainsKey(key))
                    {
                        self.allRemoveBuffWatchers.Add(key, new List<IRemoveBuffWatcher>());
                    }
                    self.allRemoveBuffWatchers[key].Add(obj);
                }
            }
            
            types = Game.EventSystem.GetTypes(TypeInfo<MoveBuffWatcherAttribute>.Type);
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(TypeInfo<MoveBuffWatcherAttribute>.Type, false);

                for (int i = 0; i < attrs.Length; i++)
                {
                    MoveBuffWatcherAttribute item = (MoveBuffWatcherAttribute)attrs[i];
                    IMoveBuffWatcher obj = (IMoveBuffWatcher)Activator.CreateInstance(type);
                    var key = item.BuffSubType;
                    if (!self.allMoveBuffWatchers.ContainsKey(key))
                    {
                        self.allMoveBuffWatchers.Add(key, new List<IMoveBuffWatcher>());
                    }
                    self.allMoveBuffWatchers[key].Add(obj);
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
        
        public static bool BeforeAddBuff(this BuffWatcherComponent self,Buff baseBuff, int type,Unit attacker,Unit target,int buffId)
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
                numericWatcher.BeforeAdd(attacker,target,baseBuff,buffId,ref res);
                if (!res)
                {
                    return false;
                }
            }
            return true;
        }
        
        public static void AfterAddBuff(this BuffWatcherComponent self,Buff baseBuff, int type,Unit attacker,Unit target,Buff buff)
        {
            List<IAddBuffWatcher> list;
            if (!self.allAddBuffWatchers.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IAddBuffWatcher numericWatcher = list[i];
                numericWatcher.AfterAdd(attacker,target,baseBuff,buff);
            }
        }
        
        public static bool BeforeRemoveBuff(this BuffWatcherComponent self,Buff baseBuff, int type,Unit target,Buff buff)
        {
            List<IRemoveBuffWatcher> list;
            if (!self.allRemoveBuffWatchers.TryGetValue(type, out list))
            {
                return true;
            }

            bool res = true;
            for (int i = 0; i < list.Count; i++)
            {
                IRemoveBuffWatcher numericWatcher = list[i];
                numericWatcher.BeforeRemove(target,baseBuff,buff,ref res);
                if (!res)
                {
                    return false;
                }
            }
            return true;
        }
        
        public static void AfterRemoveBuff(this BuffWatcherComponent self,Buff baseBuff, int type,Unit target,Buff buff)
        {
            List<IRemoveBuffWatcher> list;
            if (!self.allRemoveBuffWatchers.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IRemoveBuffWatcher numericWatcher = list[i];
                numericWatcher.AfterRemove(target,baseBuff,buff);
            }
        }
        
        public static void AfterMove(this BuffWatcherComponent self, int type,Unit target,Buff buff,Vector3 before)
        {
            List<IMoveBuffWatcher> list;
            if (!self.allMoveBuffWatchers.TryGetValue(type, out list))
            {
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                IMoveBuffWatcher numericWatcher = list[i];
                numericWatcher.AfterMove(target,buff,before);
            }
        }
    }
}