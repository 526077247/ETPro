namespace ET
{

    [ActorMessageHandler]
    public class M2M_RemoveBuffHandler : AMActorLocationHandler<Scene, M2M_RemoveBuff>
    {
        protected override async ETTask Run(Scene scene, M2M_RemoveBuff message)
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
                var buff = buffC.GetByConfigId(message.ConfigId);
                if(buff!=null) buffC.RemoveByOther(buff.Id,true);
            }
            await ETTask.CompletedTask;
        }
    }
}