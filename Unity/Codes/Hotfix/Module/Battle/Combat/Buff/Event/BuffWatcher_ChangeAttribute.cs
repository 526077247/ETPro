namespace ET
{
    [AddBuffWatcher(BuffSubType.Attribute)]
    public class AddBuff_ChangeAttribute:IAddBuffWatcher
    {
        public void AfterAdd(Unit attacker, Unit target, Buff buff)
        {
            AddBuffAttrValue(buff, target);
        }

        public void BeforeAdd(Unit attacker, Unit target, int id, ref bool canAdd)
        {
            
        }
        
        /// <summary>
        /// 添加BUFF属性加成
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        void AddBuffAttrValue(Buff self, Unit unit)
        {
            if (self.AttrConfig.AttributeType != null)
            {
                var numc = unit.GetComponent<NumericComponent>();
                if (numc != null)
                {
                    for (int i = 0; i < self.AttrConfig.AttributeType.Length; i++)
                    {
                        if (NumericType.Map.TryGetValue(self.AttrConfig.AttributeType[i], out var attr))
                        {
                            if (self.AttrConfig.AttributeAdd != null && self.AttrConfig.AttributeAdd.Length > i)
                                numc.Set(attr * 10 + 2, numc.GetAsInt(attr * 10 + 2) + self.AttrConfig.AttributeAdd[i]);
                            if (self.AttrConfig.AttributePct != null && self.AttrConfig.AttributePct.Length > i)
                                numc.Set(attr * 10 + 3, numc.GetAsInt(attr * 10 + 3) + self.AttrConfig.AttributePct[i]);
                            if (self.AttrConfig.AttributeFinalAdd != null && self.AttrConfig.AttributeFinalAdd.Length > i)
                                numc.Set(attr * 10 + 4,
                                    numc.GetAsInt(attr * 10 + 4) + self.AttrConfig.AttributeFinalAdd[i]);
                            if (self.AttrConfig.AttributeFinalPct != null && self.AttrConfig.AttributeFinalPct.Length > i)
                                numc.Set(attr * 10 + 5,
                                    numc.GetAsInt(attr * 10 + 5) + self.AttrConfig.AttributeFinalPct[i]);
                        }
                        else
                        {
                            Log.Info("BuffConfig属性没找到 【" + self.AttrConfig.AttributeType[i]+"】");
                        }
                    }
                }
                else
                {
                    Log.Error("添加BUFF id= " + unit.Id + " 时没找到 NumericComponent 组件");
                }
            }
        }
    }

    [RemoveBuffWatcher(BuffSubType.Attribute)]
    public class RemoveBuff_ChangeAttribute: IRemoveBuffWatcher
    {
        public void AfterRemove(Unit target, Buff buff)
        {
            if(buff.AttrConfig.IsRemove == 0)
                RemoveBuffAttrValue(buff,target);
        }

        public void BeforeRemove(Unit target, Buff buff, ref bool canRemove)
        {
            
        }
        
        /// <summary>
        /// 移除BUFF属性加成
        /// </summary>
        /// <param name="self"></param>
        /// <param name="unit"></param>
        public void RemoveBuffAttrValue(Buff self,Unit unit)
        {
            if (self.AttrConfig.AttributeType != null)
            {
                var numc = unit.GetComponent<NumericComponent>();
                if (numc != null)
                {
                    for (int i = 0; i < self.AttrConfig.AttributeType.Length; i++)
                    {
                        if (NumericType.Map.TryGetValue(self.AttrConfig.AttributeType[i], out var attr))
                        {
                            if (self.AttrConfig.AttributeAdd != null && self.AttrConfig.AttributeAdd.Length > i)
                                numc.Set(attr * 10 + 2, numc.GetAsInt(attr * 10 + 2) - self.AttrConfig.AttributeAdd[i]);
                            if (self.AttrConfig.AttributePct != null && self.AttrConfig.AttributePct.Length > i)
                                numc.Set(attr * 10 + 3, numc.GetAsInt(attr * 10 + 3) - self.AttrConfig.AttributePct[i]);
                            if (self.AttrConfig.AttributeFinalAdd != null && self.AttrConfig.AttributeFinalAdd.Length > i)
                                numc.Set(attr * 10 + 4,
                                    numc.GetAsInt(attr * 10 + 4) - self.AttrConfig.AttributeFinalAdd[i]);
                            if (self.AttrConfig.AttributeFinalPct != null && self.AttrConfig.AttributeFinalPct.Length > i)
                                numc.Set(attr * 10 + 5,
                                    numc.GetAsInt(attr * 10 + 5) - self.AttrConfig.AttributeFinalPct[i]);
                        }
                        else
                        {
                            Log.Info("BuffConfig属性没找到 【" + self.AttrConfig.AttributeType[i]+"】");
                        }
                    }
                }
                else
                {
                    Log.Error("移除BUFF id= " + self.Config.Id + " 时没找到 NumericComponent 组件");
                }
                
            }
        }

    }
}