using System;
using UnityEngine;
namespace ET
{
    [Timer(TimerType.MoveAndSpellSkill)]
    [FriendClass(typeof(SkillAbility))]
    [FriendClass(typeof(MoveAndSpellComponent))]
    public class MoveAndSpellSkill: ATimer<MoveAndSpellComponent>
    {
        public override void Run(MoveAndSpellComponent self)
        {
            try
            {
                if(self==null||self.IsDisposed) return;
                var previewType = self.Skill.SkillConfig.PreviewType;
                //0大圈选一个目标
                if (previewType == SkillPreviewType.SelectTarget)
                {
                    self.SpellWithTarget(self.Skill,self.Target);
                }
                //1大圈选小圈
                else if (previewType == SkillPreviewType.SelectCircularInCircularArea)
                {
                    self.SpellWithPoint(self.Skill,self.Point);
                }
                //2矩形
                else if (previewType == SkillPreviewType.SelectRectangleArea)
                {
                    self.SpellWithDirect(self.Skill,self.Point);
                }
                //自动
                else
                {
                    Log.Error("未处理的施法类型"+previewType);
                }

            }
            catch (System.Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    [ObjectSystem]
    public class MoveAndSpellComponentDestroySystem:DestroySystem<MoveAndSpellComponent>
    {
        public override void Destroy(MoveAndSpellComponent self)
        {
            if (self.Skill != null)
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
            }
        }
    }
    [FriendClass(typeof(MoveAndSpellComponent))]
    public static class MoveAndSpellComponentSystem
    {
        /// <summary>
        /// 释放对目标技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellSkill"></param>
        /// <param name="targetEntity"></param>
        public static void SpellWithTarget(this MoveAndSpellComponent self, SkillAbility spellSkill, CombatUnitComponent targetEntity)
        {
            if(!spellSkill.CanUse())return;
            if (self.Skill != null && (self.Skill!=spellSkill||targetEntity!=self.Target))//换新技能释放了
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
            }
            
            var unit = self.GetParent<CombatUnitComponent>().unit;
            var nowpos = unit.Position;
            var point = targetEntity.unit.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(point.x, point.z)) >
                spellSkill.SkillConfig.PreviewRange[0])
            {
                self.MoveTo(unit, point);
                if (self.Skill == null)
                {
                    self.Skill = spellSkill;
                    self.Target = targetEntity;
                    self.TimerId = TimerComponent.Instance.NewFrameTimer(TimerType.MoveAndSpellSkill, self);
                }

                return;
            }

            if (self.Skill != null)
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
                unit.Stop(0);
            }
#if SERVER //单机去掉
            self.Parent.GetComponent<SpellComponent>().SpellWithTarget(spellSkill,targetEntity);
#else
            spellSkill.UseSkill(Vector3.zero,targetEntity.Id);
#endif
        }
        /// <summary>
        /// 释放对点技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellSkill"></param>
        /// <param name="point"></param>
        public static void SpellWithPoint(this MoveAndSpellComponent self,SkillAbility spellSkill, Vector3 point)
        {
            if(!spellSkill.CanUse())return;
            if (self.Skill != null)//换新技能释放了
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
            }

            var unit = self.GetParent<CombatUnitComponent>().unit;
            var nowpos = unit.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(point.x, point.z)) >
                spellSkill.SkillConfig.PreviewRange[0])
            {
                self.MoveTo(unit, point);
                if (self.Skill == null)
                {
                    self.Skill = spellSkill;
                    self.TimerId = TimerComponent.Instance.NewFrameTimer(TimerType.MoveAndSpellSkill, self);
                }
                return;
            }
            if (self.Skill != null)
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
                unit.Stop(0);
            }
#if SERVER //单机去掉
            self.Parent.GetComponent<SpellComponent>().SpellWithPoint(spellSkill,point);
#else
            spellSkill.UseSkill(point);
#endif
        }
        /// <summary>
        /// 释放方向技能
        /// </summary>
        /// <param name="self"></param>
        /// <param name="spellSkill"></param>
        /// <param name="point"></param>
        public static void SpellWithDirect(this MoveAndSpellComponent self,SkillAbility spellSkill, Vector3 point)
        {
            if(!spellSkill.CanUse())return;
            if (self.Skill != null)//换新技能释放了
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
            }

            var unit = self.GetParent<CombatUnitComponent>().unit;
            var nowpos = unit.Position;
            if (Vector2.Distance(new Vector2(nowpos.x, nowpos.z), new Vector2(point.x, point.z)) >
                spellSkill.SkillConfig.PreviewRange[0])
            {
                self.MoveTo(unit, point);
                if (self.Skill == null)
                {
                    self.Skill = spellSkill;
                    self.TimerId = TimerComponent.Instance.NewFrameTimer(TimerType.MoveAndSpellSkill, self);
                }

                return;
            }
            if (self.Skill != null)
            {
                self.Skill = null;
                self.Target = null;
                TimerComponent.Instance.Remove(ref self.TimerId);
                unit.Stop(0);
            }
#if SERVER //单机去掉
            self.Parent.GetComponent<SpellComponent>().SpellWithPoint(spellSkill,point);
#else
            spellSkill.UseSkill(point);
#endif
        }

        public static void MoveTo(this MoveAndSpellComponent self,Unit unit, Vector3 point)
        {
            if (self.Skill==null||(self.Point-point).sqrMagnitude>self.Skill.SkillConfig.PreviewRange[0]/2f)
            {
                self.Point = point;
#if !SERVER
                unit.MoveToAsync(point).Coroutine();
#else
                unit.FindPathMoveToAsync(point).Coroutine();
#endif
            }
        }

        public static void Cancel(this MoveAndSpellComponent self)
        {
            self.Skill = null;
            self.Target = null;
            TimerComponent.Instance.Remove(ref self.TimerId);
        }
    }
}