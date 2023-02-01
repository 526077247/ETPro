using System;

namespace ET
{
    public interface IOnDestroySystem : ISystemType
    {
        void Run(object o);
    }

    [UISystem]
    public abstract class OnDestroySystem<T> : IOnDestroySystem where T :Entity
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IOnDestroySystem>.Type;
        }

        public void Run(object o)
        {
            this.OnDestroy((T)o);
        }

        public abstract void OnDestroy(T self);
    }

}