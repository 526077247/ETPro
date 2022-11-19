using System;
using UnityEngine;
using System.Collections.Generic;
namespace ET
{
    public class SkillStepPara
    {
        public int Index;
        public object[] Paras;
        public int Interval;
        public int Count;//作用单位数
    }
    /// <summary>
    /// 其他地方不要持有SkillPara的引用！！
    /// </summary>
    public class SkillPara:IDisposable
    {

        public Vector3 Position;
        public Quaternion Rotation;
        public CombatUnitComponent From;
        public CombatUnitComponent To;
        public List<int> CostId = new List<int>();
        public List<int> Cost  = new List<int>();
        public SkillAbility Ability;
        public int CurIndex;
        #region 步骤参数

        public MultiMap<string,SkillStepPara> GroupStepPara = new MultiMap<string,SkillStepPara>();
        public List<SkillStepPara> StepPara => GroupStepPara[Ability.CurGroupId];

        #endregion

        public static SkillPara Create()
        {
            return MonoPool.Instance.Fetch(typeof (SkillPara)) as SkillPara;
        }
        
        public void Clear()
        {
            Position=Vector3.zero;
            Rotation = Quaternion.identity;
            From = null;
            To = null;
            CostId.Clear();
            Cost.Clear();
            Ability = null;
            GroupStepPara.Clear();
        }
        public void Dispose()
        {
            Clear();
            MonoPool.Instance.Recycle(this);
        }

    }
}