namespace ET
{
    public static class SkillJudgeType
    {
        /// <summary>
        /// 固定位置碰撞体
        /// </summary>
        public const int FixedPosition = 0;
        /// <summary>
        /// 固定方向碰撞体
        /// </summary>
        public const int FixedRotation = 1;
        /// <summary>
        /// 朝指定位置方向飞行碰撞体
        /// </summary>
        public const int Target = 2;
        /// <summary>
        /// 锁定目标飞行
        /// </summary>
        public const int Aim = 3;
        /// <summary>
        /// 立刻结算
        /// </summary>
        public const int Immediate = 4;
    }
}