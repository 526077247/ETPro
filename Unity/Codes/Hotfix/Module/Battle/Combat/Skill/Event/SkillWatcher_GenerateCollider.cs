using UnityEngine;
using System.Collections.Generic;
using System;

namespace ET
{

    /// <summary>
    /// 生成碰撞体
    /// </summary>
    [SkillWatcher(SkillStepType.GenerateCollider)]
    public class SkillWatcher_GenerateCollider : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
#if SERVER
            if (para.From.unit.IsGhost()) return;
            int curIndex = para.CurIndex;
            var stepPara = para.StepPara[curIndex];
            Log.Info("SkillWatcher_GenerateCollider");
            if(int.TryParse(stepPara.Paras[0].ToString(),out var colliderId))
            {
                SkillJudgeConfig collider = SkillJudgeConfigCategory.Instance.Get(colliderId);
                if (collider != null)
                {
                    var aoiUnit = para.From.unit.GetComponent<AOIUnitComponent>();
                    var scene = aoiUnit.Scene.GetParent<Scene>();
                    Unit unit = null;
                    Vector3 FromUnitPos = para.From.unit.Position;
                    Vector3 ToUnitPos = Vector3.zero;
                    if(para.To!=null)
                        ToUnitPos = para.To.unit.Position;

                    #region 创建碰撞体AOIUnit
                    
                    if (collider.ColliderType == SkillJudgeType.FixedPosition)//固定位置碰撞体
                    {
                        if(collider.StartPosType == ColliderStartPosType.Self)
                            unit = UnitFactory.CreateSkillCollider(scene,colliderId, FromUnitPos,para.Rotation,para);
                        else if (collider.StartPosType == ColliderStartPosType.Aim && para.To != null)
                        {
                            unit = UnitFactory.CreateSkillCollider(scene, colliderId,ToUnitPos,para.Rotation,para);
                        }
                        else if(collider.StartPosType == ColliderStartPosType.MousePos)
                            unit = UnitFactory.CreateSkillCollider(scene,colliderId, para.Position,para.Rotation,para);
                        else
                        {
                            Log.Info("目标未指定,或触发体类型不存在");
                            return;
                        }
                    }
                    else if (collider.ColliderType == SkillJudgeType.FixedRotation)//固定方向碰撞体
                    {
                        var dir =new Vector3(para.Position.x - FromUnitPos.x,para.Position.y- FromUnitPos.y, para.Position.z - FromUnitPos.z).normalized;
                        if (collider.ColliderShape == SkillColliderShapeType.OBB)//立方找到中点
                        {
                            var point = FromUnitPos + dir * collider.ColliderPara[2] / 2;
                            if (collider.StartPosType == ColliderStartPosType.Self)
                            {
                                unit = UnitFactory.CreateSkillCollider(scene,colliderId, point, para.Rotation, para);
                            }
                            else
                            {
                                Log.Info("目标未指定,或触发体类型不存在");
                                return;
                            }
                        }
                        else
                        {
                            Log.Info("目标未指定,或触发体类型不存在");
                            return;
                        }
                    }
                    else if (collider.ColliderType == SkillJudgeType.Target)//朝指定位置方向飞行碰撞体
                    {
                        Vector3 startPos = FromUnitPos;
                        if (collider.StartPosType == ColliderStartPosType.Self)
                            startPos = FromUnitPos;
                        else if(collider.StartPosType == ColliderStartPosType.Aim&&para.To!=null)
                            startPos = ToUnitPos;
                        else if (collider.StartPosType == ColliderStartPosType.MousePos)
                            startPos = para.Position;
                        else
                        {
                            Log.Info("目标未指定,或触发体类型不存在");
                            return;
                        }
                        unit = UnitFactory.CreateSkillCollider(scene,colliderId, startPos,para.Rotation,para);
                    }
                    else if (collider.ColliderType == SkillJudgeType.Aim)//锁定目标飞行
                    {
                        Vector3 startPos = FromUnitPos;
                        if (collider.StartPosType == ColliderStartPosType.Self&&para.To!=null)
                            startPos = FromUnitPos;
                        else if(collider.StartPosType == ColliderStartPosType.Aim&&para.To!=null)
                            startPos = ToUnitPos;
                        else if (collider.StartPosType == ColliderStartPosType.MousePos&&para.To!=null)
                            startPos = para.Position;
                        else
                        {
                            Log.Info("目标未指定,或触发体类型不存在");
                            return;
                        }
                        unit = UnitFactory.CreateSkillCollider(scene,colliderId, startPos,para.Rotation,para);
                        
                    }
                    else if (collider.ColliderType == SkillJudgeType.Immediate) //立刻结算
                    {
                        if (collider.StartPosType == ColliderStartPosType.Self)
                            EventSystem.Instance.Publish(new EventType.OnSkillTrigger
                            {
                                From = aoiUnit,
                                To = aoiUnit,
                                Para = stepPara,
                                Type = AOITriggerType.Enter,
                                Config = para.Ability.SkillConfig,
                                Cost = para.Cost,
                                CostId =  para.CostId,
                            });
                        else if(collider.StartPosType == ColliderStartPosType.Aim&&para.To!=null)
                            EventSystem.Instance.Publish(new EventType.OnSkillTrigger
                            {
                                From = aoiUnit,
                                To = para.To.unit.GetComponent<AOIUnitComponent>(),
                                Para = stepPara,
                                Type = AOITriggerType.Enter,
                                Config = para.Ability.SkillConfig,
                                Cost = para.Cost,
                                CostId =  para.CostId,
                            });
                        else if (collider.StartPosType == ColliderStartPosType.MousePos)
                        {
                            Log.Error("立刻结算类型,必须指定目标");
                            return;
                        }
                        else
                        {
                            Log.Info("目标未指定,或触发体类型不存在");
                            return;
                        }
                    }
                    else
                    {
                        Log.Error("碰撞体类型未处理"+collider.ColliderType);
                        return;
                    }
                    #endregion
                    
                    
                    
                }
                
            }
#endif
        }

        
    }
}