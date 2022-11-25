using System;
using UnityEngine;
namespace ET
{
    [Timer(TimerType.SkillColliderRemove)]
    public class SkillColliderRemove: ATimer<Unit>
    {
        public override void Run(Unit self)
        {
            try
            {
                self.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    [Timer(TimerType.GenerateSkillCollider)]
    public class GenerateSkillCollider: ATimer<SkillColliderComponent>
    {
        public override void Run(SkillColliderComponent self)
        {
            try
            {
                self.GenerateSkillCollider();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    public class SkillColliderAwakeSystem1 : AwakeSystem<SkillColliderComponent, SkillPara>
    {
        public override void Awake(SkillColliderComponent self, SkillPara a)
        {
            self.Awake(a);
        }
    }
    public class SkillColliderAwakeSystem2 : AwakeSystem<SkillColliderComponent, SkillPara,long>
    {
        public override void Awake(SkillColliderComponent self, SkillPara a, long b)
        {
            self.ToId = b;
            self.Awake(a);
        }
    }
    public class SkillColliderAwakeSystem3 : AwakeSystem<SkillColliderComponent, SkillPara,Vector3>
    {
        public override void Awake(SkillColliderComponent self, SkillPara a, Vector3 b)
        {
            self.Position = b;
            self.Awake(a);
        }
    }

    public class SkillColliderDestroySystem: DestroySystem<SkillColliderComponent>
    {
        public override void Destroy(SkillColliderComponent self)
        {
            TimerComponent.Instance.Remove(ref self.GenerateSkillColliderTimer);
            TimerComponent.Instance.Remove(ref self.SkillColliderRemoveTimer);
        }
    }

    [FriendClass(typeof(SkillColliderComponent))]
    [FriendClass(typeof(SkillPara))]
    public static class SkillColliderComponentSystem
    {
        public static void Awake(this SkillColliderComponent self, SkillPara para)
        {
            self.SkillGroup = para.CurGroup;
            self.Index = para.CurIndex;
            
            var stepPara = para.GetCurSkillStepPara();
            self.Cost = para.Cost;
            self.CostId = para.CostId;
            self.SkillConfigId = para.SkillConfigId;

            self.FromId = para.From.Id;
            
            if (StepParaHelper.TryParseInt(ref stepPara.Paras[0], out var colliderId))
            {
                self.ConfigId = colliderId;
                int deltaTime = 0;
                if (stepPara.Paras.Length >= 6)
                {
                    StepParaHelper.TryParseInt(ref stepPara.Paras[5], out deltaTime);
                }
                if (deltaTime <= 0)
                {
                    deltaTime = 1;//等下一帧
                }

                self.CreateViewTime = TimeHelper.ServerNow();
                self.CreateColliderTime =self.CreateViewTime + deltaTime;
                self.OnCreate();
            }
            else
            {
                Log.Error("stepPara.Paras[0] Error! "+stepPara.Paras[0]);
            }
        }
        /// <summary>
        /// 创建调用，可能是本地图产生的，也可能是传送过来的
        /// </summary>
        /// <param name="self"></param>
        public static void OnCreate(this SkillColliderComponent self)
        {
            #region 添加触发器
            
            if (self.Config.ColliderShape == ColliderShape.None)
            {
                return;
            }
            else if (self.Config.ColliderShape == ColliderShape.Sphere||
                     self.Config.ColliderShape == ColliderShape.OBB)
            {
                if (self.CreateColliderTime <= TimeHelper.ServerNow())
                {
                    self.GenerateSkillCollider();
                }
                else
                {
                    self.GenerateSkillColliderTimer = TimerComponent.Instance.NewOnceTimer(self.CreateColliderTime, TimerType.GenerateSkillCollider, self);
                }
            }
            else
            {
                Log.Error("碰撞体形状未处理" + self.Config.ColliderType);
                return;
            }

            #endregion

            self.SkillColliderRemoveTimer = TimerComponent.Instance.NewOnceTimer(self.CreateViewTime + self.Config.Time,
                TimerType.SkillColliderRemove, self.Unit);
        }
        
        public static void GenerateSkillCollider(this SkillColliderComponent self)
        {
            var aoiUnit = self.Unit.Parent.GetChild<Unit>(self.FromId).GetComponent<AOIUnitComponent>();
            var skillAOIUnit = self.Unit.GetComponent<AOIUnitComponent>();
            if (skillAOIUnit == null||skillAOIUnit.IsDisposed)
            {
                Log.Info("skillAOIUnit == null||skillAOIUnit.IsDisposed");
                return;
            }
            if (self.Config.ColliderShape == ColliderShape.OBB)
            {
                Vector3 par = new Vector3(self.Config.ColliderPara[0], self.Config.ColliderPara[1],
                    self.Config.ColliderPara[2]);
                skillAOIUnit.AddOBBTrigger(par, AOITriggerType.All,
                    (o, e) =>
                    {
                        EventSystem.Instance.Publish(new EventType.OnSkillTrigger
                        {
                            Skill = skillAOIUnit,
                            From = aoiUnit,
                            To = o,
                            Para = self.GetPara(aoiUnit),
                            Config = self.SkillConfig,
                            Cost = self.Cost,
                            CostId = self.CostId,
                            Type = e
                        });
                    },  UnitType.ALL);
            }
            else if (self.Config.ColliderShape == ColliderShape.Sphere)
            {
                skillAOIUnit.AddSphereTrigger(self.Config.ColliderPara[0], AOITriggerType.All,
                    (o, e) =>
                    {
                        EventSystem.Instance.Publish(new EventType.OnSkillTrigger
                        {
                            Skill = skillAOIUnit,
                            From = aoiUnit,
                            To = o,
                            Para = self.GetPara(aoiUnit),
                            Config = self.SkillConfig,
                            Cost = self.Cost,
                            CostId = self.CostId,
                            Type = e
                        });
                    }, UnitType.ALL);
            }
            else
            {
                Log.Error("碰撞体形状未处理" + self.Config.ColliderType);
                return;
            }
            if (self.Config.ColliderType == ColliderType.Immediate)
            {
                self.Unit.Dispose();
            }
        }

        private static SkillStepPara GetPara(this SkillColliderComponent self,AOIUnitComponent aoiU)
        {
            var para = aoiU.Parent.GetComponent<CombatUnitComponent>()?.GetComponent<SpellComponent>()?.GetComponent<SkillPara>();
            if (para != null && para.SkillConfigId == self.SkillConfigId)
            {
                return para.GetSkillStepPara(self.SkillGroup, self.Index);
            }

            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(self.SkillConfigId, self.SkillGroup);
            return new SkillStepPara()
            {
                Index = self.Index,
                Paras = SkillStepComponent.Instance.GetSkillStepParas(conf.Id)[self.Index]
            };
        }
    }
}