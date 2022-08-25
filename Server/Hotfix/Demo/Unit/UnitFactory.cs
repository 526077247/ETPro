using System;
using UnityEngine;
using System.Collections.Generic;

namespace ET
{
    public static class UnitFactory
    {

        public static void AfterCreateUnitFromMsg(Unit unit,CreateUnitFromMsgType type)
        {
            var scene = unit.Parent.GetParent<Scene>();
            UnitType unitType = unit.Type;
            switch (unitType)
            {
                case UnitType.Player:
                {
                    if (unit.GetComponent<AOIUnitComponent>()==null)
                    {
                        unit.AddComponent<MoveComponent>();
                        MapSceneConfig conf = MapSceneConfigCategory.Instance.Get((int) scene.Id);
                        unit.AddComponent<PathfindingComponent, string>(conf.Recast);
                        var numericComponent = unit.GetComponent<NumericComponent>();

                        // 加入aoi
                        var aoiu = unit.AddComponent<AOIUnitComponent, Vector3, Quaternion, UnitType, int,bool>(unit.Position, unit.Rotation, unit.Type,
                            numericComponent.GetAsInt(NumericType.AOI),type!=CreateUnitFromMsgType.Create);
                        aoiu.AddSphereCollider(0.5f);
                        if (type != CreateUnitFromMsgType.Create)
                        {
                            aoiu.GetComponent<GhostComponent>().IsGoast = type == CreateUnitFromMsgType.Add;
                        }
                    }
                    else
                    {
                        var aoiu = unit.GetComponent<AOIUnitComponent>();
                        aoiu.GetComponent<GhostComponent>().IsGoast = type == CreateUnitFromMsgType.Add;
                    }
                    if (type!=CreateUnitFromMsgType.Add)
                    {
                        if (unit.GetComponent<MailBoxComponent>() == null)
                        {
                            unit.AddComponent<MailBoxComponent>();
                        }
                        else
                        {
                            Log.Error("??? unit.GetComponent<MailBoxComponent>() != null");
                        }
                    }
                    break;
                }
                case UnitType.Skill:
                {
                    if (unit.GetComponent<AOIUnitComponent>()==null)
                    {
                        var skillInfo = unit.GetComponent<SkillColliderComponent>();
                        var pos = unit.Position;
                        var collider = skillInfo.Config;
                        if (collider.ColliderType == SkillJudgeType.Target)//朝指定位置方向飞行碰撞体
                        {
                            var moveComp = unit.AddComponent<MoveComponent>();
                            List<Vector3> target = new List<Vector3>();
                            target.Add(pos);
                            target.Add(pos + (skillInfo.Position - pos).normalized * collider.Speed * collider.Time / 1000f);
                            moveComp.MoveToAsync(target, collider.Speed).Coroutine();
                        }
                        else if (collider.ColliderType == SkillJudgeType.Aim) //锁定目标飞行
                        {
                            var toUnit = unit.Parent.GetChild<Unit>(skillInfo.ToId);
                            unit.AddComponent<ZhuiZhuAimComponent, Unit, Action>(toUnit, () =>
                            {
                                unit.Dispose();
                            });
                            unit.AddComponent<AIComponent,int,int>(2,50);
                        }
                        var aoiu =unit.AddComponent<AOIUnitComponent,Vector3,Quaternion, UnitType,bool>(pos,unit.Rotation,unit.Type,type!=CreateUnitFromMsgType.Create);
                        skillInfo.OnCreate();
                        if (type != CreateUnitFromMsgType.Create)
                        {
                            aoiu.GetComponent<GhostComponent>().IsGoast = type == CreateUnitFromMsgType.Add;
                        }
                    }
                    else
                    {
                        var aoiu = unit.GetComponent<AOIUnitComponent>();
                        aoiu.GetComponent<GhostComponent>().IsGoast = type == CreateUnitFromMsgType.Add;
                    }
                    break;
                }

            }
            
        }

        public static Unit Create(Scene scene, long id, UnitType unitType)
        {
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            switch (unitType)
            {
                case UnitType.Player:
                {
                    Unit unit = unitComponent.AddChildWithId<Unit, int>(id, 1);
                    //ChildType测试代码 取消注释 编译Server.hotfix 可发现报错
                    //unitComponent.AddChild<Player, string>("Player");
                    unit.AddComponent<MoveComponent>();
                    unit.Position = new Vector3(-10, 0, -10);
			
                    NumericComponent numericComponent = unit.AddComponent<NumericComponent>();
                    numericComponent.Set(NumericType.SpeedBase, 6f); // 速度是6米每秒
                    numericComponent.Set(NumericType.AOIBase, 2); // 视野2格
                    numericComponent.Set(NumericType.HpBase, 1000); // 生命1000
                    numericComponent.Set(NumericType.MaxHpBase, 1000); // 最大生命1000
                    numericComponent.Set(NumericType.LvBase,1); //1级
                    numericComponent.Set(NumericType.ATKBase,100); //100攻击
                    numericComponent.Set(NumericType.DEFBase,500); //500防御
                    var SkillIds = new List<int>(){1001,1002,1003,1004,1005};//初始技能
                    unit.AddComponent<CombatUnitComponent,List<int>>(SkillIds);
                    unitComponent.Add(unit);
                    // 进入地图再加入aoi
                    
                    return unit;
                }
                default:
                    throw new Exception($"not such unit type: {unitType}");
            }
        }
        
        public static Unit CreateSkillCollider(Scene currentScene, int configId, Vector3 pos,Quaternion rota,SkillPara para)
        {
            UnitComponent unitComponent = currentScene.GetComponent<UnitComponent>();
            Unit unit = unitComponent.AddChild<Unit,int>(configId);
        
            unit.Position = pos;
            unit.Rotation = rota;
            var collider = SkillJudgeConfigCategory.Instance.Get(configId);
            if (collider.ColliderType == SkillJudgeType.Target)//朝指定位置方向飞行碰撞体
            {
                var numc = unit.AddComponent<NumericComponent>();
                numc.Set(NumericType.SpeedBase, collider.Speed);
                var moveComp = unit.AddComponent<MoveComponent>();
                List<Vector3> target = new List<Vector3>();
                target.Add(pos);
                target.Add(pos + (para.Position - pos).normalized * collider.Speed * collider.Time / 1000f);
                moveComp.MoveToAsync(target, collider.Speed).Coroutine();
                unit.AddComponent<SkillColliderComponent,SkillPara,Vector3>(para,para.Position);
            }
            else if (collider.ColliderType == SkillJudgeType.Aim) //锁定目标飞行
            {
                var numc = unit.AddComponent<NumericComponent>();
                numc.Set(NumericType.SpeedBase,collider.Speed);
                unit.AddComponent<MoveComponent>();
                unit.AddComponent<ZhuiZhuAimComponent, Unit, Action>(para.To.unit, () =>
                {
                    unit.Dispose();
                });
                unit.AddComponent<AIComponent,int,int>(2,50);
                unit.AddComponent<SkillColliderComponent,SkillPara,long>(para,para.To.Id);
            }
            else
            {
                unit.AddComponent<SkillColliderComponent, SkillPara>(para);
            }
            unit.AddComponent<AOIUnitComponent,Vector3,Quaternion, UnitType>(pos,rota,unit.Type);
            return unit;
        }
    }
}