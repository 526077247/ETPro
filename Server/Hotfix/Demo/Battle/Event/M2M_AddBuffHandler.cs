namespace ET
{

    [ActorMessageHandler]
    public class M2M_AddBuffHandler : AMActorLocationHandler<Scene, M2M_AddBuff>
    {
        protected override async ETTask Run(Scene scene, M2M_AddBuff message)
        {
            UnitComponent uc = scene.GetComponent<UnitComponent>();
            var unit = uc.Get(message.UnitId);
            if (unit != null)
            {
                var combatU = unit.GetComponent<CombatUnitComponent>();
                if (combatU == null)
                {
                    Log.Info("combatU == null "+message.UnitId);
                    combatU = unit.AddComponent<CombatUnitComponent>();
                }

                var buffC = combatU.GetComponent<BuffComponent>();
                buffC.AddBuff(message.ConfigId, message.Timestamp,message.SourceId);
            }
            await ETTask.CompletedTask;
        }
    }
}