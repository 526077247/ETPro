namespace ET
{
    public static class SkillStepType
    {
        /// <summary>
        /// 空
        /// </summary>
        public const int None = -1;
        /// <summary>
        /// 仅等待
        /// </summary>
        public const int Wait = 0;
        /// <summary>
        /// 改变坐标
        /// 参数1:相对坐标字符串"x,y,z"
        /// </summary>
        public const int Move = 1;
        /// <summary>
        /// 播动画
        /// 参数1:动画名
        /// </summary>
        public const int Anim = 2;
        /// <summary>
        /// 播放声音
        /// 参数1:音频路径
        /// </summary>
        public const int Sound = 3;
        /// <summary>
        /// 结算消耗
        /// 参数1:消耗的属性Key字符串,
        /// 参数2:固定消耗值,
        /// 参数3:消耗的计算公式
        /// </summary>
        public const int Cost = 4;
        /// <summary>
        /// 生成召唤物
        /// 参数1:召唤物Id
        /// </summary>
        public const int GenerateObject = 5;
        /// <summary>
        /// 生成碰撞器
        /// 参数1:触发器Id(整数),
        /// 参数2:伤害计算公式Id(整数),
        /// 参数3:最终伤害百分比(0-1小数),
        /// 参数4:最大作用单位数(0表示无限)(整数),
        /// 参数5:"buff1,buff1持续时间单位毫秒,buff1离开范围是否移除buff0或1;buff2,buff2持续时间单位毫秒,buff2离开范围是否移除buff0或1")(字符串，逗号和分号分割),
        /// 参数6:触发器延时生成(整数单位ms,默认1),
        /// </summary>
        public const int GenerateCollider = 6;
        /// <summary>
        /// 添加特效（临时特效用这个，常驻还是走buff）
        /// 参数1:特效路径
        /// </summary>
        public const int AddEffect = 7;
        /// <summary>
        /// 给自己加BUFF
        /// 参数1:buffid,
        /// 参数2时间ms
        /// </summary>
        public const int AddBuff = 9;
        /// <summary>
        /// 移除自己BUFF
        /// 参数1:buffid
        /// </summary>
        public const int RemoveBuff = 10;
        /// <summary>
        /// <para>改变Group</para>>
        /// <para>(1个参数.参数1:Group)</para>>
        /// <para>(2个参数.参数1:Condition 参数2:满足条件Group)</para>>
        /// <para>(3个参数.参数1:Condition 参数2:满足条件Group 参数3:不满足条件Group)</para>>
        /// </summary>
        public const int ChangeGroup = 11;
    }
}