using UnityEngine;
using System;
namespace ET
{
    [FriendClass(typeof(AOIUnitComponent))]
    [FriendClass(typeof(AOITrigger))]
    [FriendClass(typeof(OBBComponent))]
    [FriendClass(typeof(GameObjectComponent))]
    [FriendClass(typeof(GlobalComponent))]
    public class AOIRegisterUnit_CreateUnitView : AEvent<EventType.AOIRegisterUnit>
    {
        protected override void Run(EventType.AOIRegisterUnit args)
        {
            var myunitId = args.Receive.GetMyUnitIdFromZoneScene();
            if (args.Receive.Id != myunitId)
            {
                // Log.Info("args.Receive.Id != myunitId  "+args.Unit.Id+"   " +args.Receive.Id +"  "+ myunitId);
                return;
            }
            for (int i = 0; i < args.Units.Count; i++)
            {
                var unit = args.Units[i].GetParent<Unit>();
                RunAsync(unit).Coroutine();
            }
        }

        public async ETTask RunAsync(Unit unit)
        {
            GameObjectComponent showObj;
            if (unit.Type==UnitType.Player||unit.Type==UnitType.Monster)//人物怪物类
            {
                Log.Info("AOIRegisterUnit"+unit.Id);
                // Unit View层
                // 这里可以改成异步加载，demo就不搞了
                var go = await GameObjectPoolComponent.Instance.GetGameObjectAsync(unit.Config.Perfab);
                var trans = go.GetComponentsInChildren<Transform>();
                for (int i = 0; i < trans.Length; i++)
                {
                    trans[i].gameObject.layer = LayerMask.NameToLayer("Unit");
                }
                go.transform.position = unit.Position;
                go.transform.parent = GlobalComponent.Instance.Unit;
                var idc = go.GetComponent<UnitIdComponent>();
                if (idc == null)
                    idc = go.AddComponent<UnitIdComponent>();
                idc.UnitId = unit.Id;
                showObj = unit.AddComponent<GameObjectComponent,GameObject,Action>(go, () =>
                {
                    GameObjectPoolComponent.Instance?.RecycleGameObject(go);
                });
                unit.AddComponent<AnimatorComponent>();
                
                unit.AddComponent<InfoComponent>();
                var combatU = unit.GetComponent<CombatUnitComponent>();
                if (combatU != null)
                {
                    combatU.GetComponent<BuffComponent>()?.ShowAllBuffView();
                }
            }
            else if (unit.Type==UnitType.Skill)
            {
                SkillColliderComponent colliderComponent = unit.GetComponent<SkillColliderComponent>();
                var go = await GameObjectPoolComponent.Instance.GetGameObjectAsync(unit.Config.Perfab);
                var trans = go.GetComponentsInChildren<Transform>();
                for (int i = 0; i < trans.Length; i++)
                {
                    trans[i].gameObject.layer = LayerMask.NameToLayer("Skill");
                }
                go.transform.position = unit.Position;
                go.transform.parent = GlobalComponent.Instance.Unit;
                go.transform.rotation = unit.Rotation;
                showObj = unit.AddComponent<GameObjectComponent,GameObject,Action>(go, () =>
                {
                    GameObjectPoolComponent.Instance?.RecycleGameObject(go);
                });
            }
            else
            {
                Log.Error("类型未处理");
                return;
            }
            if (GlobalComponent.Instance.ColliderDebug)
            {
                await TimerComponent.Instance.WaitAsync(10);
                var SphereTriggers = unit.GetComponent<AOIUnitComponent>().SphereTriggers;
                for (int i = 0; i < SphereTriggers.Count; i++)
                {
                    var item = SphereTriggers[i];
                    GameObject obj;
                    if (item.TriggerType == TriggerShapeType.Sphere)
                    {
                        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        obj.GetComponent<Collider>().isTrigger = true;
                        obj.transform.parent = showObj.GameObject.transform;
                        obj.transform.localPosition = new Vector3(0,item.OffsetY,0);
                        obj.transform.localScale = new Vector3(item.Radius*2,item.Radius*2,item.Radius*2);
                    }
                    else if (item.TriggerType == TriggerShapeType.Cube)
                    {
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.GetComponent<Collider>().isTrigger = true;
                        obj.transform.parent = showObj.GameObject.transform;
                        obj.transform.localPosition = new Vector3(0,item.OffsetY,0);
                        obj.transform.localRotation = Quaternion.identity;
                        obj.transform.localScale = item.GetComponent<OBBComponent>().Scale;
                    }
                    else
                    {
                        Log.Error("Define.Debug 碰撞体未添加");
                        continue;
                    }
                    
                    var debugObj = showObj.AddChild<GameObjectComponent, GameObject, Action>(obj, () =>
                    {
                        GameObject.Destroy(obj);
                    });
                    debugObj.IsDebug = true;
                }
            }
        }
    }
}