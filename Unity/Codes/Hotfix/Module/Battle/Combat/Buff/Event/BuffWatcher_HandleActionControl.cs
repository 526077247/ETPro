namespace ET
{
    [AddBuffWatcher(BuffSubType.ActionControl)]
    [FriendClass(typeof(BuffComponent))]
    public class AddBuff_HandleActionControl:IAddBuffWatcher
    {
        public void AfterAdd(Unit attacker, Unit target, Buff buff)
        {
            AddBuffActionControl(buff, target);
        }

        public void BeforeAdd(Unit attacker, Unit target, int id, ref bool canAdd)
        {
            
        }
        
        /// <summary>
        /// 添加行为禁制
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        void AddBuffActionControl(Buff self, Unit unit)
        {
            var buffComp = self.GetParent<BuffComponent>();
            if (self.ActionControlConfig.ActionControl != null)
            {
                for (int i = 0; i < self.ActionControlConfig.ActionControl.Length; i++)
                {
                    var type = self.ActionControlConfig.ActionControl[i];
                    if (!buffComp.ActionControls.ContainsKey(type)||buffComp.ActionControls[type]==0)
                    {
                        buffComp.ActionControls[type] = 1;
                        // Log.Info("BuffWatcherComponent");
                        BuffWatcherComponent.Instance.SetActionControlActive(type,true,unit);
                    }
                    else
                    {
                        buffComp.ActionControls[type]++;
                    }
                }
            }
        }
    }

    [RemoveBuffWatcher(BuffSubType.ActionControl)]
    [FriendClass(typeof(BuffComponent))]
    public class RemoveBuff_HandleActionControl: IRemoveBuffWatcher
    {
        public void AfterRemove(Unit target, Buff buff)
        {
            RemoveBuffActionControl(buff, target);
        }

        public void BeforeRemove(Unit target, Buff buff, ref bool canRemove)
        {
            
        }
        
        
        /// <summary>
        /// 移除行为禁制
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public static void RemoveBuffActionControl(Buff self, Unit unit)
        {
            var buffComp = self.GetParent<BuffComponent>();
            if (self.ActionControlConfig.ActionControl != null)
            {
                for (int i = 0; i < self.ActionControlConfig.ActionControl.Length; i++)
                {
                    var type = self.ActionControlConfig.ActionControl[i];
                    if (buffComp.ActionControls.ContainsKey(type)&&buffComp.ActionControls[type]>0)
                    {
                        buffComp.ActionControls[type]--;
                        if (buffComp.ActionControls[type] == 0)
                        {
                            // Log.Info("BuffWatcherComponent");
                            BuffWatcherComponent.Instance.SetActionControlActive(type,false,unit);
                        }
                    }
                }
            }
        }
    }
}