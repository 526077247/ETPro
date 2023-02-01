using System;

namespace ET
{
    public interface IFSMOnExitSystem : ISystemType
    {
        ETTask Run(object o);
    }

    [FSMSystem]
    public abstract class FSMOnExitSystem<T> : IFSMOnExitSystem where T : Entity
    {
        public Type Type()
        {
            return TypeInfo<T>.Type;
        }

        public Type SystemType()
        {
            return TypeInfo<IFSMOnExitSystem>.Type;
        }

        public async ETTask Run(object o)
        {
            await this.FSMOnExit((T)o);
        }

        public abstract ETTask FSMOnExit(T self);
    }

}