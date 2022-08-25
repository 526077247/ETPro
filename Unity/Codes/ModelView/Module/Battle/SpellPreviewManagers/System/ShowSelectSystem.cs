using System;
namespace ET
{
    public interface IShow{}
    public interface IShow<T>{}
    public interface IShow<T,V>{}
    public interface IShowSelectSystem : ISystemType
    {
        ETTask Show(object o);
    }
    public interface IShowSelectSystem<T> : ISystemType
    {
        ETTask Show(object o,T v);
    }
    public interface IShowSelectSystem<T,V> : ISystemType
    {
        ETTask Show(object o,T t,V v);
    }

    [SelectSystem]
    public abstract class ShowSelectSystem<T> : IShowSelectSystem where T :IShow
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IShowSelectSystem);
        }

        public ETTask Show(object o)
        {
            return this.OnShow((T)o);
        }

        public abstract ETTask OnShow(T self);
    }
    [SelectSystem]
    public abstract class ShowSelectSystem<T,V> : IShowSelectSystem<V> where T :IShow<V>
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IShowSelectSystem<V>);
        }

        public ETTask Show(object o,V v)
        {
            return this.OnShow((T)o,v);
        }

        public abstract ETTask OnShow(T self,V v);
    }
    
    [SelectSystem]
    public abstract class ShowSelectSystem<T,V,U> : IShowSelectSystem<V,U> where T :IShow<V,U>
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IShowSelectSystem<V,U>);
        }

        public ETTask Show(object o,V v,U u)
        {
            return this.OnShow((T)o,v,u);
        }

        public abstract ETTask OnShow(T self,V v,U u);
    }
}