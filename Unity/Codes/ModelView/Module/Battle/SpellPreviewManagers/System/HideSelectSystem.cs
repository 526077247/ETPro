using System;
namespace ET
{
    public interface IHide{}
    public interface IHideSelectSystem : ISystemType
    {
        void Hide(object o);
    }

    [SelectSystem]
    public abstract class HideSelectSystem<T> : IHideSelectSystem where T :IHide
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IHideSelectSystem);
        }

        public void Hide(object o)
        {
            this.OnHide((T)o);
        }

        public abstract void OnHide(T self);
    }
}