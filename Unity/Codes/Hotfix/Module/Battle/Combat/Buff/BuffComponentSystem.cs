using System.Collections.Generic;

namespace ET
{

    [ObjectSystem]
    [FriendClass(typeof(CombatUnitComponent))]
    public class BuffComponentAwakeSystem : AwakeSystem<BuffComponent>
    {
        public override void Awake(BuffComponent self)
        {
            self.AllBuff = new List<long>();
            self.Groups =new Dictionary<int, long>();
            self.ActionControls =new Dictionary<int, int>();
        }
    }
    
    [ObjectSystem]
    public class BuffComponentDestroySystem : DestroySystem<BuffComponent>
    {
        public override void Destroy(BuffComponent self)
        {
            self.AllBuff.Clear();
            self.Groups.Clear();
            self.ActionControls.Clear();
        }
    }
	[FriendClass(typeof(BuffComponent))]
    [FriendClass(typeof(Buff))]
    [FriendClass(typeof(CombatUnitComponent))]
    public static class BuffComponentSystem
    {
        /// <summary>
        /// 初始化(第一次创建Unit走这里，因为服务端穿的属性是加了BUFF后的属性，所以这里创建BUFF时不叠加属性)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="buffIds"></param>
        /// <param name="buffTimestamps"></param>
        /// <param name="sourceIds"></param>
        public static void Init(this BuffComponent self,List<int> buffIds,List<long> buffTimestamps,List<long> sourceIds)
        {
            self.AllBuff.Clear();
            self.Groups.Clear();
            self.ActionControls.Clear();
            for (int i = 0; i < buffIds.Count; i++)
            {
                var id = buffIds[i];
                var timestamp = buffTimestamps[i];
                var sourceId = sourceIds[i];
                BuffConfig conf = BuffConfigCategory.Instance.Get(id);
                if (self.Groups.ContainsKey(conf.Group))
                {
                    var oldId = self.Groups[conf.Group];
                    var old = self.GetChild<Buff>(oldId);
                    if (old.Config.Priority > conf.Priority) {
                        Log.Info("添加BUFF失败，优先级"+old.Config.Id+" > "+conf.Id);
                        continue; //优先级低
                    }
                    Log.Info("优先级高或相同，替换旧的");
                    self.Remove(self.Groups[conf.Group]);
                }
            
                Buff buff = self.AddChild<Buff,int,long,bool,long>(id,timestamp,true,sourceId);//走这里不叠加属性
                self.Groups[conf.Group] = buff.Id;
                self.AllBuff.Add(buff.Id);
                EventSystem.Instance.Publish(new EventType.AfterAddBuff(){Buff = buff});
            }
        }
        
        /// <summary>
        /// 添加BUFF
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <param name="timestamp"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public static Buff AddBuff(this BuffComponent self, int id,long timestamp,long sourceId)
        {
            BuffConfig conf = BuffConfigCategory.Instance.Get(id);
            if (self.Groups.ContainsKey(conf.Group))
            {
                var oldId = self.Groups[conf.Group];
                var old = self.GetChild<Buff>(oldId);
                if (old.Config.Priority > conf.Priority) {
                    Log.Info("添加BUFF失败，优先级"+old.Config.Id+" > "+conf.Id);
                    return null; //优先级低
                }
                Log.Info("优先级高或相同，替换旧的");
                self.Remove(self.Groups[conf.Group]);
            }
            
            Buff buff = self.AddChild<Buff,int,long,long>(id,timestamp,sourceId,true);
            self.Groups[conf.Group] = buff.Id;
            self.AllBuff.Add(buff.Id);
            EventSystem.Instance.Publish(new EventType.AfterAddBuff(){Buff = buff});
            return buff;
        }
        /// <summary>
        /// 通过Buff的唯一Id取
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Buff Get(this BuffComponent self, long id)
        {
            Buff buff = self.GetChild<Buff>(id);
            return buff;
        }
        /// <summary>
        /// 通过Buff配置表的id取
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Buff GetByConfigId(this BuffComponent self, int id)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(id);
            if (self.Groups.ContainsKey(config.Group))
            {
                Buff buff = self.GetChild<Buff>(self.Groups[config.Group]);
                if (buff.ConfigId == id)
                {
                    return buff;
                }
            }

            return null;
        }
        /// <summary>
        /// 通过Buff的唯一Id移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        public static void Remove(this BuffComponent self, long id)
        {
            Buff buff = self.GetChild<Buff>(id);
            if(buff==null) return;
            EventSystem.Instance.Publish(new EventType.AfterRemoveBuff(){Buff = buff});
            self.Groups.Remove(buff.Config.Group);
            self.AllBuff.Remove(id);
            buff.Dispose();
        }
        /// <summary>
        /// 通过Buff配置表的id移除buff
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        public static void RemoveByConfigId(this BuffComponent self, int id)
        {
            BuffConfig config = BuffConfigCategory.Instance.Get(id);
            if (self.Groups.ContainsKey(config.Group))
            {
                Buff buff = self.GetChild<Buff>(self.Groups[config.Group]);
                if (buff.ConfigId == id)
                {
                    self.Groups.Remove(buff.Config.Group);
                    buff?.Dispose();
                }
            }
        }
        
        /// <summary>
        /// 造成伤害前
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        public static void BeforeDamage(this BuffComponent self, Unit attacker,Unit target,DamageInfo damage)
        {
            for (int i = 0; i < self.AllBuff.Count; i++)
            {
                var buff = self.GetChild<Buff>(self.AllBuff[i]);
                for (int j = 0; j < buff.Config.Type.Length; j++)
                {
                    BuffWatcherComponent.Instance.BeforeDamage(buff.Config.Type[j],attacker,target,buff,damage);
                }
            }
        }
        
        /// <summary>
        /// 造成伤害后
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        public static void AfterDamage(this BuffComponent self, Unit attacker,Unit target,DamageInfo damage)
        {
            for (int i = 0; i < self.AllBuff.Count; i++)
            {
                var buff = self.GetChild<Buff>(self.AllBuff[i]);
                for (int j = 0; j < buff.Config.Type.Length; j++)
                {
                    BuffWatcherComponent.Instance.AfterDamage(buff.Config.Type[j], attacker, target, buff, damage);
                }
            }
        }
#if !SERVER
        /// <summary>
        /// 展示所有BUFF
        /// </summary>
        /// <param name="self"></param>
        public static void ShowAllBuffView(this BuffComponent self)
        {
            foreach (var item in self.Groups)
            {
                EventSystem.Instance.Publish(new EventType.AfterAddBuff(){Buff = self.GetChild<Buff>(item.Value)});
            }
        }
        
        /// <summary>
        /// 隐藏所有BUFF效果
        /// </summary>
        /// <param name="self"></param>
        public static void HideAllBuffView(this BuffComponent self)
        {
            foreach (var item in self.Groups)
            {
                EventSystem.Instance.Publish(new EventType.AfterRemoveBuff(){Buff = self.GetChild<Buff>(item.Value)});
            }
        }
#endif
    }

}