namespace ET
{
    public class AOIRemoveUnit_BeforeUnitRemove: AEvent<EventType.AOIRemoveUnit>
    {
        protected override void Run(EventType.AOIRemoveUnit args)
        {
            if (args.Units == null) return;
            var myunitId = args.Receive.GetMyUnitIdFromZoneScene();
            if (args.Receive.Id != myunitId)
            {
                return;
            }
        
            for (int i = 0; i < args.Units.Count; i++)
            {
                var unit = args.Units[i].GetParent<Unit>();
                var combatU = unit.GetComponent<CombatUnitComponent>();
                if (combatU != null)
                {
                    combatU.GetComponent<BuffComponent>()?.HideAllBuffView();
                }
                unit.RemoveComponent<AnimatorComponent>();
                unit.RemoveComponent<InfoComponent>();
                // unit.RemoveComponent<NumberComponent>();
                unit.RemoveComponent<GameObjectComponent>();
            }
        }
   
    }
}