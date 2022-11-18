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
        public static List<int> GetSkillStepTimeLine(this SkillStepComponent self,int configId)
        {
            if (!self.TimeLine.ContainsKey(configId))
            {
                List<int> timeline = self.TimeLine[configId] = new List<int>();
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object timelineItem = null;
                    try
                    {
                        var triggerTime = type.GetProperty("TriggerTime" + i);
                        timelineItem = triggerTime.GetValue(config);
                        if(timelineItem!=null)
                            timeline.Add((int)timelineItem);
                        else
                            timeline.Add(0);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(configId+" Load Fail! at "+i+" values:"+timelineItem+"\r\n"+ex);
                    }
                }
                return timeline;
            }
            else
            {
                return self.TimeLine[configId];
            }
        }
        
        public static List<int> GetSkillStepType(this SkillStepComponent self,int configId)
        {
            if (!self.StepType.ContainsKey(configId))
            {
                var steptype = self.StepType[configId] = new List<int>();
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object steptypeItem = null;
                    try
                    {
                        var stepStyle = type.GetProperty("StepStyle" + i);
                        steptypeItem = stepStyle.GetValue(config);
                        if(steptypeItem!=null)
                            steptype.Add((int)steptypeItem);
                        else
                            steptype.Add(0);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(configId+" Load Fail! at "+i+" values:"+" "+steptypeItem+"\r\n"+ex);
                    }
                }

                return steptype;
            }
            else
            {
                return self.StepType[configId];
            }
        }
        
        public static List<object[]> GetSkillStepParas(this SkillStepComponent self,int configId)
        {
            if (!self.Params.ContainsKey(configId))
            {
                var paras = self.Params[configId] = new List<object[]>();
                SkillStepConfig config = SkillStepConfigCategory.Instance.Get(configId);
                var type = config.GetType();
                for (int i = 0; i < config.ParaCount; i++)
                {
                    object stepParameterItem = null;
                    try
                    {
                        var stepParameter = type.GetProperty("StepParameter" + i);
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
                        Log.Error(configId+" Load Fail! at "+i+" values:"+stepParameterItem+"\r\n"+ex);
                    }
                }

                return paras;
            }
            else
            {
                return self.Params[configId];
            }
            
        }
    }
}