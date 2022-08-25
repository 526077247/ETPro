using System;
using UnityEngine;

namespace ET
{
    [FriendClass(typeof(BuffComponent))]
    public class AfterAddBuff_CreateBuffView: AEvent<EventType.AfterAddBuff>
    {
        protected override void Run(EventType.AfterAddBuff args)
        {
            if (args.Buff.Config.EffectId != 0)
            {
                EventSystem.Instance.Publish(new EventType.AddEffect
                {
                    EffectId = args.Buff.Config.EffectId,
                    Parent = args.Buff,
                    Unit = args.Buff.GetParent<BuffComponent>().unit
                });
            }
        }

    }
}