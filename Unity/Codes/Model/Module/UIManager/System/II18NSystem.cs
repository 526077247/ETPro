using System;

namespace ET
{
    public interface II18N
    {
        
    }
    public interface II18NSystem : ISystemType
    {
        void Run(object o);
    }

    
    [UISystem]
    public abstract class I18NSystem<T> : II18NSystem where T :II18N
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<II18NSystem>.Type;
        }

        public void Run(object o)
        {
            this.OnLanguageChange((T)o);
        }

        public abstract void OnLanguageChange(T self);
    }

   
}