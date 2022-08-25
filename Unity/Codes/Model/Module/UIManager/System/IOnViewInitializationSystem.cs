using System;

namespace ET
{
    /// <summary>
    /// 当窗口gameObject实例化之前
    /// </summary>
    public interface IOnViewInitializationSystem : ISystemType
    {
        ETTask Run(object o);
    }

    /// <summary>
    /// 当窗口gameObject实例化之前
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [UISystem]
    public abstract class OnViewInitializationSystem<T> : IOnViewInitializationSystem where T :Entity
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IOnViewInitializationSystem);
        }

        public async ETTask Run(object o)
        {
            await this.OnViewInitialization((T)o);
        }

        public abstract ETTask OnViewInitialization(T self);
    }

}