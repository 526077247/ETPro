using System;
namespace ET
{
    public interface IAutoSpell{}
    public interface IAutoSpell<T>{}
    public interface IAutoSpell<T,V>{}
    public interface IAutoSpellSystem : ISystemType
    {
        void AutoSpell(object o);
    }
    public interface IAutoSpellSystem<T> : ISystemType
    {
        void AutoSpell(object o,T v);
    }
    public interface IAutoSpellSystem<T,V> : ISystemType
    {
        void AutoSpell(object o,T t,V v);
    }

    [SelectSystem]
    public abstract class AutoSpellSystem<T> : IAutoSpellSystem where T :IAutoSpell
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IAutoSpellSystem>.Type;
        }

        public void AutoSpell(object o)
        {
            this.OnAutoSpell((T)o);
        }

        public abstract void OnAutoSpell(T self);
    }
    [SelectSystem]
    public abstract class AutoSpellSystem<T,V> : IAutoSpellSystem<V> where T :IAutoSpell<V>
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IAutoSpellSystem<V>>.Type;
        }

        public void AutoSpell(object o,V v)
        { 
            this.OnAutoSpell((T)o,v);
        }

        public abstract void OnAutoSpell(T self,V v);
    }
    
    [SelectSystem]
    public abstract class AutoSpellSystem<T,V,U> : IAutoSpellSystem<V,U> where T :IAutoSpell<V,U>
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IAutoSpellSystem<V,U>>.Type;
        }

        public void AutoSpell(object o,V v,U u)
        {
            this.OnAutoSpell((T)o,v,u);
        }

        public abstract void OnAutoSpell(T self,V v,U u);
    }
}