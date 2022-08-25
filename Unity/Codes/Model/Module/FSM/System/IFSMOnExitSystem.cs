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
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IFSMOnExitSystem);
        }

        public async ETTask Run(object o)
        {
            await this.FSMOnExit((T)o);
        }

        public abstract ETTask FSMOnExit(T self);
    }

}