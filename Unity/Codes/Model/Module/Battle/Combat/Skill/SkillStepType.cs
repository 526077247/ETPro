namespace ET
{
    public static class SkillStepType
    {
        /// <summary>
        /// 仅等待
        /// </summary>
        public const int Wait = 0;
        /// <summary>
        /// 改变坐标
        /// </summary>
        public const int Move = 1;
        /// <summary>
        /// 播动画
        /// </summary>
        public const int Anim = 2;
        /// <summary>
        /// 播放声音
        /// </summary>
        public const int Sound = 3;
        /// <summary>
        /// 结算消耗
        /// </summary>
        public const int Cost = 4;
        /// <summary>
        /// 生成召唤物
        /// </summary>
        public const int GenerateObject = 5;
        /// <summary>
        /// 生成碰撞器
        /// </summary>
        public const int GenerateCollider = 6;
        /// <summary>
        /// 添加特效
        /// </summary>
        public const int AddEffect = 7;
    }
}