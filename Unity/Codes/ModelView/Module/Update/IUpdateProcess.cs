using System;

namespace ET
{
    public interface IUpdateProcess
    {
        ETTask<UpdateRes> Process(UpdateTask task);
    }
}