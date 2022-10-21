using UnityEngine;

namespace ET
{
    [AddBuffWatcher(BuffSubType.Chant)]
    public class AddBuff_StartChant:IAddBuffWatcher
    {
        public void AfterAdd(Unit attacker, Unit target, Buff buff)
        {
#if SERVER
            if(buff.BuffChantConfig.MoveInterrupt == 1)
                target.Stop(0);
#else
            //todo:打开读条ui
#endif
        }
    
        public void BeforeAdd(Unit attacker, Unit target, int id, ref bool canRemove)
        {
            
        }
    }

    [RemoveBuffWatcher(BuffSubType.Chant)]
    public class RemoveBuff_EndChant: IRemoveBuffWatcher
    {
        public void AfterRemove(Unit target, Buff buff)
        {
#if !SERVER
            //todo:关闭读条ui
#endif
            
        }

        public void BeforeRemove(Unit target, Buff buff, ref bool canRemove)
        {

        }
    }

    [BuffDamageWatcher(BuffSubType.Chant)]
    public class AfterDamage_TryInterruptChant: IDamageBuffWatcher
    {
        public void AfterDamage(Unit attacker, Unit target, Buff buff, DamageInfo info)
        {
#if SERVER
            if (info.Value>0 && buff.BuffChantConfig.DamageInterrupt == 1)
            {
                var sc = target.GetComponent<CombatUnitComponent>()?.GetComponent<SpellComponent>();
                if (sc != null&&sc.CanInterrupt())
                {
                    sc.Interrupt();
                    var bc = target.GetComponent<CombatUnitComponent>()?.GetComponent<BuffComponent>();
                    bc.Remove(buff.Id);
                }
            }
#endif
        }

        public void BeforeDamage(Unit attacker, Unit target, Buff buff, DamageInfo info)
        {
            
        }
    }
    
    [MoveBuffWatcher(BuffSubType.Chant)]
    public class AfterMove_TryInterruptChant: IMoveBuffWatcher
    {
        public void AfterMove(Unit target, Buff buff, WrapVector3 before)
        {
#if SERVER
            if (buff.BuffChantConfig.MoveInterrupt == 1)
            {
                var sc = target.GetComponent<CombatUnitComponent>()?.GetComponent<SpellComponent>();
                if (sc != null&&sc.CanInterrupt())
                {
                    sc.Interrupt();
                    var bc = target.GetComponent<CombatUnitComponent>()?.GetComponent<BuffComponent>();
                    bc.Remove(buff.Id);
                }
            }
#endif
        }
        
    }
}