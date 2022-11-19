using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;
namespace ET
{
    [Timer(TimerType.PlayNextSkillStep)]
    [FriendClass(typeof(SpellComponent))]
    public class PlayNextSkillStep: ATimer<SpellComponent>
    {
        public override void Run(SpellComponent self)
        {
            try
            {
                self.PlayNextSkillStep(self.NextSkillStep);
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    [ObjectSystem]
    public class SpellComponentAwakeSystem : AwakeSystem<SpellComponent>
    {
        public override void Awake(SpellComponent self)
        {
            self.CurSkillConfigId = 0;
            self.Para = SkillPara.Create();
        }
    }
    [ObjectSystem]
    public class SpellComponentDestroySystem : DestroySystem<SpellComponent>
    {
        public override void Destroy(SpellComponent self)
        {
            self.Interrupt();
            self.Para.Dispose();
            self.Para = null;
        }
    }
    [FriendClass(typeof(SpellComponent))]
    [FriendClass(typeof(SkillAbility))]
    [FriendClass(typeof(CombatUnitComponent))]
    public static class SpellComponentSystem
    {
        /// <summary>
        /// 当前技能
        /// </summary>
        public static SkillAbility GetSkill(this SpellComponent self)
        {
            if (self.GetParent<CombatUnitComponent>().TryGetSkillAbility(self.CurSkillConfigId, out var res))
            {
                return res;
            }
            return null;
        } 
        /// <summary>
        /// 设置是否可施法
        /// </summary>
        /// <param name="self"></param>
        /// <param name="enable"></param>
        public static void SetEnable(this SpellComponent self, bool enable)
        {
            self.Enable = enable;
            if(!enable)
                self.Interrupt();
        }
        /// <summary>
        /// 打断
        /// </summary>
        /// <param name="self"></param>
        public static void Interrupt(this SpellComponent self)
        {
            if (self.CanInterrupt())
            {
#if SERVER
                var unit = self.Parent.GetParent<Unit>();
                if(unit.IsGhost()) return;
                M2C_Interrupt msg = new M2C_Interrupt { UnitId = self.Id, ConfigId = self.CurSkillConfigId, Timestamp = TimeHelper.ServerNow() };
                MessageHelper.Broadcast(unit, msg);
#endif
                self.ChangeGroup(self.Para.Ability.SkillConfig.InterruptGroup);
            }
        }

        /// <summary>
        /// 是否可打断
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool CanInterrupt(this SpellComponent self)
        {
            return self.CurSkillConfigId != 0&&self.TimerId!=0;
        }
        /// <summary>
        /// 改变技能释放组
        /// </summary>
        /// <param name="self"></param>
        /// <param name="group"></param>
        public static void ChangeGroup(this SpellComponent self,string group)
        {
            if (self.CurSkillConfigId != 0)
            {
                self.Para.Ability.CurGroupId = group;
                if (self.TimerId != 0)//在等待中
                {
                    TimerComponent.Instance.Remove(ref self.TimerId);
                    self.PlayNextSkillStep(0);
                }
                else//在PlayNextSkillStep的循环中
                {
                    self.NextSkillStep = -1;
                }
            }
        }
        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="self"></param>
        private static void OnSkillPlayOver(this SpellComponent self)
        {
            var skill = self.GetSkill();
            if (skill!=null)
            {
                skill.CurGroupId = null;
                skill.LastSpellOverTime = TimeHelper.ServerNow();
            }
            self.CurSkillConfigId = 0;
        }
        /// <summary>
        /// 释放对目标技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellSkill"></param>
        /// <param name="targetEntity"></param>
        public static void SpellWithTarget(this SpellComponent self, SkillAbility spellSkill, CombatUnitComponent targetEntity)
        {
            if (!self.Enable) return;
            if (self.CurSkillConfigId != 0)
                return;
            if(!spellSkill.CanUse())return;
            spellSkill.CurGroupId = spellSkill.SkillConfig.DefaultGroup;
            self.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = self.GetParent<CombatUnitComponent>().unit.Position;
            var nowpos2 = targetEntity.unit.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(nowpos2.x, nowpos2.z)) >
                spellSkill.SkillConfig.PreviewRange[0])
            {
                return;
            }
            self.Para.Clear();
            self.Para.From = self.GetParent<CombatUnitComponent>();
            self.Para.Ability = spellSkill;
            self.Para.To = targetEntity;

            self.GetSkill().LastSpellTime = TimeHelper.ServerNow();
            self.PlayNextSkillStep(0);
        }
        /// <summary>
        /// 释放对点技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellSkill"></param>
        /// <param name="point"></param>
        public static void SpellWithPoint(this SpellComponent self,SkillAbility spellSkill, Vector3 point)
        {
            if (!self.Enable) return;
            if (self.CurSkillConfigId != 0)
                return;
            if(!spellSkill.CanUse())return;
            spellSkill.CurGroupId = spellSkill.SkillConfig.DefaultGroup;
            self.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = self.GetParent<CombatUnitComponent>().unit.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(point.x, point.z)) >
                spellSkill.SkillConfig.PreviewRange[0])
            {
                var dir =new Vector3(point.x - nowpos.x,0, point.z - nowpos.z).normalized;
                point = nowpos + dir * spellSkill.SkillConfig.PreviewRange[0];
            }

            self.Para.Clear();
            self.Para.Position = point;
            self.Para.From = self.GetParent<CombatUnitComponent>();
            self.Para.Ability = spellSkill;

            self.GetSkill().LastSpellTime = TimeHelper.ServerNow();
            self.PlayNextSkillStep(0);
        }
        /// <summary>
        /// 释放方向技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellSkill"></param>
        /// <param name="point"></param>
        public static void SpellWithDirect(this SpellComponent self,SkillAbility spellSkill, Vector3 point)
        {
            if (!self.Enable) return;
            if (self.CurSkillConfigId != 0)
                return;
            if(!spellSkill.CanUse())return;
            spellSkill.CurGroupId = spellSkill.SkillConfig.DefaultGroup;
            self.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = self.GetParent<CombatUnitComponent>().unit.Position;
            point = new Vector3(point.x, nowpos.y, point.z);
            var Rotation = Quaternion.LookRotation(point - nowpos,Vector3.up);
            
            self.Para.Clear();
            self.Para.Position = point;
            self.Para.Rotation = Rotation;
            self.Para.From = self.GetParent<CombatUnitComponent>();
            self.Para.Ability = spellSkill;

            self.GetSkill().LastSpellTime = TimeHelper.ServerNow();
            self.PlayNextSkillStep(0);
        }
        /// <summary>
        /// 触发下一个技能触发点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="index"></param>
        public static void PlayNextSkillStep(this SpellComponent self,int index)
        {
            do
            {
                if (self.Para==null||self.CurSkillConfigId==0||self.GetSkill()?.GetCurGroup()?.GetStepType()==null||index >=self.GetSkill().GetCurGroup().GetStepType().Count)
                {
                    self.OnSkillPlayOver();
                    return;
                }
                var id = self.GetSkill().GetCurGroup().GetStepType()[index];
                self.SetParaStep(index);
                SkillWatcherComponent.Instance.Run(id, self.Para);
                if (self.CheckPause()) return;
                index++;
            } 
            while (self.Para.StepPara[index-1].Interval<=0);
            self.NextSkillStep = index;
            self.TimerId = TimerComponent.Instance.NewOnceTimer(
                TimeHelper.ServerNow() + self.Para.StepPara[index-1].Interval, TimerType.PlayNextSkillStep, self);
        }
        
        static void SetParaStep(this SpellComponent self, int index)
        {
            var group = self.GetSkill().GetCurGroup();
            if(group==null) return;
            
            var stepPara = new SkillStepPara();
            stepPara.Index = index;
            stepPara.Paras = null;
            stepPara.Interval = 0;
            if (group.GetParas() != null && index < group.GetParas().Count)
            {
                stepPara.Paras = group.GetParas()[index];
            }
            if (group.GetTimeLine() != null && index < group.GetTimeLine().Count)
            {
                stepPara.Interval = group.GetTimeLine()[index];
            }
            stepPara.Count = 0;
            
            self.Para.CurIndex = index;
            self.Para.GroupStepPara.Add(self.Para.Ability.CurGroupId,stepPara);
        }


        /// <summary>
        /// 步骤是否暂停等待中
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        static bool CheckPause(this SpellComponent self)
        {
            return self.WaitStep != SkillStepType.None;
        }
        
        
        /// <summary>
        /// 等待步骤
        /// </summary>
        /// <param name="self"></param>
        /// <param name="stepType"></param>
        public static void WaitStep(this SpellComponent self,int stepType)
        {
            if(self.CurSkillConfigId==0||self.WaitStep == stepType) return;
            var index = self.NextSkillStep;
            var id = self.GetSkill().GetCurGroup().GetStepType()[index];
            if (stepType == id)
            {
                self.WaitStep = stepType;
            }
        }
        /// <summary>
        /// 等待步骤结束
        /// </summary>
        /// <param name="self"></param>
        /// <param name="stepType"></param>
        public static void WaitStepOver(this SpellComponent self,int stepType)
        {
            if (self.WaitStep == stepType)
            {
                self.WaitStep = SkillStepType.None;
                var index = self.NextSkillStep+1;
                if (self.Para.StepPara[index - 1].Interval <= 0)
                {
                    self.PlayNextSkillStep(index);
                }
                else
                {
                    self.NextSkillStep = index;
                    self.TimerId = TimerComponent.Instance.NewOnceTimer(
                        TimeHelper.ServerNow() + self.Para.StepPara[index-1].Interval, TimerType.PlayNextSkillStep, self);
                }
                
            }
        }
    }
}