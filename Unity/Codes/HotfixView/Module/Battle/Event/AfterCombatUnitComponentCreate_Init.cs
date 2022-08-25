
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [FriendClass(typeof(CombatUnitComponent))]
    public class AfterCombatUnitComponentCreate_Init:AEvent<EventType.AfterCombatUnitComponentCreate>
    {
        protected override void Run(EventType.AfterCombatUnitComponentCreate args)
        {
            var self = args.CombatUnitComponent;
            if (UnitHelper.GetMyUnitFromZoneScene(self.ZoneScene())== self.unit)
            {
                self.AddComponent<SpellPreviewComponent>();
            }
        }
    }
}