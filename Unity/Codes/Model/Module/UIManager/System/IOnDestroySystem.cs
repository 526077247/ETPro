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
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IOnDestroySystem);
        }

        public void Run(object o)
        {
            this.OnDestroy((T)o);
        }

        public abstract void OnDestroy(T self);
    }

}