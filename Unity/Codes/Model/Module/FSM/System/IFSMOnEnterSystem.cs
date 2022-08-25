using System;

namespace ET
{
    public interface IFSMOnEnterSystem : ISystemType
    {
        ETTask Run(object o);
    }

    public interface IFSMOnEnterSystem<A> : ISystemType
    {
        ETTask Run(object o, A a);
    }

    public interface IFSMOnEnterSystem<A, B> : ISystemType
    {
        ETTask Run(object o, A a, B b);
    }

    public interface IFSMOnEnterSystem<A, B, C> : ISystemType
    {
        ETTask Run(object o, A a, B b, C c);
    }

    public interface IFSMOnEnterSystem<A, B, C, D> : ISystemType
    {
        ETTask Run(object o, A a, B b, C c, D d);
    }

    [FSMSystem]
    public abstract class FSMOnEnterSystem<T> : IFSMOnEnterSystem where T : Entity
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IFSMOnEnterSystem);
        }

        public async ETTask Run(object o)
        {
            await this.FSMOnEnter((T)o);
        }

        public abstract ETTask FSMOnEnter(T self);
    }

    [FSMSystem]
    public abstract class FSMOnEnterSystem<T, A> : IFSMOnEnterSystem<A> where T : Entity
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IFSMOnEnterSystem<A>);
        }

        public async ETTask Run(object o, A a)
        {
            await this.FSMOnEnter((T)o, a);
        }

        public abstract ETTask FSMOnEnter(T self, A a);
    }

    [FSMSystem]
    public abstract class FSMOnEnterSystem<T, A, B> : IFSMOnEnterSystem<A, B> where T : Entity
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IFSMOnEnterSystem<A, B>);
        }

        public async ETTask Run(object o, A a, B b)
        {
            await this.FSMOnEnter((T)o, a, b);
        }

        public abstract ETTask FSMOnEnter(T self, A a, B b);
    }

    [FSMSystem]
    public abstract class FSMOnEnterSystem<T, A, B, C> : IFSMOnEnterSystem<A, B, C> where T : Entity
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IFSMOnEnterSystem<A, B, C>);
        }

        public async ETTask Run(object o, A a, B b, C c)
        {
            await this.FSMOnEnter((T)o, a, b, c);
        }

        public abstract ETTask FSMOnEnter(T self, A a, B b, C c);
    }

    [FSMSystem]
    public abstract class FSMOnEnterSystem<T, A, B, C, D> : IFSMOnEnterSystem<A, B, C, D> where T : Entity
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IFSMOnEnterSystem<A, B, C, D>);
        }

        public async ETTask Run(object o, A a, B b, C c, D d)
        {
            await this.FSMOnEnter((T)o, a, b, c, d);
        }

        public abstract ETTask FSMOnEnter(T self, A a, B b, C c, D d);
    }
}