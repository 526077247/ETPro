using System;
using System.Collections.Generic;
namespace ET
{
    public interface IInput
    {
        
    }
    public interface IInputSystem: ISystemType
    {
        void Run(object o,int key,int type, ref bool stop);
    }
    [ObjectSystem]
    public abstract class InputSystem<T> : IInputSystem where T :IInput
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IInputSystem);
        }

        public void Run(object o,int key,int type, ref bool stop)
        {
            this.Run((T)o,key,type,ref stop);
        }

        public abstract void Run(T self,int key,int type, ref bool stop);
    }

    
    public interface IInputGroupSystem: ISystemType
    {
        void Run(object o,List<int>key,List<int> type, ref bool stop);
    }
    [ObjectSystem]
    public abstract class InputGroupSystem<T> : IInputGroupSystem where T :IInput
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IInputGroupSystem);
        }

        public void Run(object o,List<int> key,List<int> type, ref bool stop)
        {
            this.Run((T)o,key,type,ref stop);
        }

        public abstract void Run(T self,List<int> key,List<int> type, ref bool stop);
    }
}