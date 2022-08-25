using System;
using UnityEngine;
namespace ET
{
    public class AddEffect_CreateEffectView: AEvent<EventType.AddEffect>
    {
        protected override void Run(EventType.AddEffect args)
        {
            RunAsync(args).Coroutine();
        }

        private async ETTask RunAsync(EventType.AddEffect args)
        {
            var unit = args.Unit;
            if (unit != null)
            {
                var showObj = unit.GetComponent<GameObjectComponent>();
                if (showObj == null) return;
                Transform root = null;
                var effectConfig = EffectConfigCategory.Instance.Get(args.EffectId);
                if (effectConfig!=null)
                {
                    root = showObj.GetCollectorObj<GameObject>(effectConfig.MountPoint)?.transform;
                }
                if(root==null) return;
                var obj = await GameObjectPoolComponent.Instance.GetGameObjectAsync(effectConfig.Prefab);
                obj.transform.SetParent(root);
                obj.transform.localPosition = new Vector3(effectConfig.RelativePos[0],effectConfig.RelativePos[1],effectConfig.RelativePos[2]);
                obj.transform.localEulerAngles = new Vector3(effectConfig.RelativeRotation[0],effectConfig.RelativeRotation[1],effectConfig.RelativeRotation[2]);
                obj.transform.localScale = Vector3.one;
                if (effectConfig.IsMount == 0)
                {
                    obj.transform.SetParent(null);
                }
                var buffView = args.Parent.AddChild<GameObjectComponent, GameObject, Action>(obj, () =>
                {
                    GameObjectPoolComponent.Instance?.RecycleGameObject(obj);
                });
                if (effectConfig.PlayTime >= 0)
                {
                    TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + effectConfig.PlayTime, TimerType.DestroyGameObject, buffView);
                }
            }
        }
    }
}