using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class GalGameEngineSuspendedStateAwakeSystem : AwakeSystem<GalGameEngineSuspendedState>
    {
        public override void Awake(GalGameEngineSuspendedState self)
        {
            self.FSM = self.GetParent<FSMComponent>();
        }
    }
    [FSMSystem]
    [FriendClass(typeof(GalGameEngineComponent))]
    public class GalGameEngineSuspendedStateFSMOnEnterSystem : FSMOnEnterSystem<GalGameEngineSuspendedState>
    {
        public override async ETTask FSMOnEnter(GalGameEngineSuspendedState self)
        {
            GalGameEngineComponent.Instance.State = GalGameEngineComponent.GalGameEngineState.Suspended;
            await ETTask.CompletedTask;
        }
    }

   
}
