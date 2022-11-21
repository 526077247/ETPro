using UnityEngine;

namespace ET
{
    [FriendClass(typeof(SkillPara))]
    public static class SkillParaSystem
    {
        public class AwakeSystem:DestroySystem<SkillPara>
        {
            public override void Destroy(SkillPara self)
            {
                self.GetParent<SpellComponent>().Interrupt();
                self.Clear();
            }
        }

        public static void Clear(this SkillPara self)
        {
            self.Position= Vector3.zero;
            self.Rotation = Quaternion.identity;
            self.FromId = default;
            self.ToId = default;
            self.CostId.Clear();
            self.Cost.Clear();
            self.SkillConfigId = default;
            self.GroupStepPara.Clear();
        }
        
        public static SkillStepPara SetParaStep(this SkillPara self,string groupId, int index)
        {
            var stepPara = new SkillStepPara();
            stepPara.Index = index;
            stepPara.Paras = null;
            stepPara.Interval = 0;
            var conf = SkillStepConfigCategory.Instance.GetSkillGroup(self.SkillConfigId, groupId);
            var para = SkillStepComponent.Instance.GetSkillStepParas(conf.Id);
            if (para != null && index < para.Count)
            {
                stepPara.Paras = para[index];
            }
            var timeline = SkillStepComponent.Instance.GetSkillStepTimeLine(conf.Id);
            if (timeline != null && index < timeline.Count)
            {
                stepPara.Interval = timeline[index];
            }
            stepPara.Count = 0;
            self.GroupStepPara.Set(groupId,index,stepPara);
            return stepPara;
        }
        
        public static SkillStepPara GetSkillStepPara(this SkillPara self,string group, int index)
        {
            if (self.GroupStepPara.TryGetValue(group, index,out var res))
            {
                return res;
            }
            return self.SetParaStep(group,index);
        }
        public static SkillStepPara GetSkillStepPara(this SkillPara self, int index)
        {
            return self.GetSkillStepPara(self.CurGroup, index);
        }
        public static SkillStepPara GetCurSkillStepPara(this SkillPara self)
        {
            return self.GetSkillStepPara(self.CurGroup, self.CurIndex);
        }
    }
}