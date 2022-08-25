using System;
namespace ET
{
    public interface IOnWidthPaddingChange
    {
    }
    
    public interface IOnWidthPaddingChangeSystem:ISystemType
    {
        void Run(object o);
    }
    
    /// <summary>
    /// 当窗口gameObject实例化之前
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [UISystem]
    public abstract class OnWidthPaddingChangeSystem<T> : IOnWidthPaddingChangeSystem where T :IOnWidthPaddingChange
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IOnWidthPaddingChangeSystem);
        }

        public void Run(object o)
        {
            this.OnWidthPaddingChange((T)o);
        }

        public abstract void OnWidthPaddingChange(T self);
    }
}