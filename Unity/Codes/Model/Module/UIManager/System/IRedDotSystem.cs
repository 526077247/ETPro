using System;

namespace ET
{
    public interface IRedDotSystem : ISystemType
    {
        void Run(object o,int count);
    }

    
    [UISystem]
    public abstract class RedDotSystem<T> : IRedDotSystem where T :Entity
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IRedDotSystem>.Type;
        }

        public void Run(object o,int count)
        {
            this.OnRefreshCount((T)o,count);
        }

        public abstract void OnRefreshCount(T self,int count);
    }

   
}