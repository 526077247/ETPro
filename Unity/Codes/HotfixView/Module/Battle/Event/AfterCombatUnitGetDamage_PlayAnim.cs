namespace ET
{
    [FriendClass(typeof(CombatUnitComponent))]
    public class AfterCombatUnitGetDamage_PlayAnim:AEvent<EventType.AfterCombatUnitGetDamage>
    {
        protected override void Run(EventType.AfterCombatUnitGetDamage args)
        {
            var anim = args.Unit.unit.GetComponent<AnimatorComponent>();
            if (anim != null)
            {
                if(args.Unit.unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Hp)<=0)
                {
                    anim.Play(MotionType.Died);
                }
                else
                    anim.Play(MotionType.Damage);
            }
            else if(args.Unit.unit.GetComponent<NumericComponent>().GetAsInt(NumericType.Hp)<=0)//直接死了
            {
                args.Unit.unit.Dispose();
            }
            
        }
        
    }
}