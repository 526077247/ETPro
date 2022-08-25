using System;

namespace ET
{
    [ObjectSystem]
    public class BuffBleedComponentAwakeSystem: AwakeSystem<BuffBleedComponent,int>
    {
        public override void Awake(BuffBleedComponent self, int a)
        {
            self.ConfigId = a;
#if SERVER
            self.LastBleedTime = TimeHelper.ServerNow();
            self.HandleBleed();
#endif
        }
    }
#if SERVER
    [ObjectSystem]
    public class BuffBleedComponentDestroySystem: DestroySystem<BuffBleedComponent>
    {
        public override void Destroy(BuffBleedComponent self)
        {
            self.TryBleed();
        }
    }

    [ObjectSystem]
    public class BuffBleedComponentUpdateSystem: UpdateSystem<BuffBleedComponent>
    {
        public override void Update(BuffBleedComponent self)
        {
            self.TryBleed();
        }
    }
#endif
    [FriendClass(typeof(BuffBleedComponent))]
    [FriendClass(typeof(Buff))]
    public static class BuffBleedComponentSystem
    {
#if SERVER
        public static void TryBleed(this BuffBleedComponent self)
        {
            var timeNow = TimeHelper.ServerNow();
            var deltaTime = timeNow - self.LastBleedTime;
            while (deltaTime >= self.Config.CD)
            {
                deltaTime -= self.Config.CD;
                self.LastBleedTime += self.Config.CD;
                self.HandleBleed();
            }
        }
        
        public static void HandleBleed(this BuffBleedComponent self)
        {
            var buff = self.GetParent<Buff>();
            var target = self.Parent.GetParent<BuffComponent>().unit;
            var from = target.Parent.GetChild<Unit>(buff.FromUnitId);
            if (from != null)
            {
                FormulaConfig formula = FormulaConfigCategory.Instance.Get(self.Config.FormulaId);
                if (formula!=null)
                {
                    FormulaStringFx fx = FormulaStringFx.GetInstance(formula.Formula);
                    NumericComponent f = from.GetComponent<NumericComponent>();
                    NumericComponent t = target.GetComponent<NumericComponent>();
                    float value = fx.GetData(f, t);
                    BattleHelper.Damage(from.GetComponent<CombatUnitComponent>(),target.GetComponent<CombatUnitComponent>(),value);
                }
            }
        }
#endif
    }
}