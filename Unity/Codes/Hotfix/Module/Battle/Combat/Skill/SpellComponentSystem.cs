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
            self.AddComponent<SkillPara>(true);
        }
    }

    [FriendClass(typeof(SpellComponent))]
    [FriendClass(typeof(SkillAbility))]
    [FriendClass(typeof(CombatUnitComponent))]
    [FriendClass(typeof(SkillPara))]
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
                self.ChangeGroup(self.GetComponent<SkillPara>().SkillConfig.InterruptGroup);
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
                self.GetComponent<SkillPara>().CurGroup = group;
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
            self.CurSkillConfigId = spellSkill.ConfigId;
            if (spellSkill.SkillConfig.PreviewRange != null && spellSkill.SkillConfig.PreviewRange.Length >=1 &&
                spellSkill.SkillConfig.PreviewRange[0] > 0)//不填或者填非正数表示无限距离
            {
                var nowpos = self.GetParent<CombatUnitComponent>().unit.Position;
                var nowpos2 = targetEntity.unit.Position;
                if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(nowpos2.x, nowpos2.z)) >
                    spellSkill.SkillConfig.PreviewRange[0])
                {
                    return;
                }
            }

            self.GetComponent<SkillPara>().Clear();
            self.GetComponent<SkillPara>().FromId = self.Id;
            self.GetComponent<SkillPara>().SkillConfigId = spellSkill.ConfigId;
            self.GetComponent<SkillPara>().ToId = targetEntity.Id;
            self.GetComponent<SkillPara>().CurGroup = spellSkill.SkillConfig.DefaultGroup;

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
            self.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = self.GetParent<CombatUnitComponent>().unit.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(point.x, point.z)) >
                spellSkill.SkillConfig.PreviewRange[0])
            {
                var dir =new Vector3(point.x - nowpos.x,0, point.z - nowpos.z).normalized;
                point = nowpos + dir * spellSkill.SkillConfig.PreviewRange[0];
            }

            self.GetComponent<SkillPara>().Clear();
            self.GetComponent<SkillPara>().Position = point;
            self.GetComponent<SkillPara>().FromId = self.Id;
            self.GetComponent<SkillPara>().SkillConfigId = spellSkill.ConfigId;
            self.GetComponent<SkillPara>().CurGroup = spellSkill.SkillConfig.DefaultGroup;

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
            self.CurSkillConfigId = spellSkill.ConfigId;
            var nowpos = self.GetParent<CombatUnitComponent>().unit.Position;
            point = new Vector3(point.x, nowpos.y, point.z);
            var Rotation = Quaternion.LookRotation(point - nowpos,Vector3.up);
            
            self.GetComponent<SkillPara>().Clear();
            self.GetComponent<SkillPara>().Position = point;
            self.GetComponent<SkillPara>().Rotation = Rotation;
            self.GetComponent<SkillPara>().FromId = self.Id;
            self.GetComponent<SkillPara>().SkillConfigId = spellSkill.ConfigId;
            self.GetComponent<SkillPara>().CurGroup = spellSkill.SkillConfig.DefaultGroup;
            
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
                var stepType = self.GetSkill()?.GetGroup(self.GetComponent<SkillPara>().CurGroup)?.GetStepType();
                if (self.GetComponent<SkillPara>()==null||self.CurSkillConfigId==0||stepType==null||
                    index >=stepType.Count)
                {
                    self.OnSkillPlayOver();
                    return;
                }
                var id = stepType[index];
                self.GetComponent<SkillPara>().SetParaStep(self.GetComponent<SkillPara>().CurGroup,index);
                self.GetComponent<SkillPara>().CurIndex = index;
                SkillWatcherComponent.Instance.Run(id, self.GetComponent<SkillPara>());
                if (self.CheckPause()) return;
                index++;
            } 
            while (self.GetComponent<SkillPara>().GetSkillStepPara(index-1).Interval<=0);
            self.NextSkillStep = index;
            self.TimerId = TimerComponent.Instance.NewOnceTimer(
                TimeHelper.ServerNow() + self.GetComponent<SkillPara>().GetSkillStepPara(index-1).Interval, TimerType.PlayNextSkillStep, self);
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
            var id = self.GetSkill().GetGroup(self.GetComponent<SkillPara>().CurGroup).GetStepType()[index];
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
                if (self.GetComponent<SkillPara>().GetSkillStepPara(index-1).Interval <= 0)
                {
                    self.PlayNextSkillStep(index);
                }
                else
                {
                    self.NextSkillStep = index;
                    self.TimerId = TimerComponent.Instance.NewOnceTimer(
                        TimeHelper.ServerNow() + self.GetComponent<SkillPara>().GetSkillStepPara(index-1).Interval, TimerType.PlayNextSkillStep, self);
                }
                
            }
        }
    }
}