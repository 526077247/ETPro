namespace ET
{
    [AddBuffWatcher(BuffSubType.Bleed)]
    public class AddBuff_StartBleed:IAddBuffWatcher
    {
        public void AfterAdd(Unit attacker, Unit target, Buff buff)
        {
            buff.AddComponent<BuffBleedComponent,int>(buff.Config.Id);
        }

        public void BeforeAdd(Unit attacker, Unit target, int id, ref bool canAdd)
        {
            
        }
    }

    [RemoveBuffWatcher(BuffSubType.Bleed)]
    public class RemoveBuff_EndBleed: IRemoveBuffWatcher
    {
        public void AfterRemove(Unit target, Buff buff)
        {
            buff.RemoveComponent<BuffBleedComponent>();
        }

        public void BeforeRemove(Unit target, Buff buff, ref bool canRemove)
        {
            
        }
    }
}