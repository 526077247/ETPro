using System;
using System.Collections.Generic;

namespace ET
{
    [ObjectSystem]
    public class SkillStepComponentAwakeSystem : AwakeSystem<SkillStepComponent>
    {
        public override void Awake(SkillStepComponent self)
        {
            SkillStepComponent.Instance = self;
            self.Params = DictionaryComponent<int, List<object[]>>.Create();
            self.StepType = DictionaryComponent<int, List<int>>.Create();
            self.TimeLine = DictionaryComponent<int, List<int>>.Create();
        }
    }
    [ObjectSystem]
    public class SkillStepComponentDestroySystem : DestroySystem<SkillStepComponent>
    {
        public override void Destroy(SkillStepComponent self)
        {
            SkillStepComponent.Instance = null;
            self.Params.Dispose();
            self.StepType.Dispose();
            self.TimeLine.Dispose();
        }
    }

    [FriendClass(typeof(SkillStepComponent))]
    [FriendClass(typeof(SkillAbility))]
    public static class SkillStepComponentSystem
    {
        public static void GetSkillStepInfo(this SkillStepComponent self,int configId, out List<int> timeline,
            out List<int> steptype, out List<object[]> paras)
        {
           
            bool needinit = false;
            if (!self.TimeLine.ContainsKey(configId))
            {
                needinit = true;
                self.TimeLine[configId] = new List<int>();
            }
            timeline = self.TimeLine[configId];
            
            if (!self.StepType.ContainsKey(configId))
            {
                needinit = true;
                self.StepType[configId] = new List<int>();
            }
            steptype = self.StepType[configId];
            
            if (!self.Params.ContainsKey(configId))
            {
                needinit = true;
                self.Params[configId] = new List<object[]>();
            }
            paras = self.Params[configId];
            
            if (needinit)
            {
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object timelineItem = null;
                    object steptypeItem = null;
                    object stepParameterItem = null;
                    try
                    {
                        var stepParameter = type.GetProperty("StepParameter" + i);
                        var stepStyle = type.GetProperty("StepStyle" + i);
                        var triggerTime = type.GetProperty("TriggerTime" + i);
                        timelineItem = triggerTime.GetValue(config);
                        if(timelineItem!=null)
                            timeline.Add((int)timelineItem);
                        else
                            timeline.Add(0);
                        
                        steptypeItem = stepStyle.GetValue(config);
                        if(steptypeItem!=null)
                            steptype.Add((int)steptypeItem);
                        else
                            steptype.Add(0);
                        stepParameterItem = stepParameter.GetValue(config);
                        if (stepParameterItem != null)
                        {
                            var list = (string[]) stepParameterItem;
                            object[] temp = new object[list.Length];
                            for (int j = 0; j < temp.Length; j++)
                            {
                                temp[j] = list[j];
                            }
                            paras.Add(temp);
                        }
                        else
                            paras.Add(new object[0]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(configId+" Load Fail! at "+i+" values:"+timelineItem+" "+steptypeItem+" "+stepParameterItem+" "+"\r\n"+ex);
                    }
                }
            }
        }
    }
}