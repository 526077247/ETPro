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
    [FriendClass(typeof(SkillColliderComponent))]
    public static class SkillColliderComponentSystem
    {
        public static void Awake(this SkillColliderComponent self, SkillPara para)
        {
            int curIndex = para.CurIndex;
            var stepPara = para.StepPara[curIndex];
            self.Cost = para.Cost;
            self.CostId = para.CostId;
            self.SkillConfigId = para.Ability.SkillConfig.Id;

            self.FromId = para.From.Id;
            self.Para = stepPara;
            if (int.TryParse(stepPara.Paras[0].ToString(), out var colliderId))
            {
                self.ConfigId = colliderId;
                int deltaTime = 0;
                if (self.Para.Paras.Length >= 6)
                {
                    int.TryParse(self.Para.Paras[5].ToString(), out deltaTime);
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
        public static void OnCreate(this SkillColliderComponent self)
        {
            #region 添加触发器
            
            if (self.Config.ColliderShape == SkillColliderShapeType.None)
            {
                return;
            }
            else if (self.Config.ColliderShape == SkillColliderShapeType.Sphere||
                     self.Config.ColliderShape == SkillColliderShapeType.OBB)
            {
                if (self.CreateColliderTime <= TimeHelper.ServerNow())
                {
                    self.GenerateSkillCollider();
                }
                else
                {
                    TimerComponent.Instance.NewOnceTimer(self.CreateColliderTime, TimerType.GenerateSkillCollider, self);
                }
            }
            else
            {
                Log.Error("碰撞体形状未处理" + self.Config.ColliderType);
                return;
            }

            #endregion

            TimerComponent.Instance.NewOnceTimer(self.CreateViewTime + self.Config.Time,
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
            if (self.Config.ColliderShape == SkillColliderShapeType.OBB)
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
                            Para = self.Para,
                            Config = self.SkillConfig,
                            Cost = self.Cost,
                            CostId = self.CostId,
                            Type = e
                        });
                    },  UnitType.ALL);
            }
            else if (self.Config.ColliderShape == SkillColliderShapeType.Sphere)
            {
                skillAOIUnit.AddSphereTrigger(self.Config.ColliderPara[0], AOITriggerType.All,
                    (o, e) =>
                    {
                        EventSystem.Instance.Publish(new EventType.OnSkillTrigger
                        {
                            Skill = skillAOIUnit,
                            From = aoiUnit,
                            To = o,
                            Para = self.Para,
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
        }
    }
}