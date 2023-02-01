using System;

namespace ET
{
    public interface IOnDisable
    {
    }

    public interface IOnDisable<A>
    {
    }
	
    public interface IOnDisable<A, B>
    {
    }
	
    public interface IOnDisable<A, B, C>
    {
    }
	
    public interface IOnDisable<A, B, C, D>
    {
    }
    public interface IOnDisableSystem : ISystemType
    {
        void Run(object o);
    }

    public interface IOnDisableSystem<A> : ISystemType
    {
        void Run(object o, A a);
    }

    public interface IOnDisableSystem<A, B> : ISystemType
    {
        void Run(object o, A a, B b);
    }

    public interface IOnDisableSystem<A, B, C> : ISystemType
    {
        void Run(object o, A a, B b, C c);
    }

    public interface IOnDisableSystem<A, B, C, D> : ISystemType
    {
        void Run(object o, A a, B b, C c, D d);
    }

    [UISystem]
    public abstract class OnDisableSystem<T> : IOnDisableSystem where T : IOnDisable
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IOnDisableSystem>.Type;
        }

        public void Run(object o)
        {
            this.OnDisable((T)o);
        }

        public abstract void OnDisable(T self);
    }

    [UISystem]
    public abstract class OnDisableSystem<T, A> : IOnDisableSystem<A> where T : IOnDisable<A>
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IOnDisableSystem<A>>.Type;
        }

        public void Run(object o, A a)
        {
            this.OnDisable((T)o, a);
        }

        public abstract void OnDisable(T self, A a);
    }

    [UISystem]
    public abstract class OnDisableSystem<T, A, B> : IOnDisableSystem<A, B> where T :  IOnDisable<A,B>
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IOnDisableSystem<A, B>>.Type;
        }

        public void Run(object o, A a, B b)
        {
            this.OnDisable((T)o, a, b);
        }

        public abstract void OnDisable(T self, A a, B b);
    }

    [UISystem]
    public abstract class OnDisableSystem<T, A, B, C> : IOnDisableSystem<A, B, C> where T :  IOnDisable<A,B,C>
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IOnDisableSystem<A, B, C>>.Type;
        }

        public void Run(object o, A a, B b, C c)
        {
            this.OnDisable((T)o, a, b, c);
        }

        public abstract void OnDisable(T self, A a, B b, C c);
    }

    [UISystem]
    public abstract class OnDisableSystem<T, A, B, C, D> : IOnDisableSystem<A, B, C, D> where T :  IOnDisable<A,B,C, D>
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IOnDisableSystem<A, B, C, D>>.Type;
        }

        public void Run(object o, A a, B b, C c, D d)
        {
            this.OnDisable((T)o, a, b, c, d);
        }

        public abstract void OnDisable(T self, A a, B b, C c, D d);
    }
}