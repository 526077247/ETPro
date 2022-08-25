namespace ET
{
    public static partial class TimerType
    {
        // 框架层0-1000，逻辑层的timer type 1000-9999
        public const int MoveTimer = 1001;
        public const int AITimer = 1002;
        public const int SessionAcceptTimeout = 1003;
        public const int SkillColliderRemove = 1004;//销毁技能判定体
        public const int PlayNextSkillStep = 1005;//技能步骤
        public const int RemoveBuff = 1006;//移除Buff
        public const int MoveAndSpellSkill = 1007;//从施法范围外移动到最远施法位置施法
        public const int GenerateSkillCollider = 1008;//延时生成触发器
        public const int DestroyGameObject = 1009;//移除GameObject
        // 不能超过10000
    }
}