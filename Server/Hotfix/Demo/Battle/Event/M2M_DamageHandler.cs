namespace ET
{
    [ActorMessageHandler]
    public class M2M_DamageHandler : AMActorLocationHandler<Scene, M2M_Damage>
    {
        protected override async ETTask Run(Scene scene, M2M_Damage message)
        {
            UnitComponent uc = scene.GetComponent<UnitComponent>();
            Log.Info(message.FromId+"对"+ message.ToId+"造成"+message.Damage+"点伤害");
            var unit = uc.Get(message.ToId);
            if (unit != null)
            {
                var from = uc.Get(message.FromId);
                BattleHelper.Damage(from?.GetComponent<CombatUnitComponent>(),unit.GetComponent<CombatUnitComponent>(),message.Damage,false);
            }
            //tips: ghost受伤和加血的数值计算时序性不重要，最终结果一致就行。但由此引起的死亡、BUFF触发需要以真实实体为准~
            await ETTask.CompletedTask;
        }
    }
}