namespace ET
{
    public static class BuffSubType
    {
        /// <summary>
        /// 属性修饰
        /// </summary>
        public const int Attribute = 1 ;
        /// <summary>
        /// 行为禁制
        /// </summary>
        public const int ActionControl = 2;
        /// <summary>
        /// 持续掉血
        /// </summary>
        public const int Bleed = 3;
        /// <summary>
        /// 技能吟唱（根据配置，可判断受击、移动打断自身施法）
        /// </summary>
        public const int Chant = 4;
    }
}