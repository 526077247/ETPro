﻿namespace ET
{
    public interface ILog
#if !NOT_UNITY
            : YooAsset.ILogger
#endif
    {
        void Trace(string message);
        void Warning(string message);
        void Info(string message);
        void Debug(string message);
        void Error(string message);
        void Trace(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Info(string message, params object[] args);
        void Debug(string message, params object[] args);
        void Error(string message, params object[] args);
    }
}