﻿namespace ET
{
    public interface IStaticAction
    {
        public void Run();
    }
    
    public interface IStaticFunc<T>
    {
        public T Run();
    }
}