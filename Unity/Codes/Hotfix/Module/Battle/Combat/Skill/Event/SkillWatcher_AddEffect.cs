#if !NOT_UNITY
namespace ET
{
    /// <summary>
    /// 添加特效(aoi创建Unit之前放的技能没添加的就算了，临时特效走这里，常驻还是走buff)
    /// </summary>
    [SkillWatcher(SkillStepType.AddEffect)]
    public class SkillWatcher_AddEffect : ISkillWatcher
    {
        public void Run(SkillPara para)
        {
            var unit = para.From.unit;
            int curIndex = para.CurIndex;
            var stepPara = para.StepPara[curIndex];
            Log.Info("SkillWatcher_AddEffect");
            if (int.TryParse(stepPara.Paras[0].ToString(), out var effectId))
            {
                EventSystem.Instance.Publish(new EventType.AddEffect { EffectId = effectId, Parent = unit, Unit = unit });
            }
        }
        
    }
}
#endif