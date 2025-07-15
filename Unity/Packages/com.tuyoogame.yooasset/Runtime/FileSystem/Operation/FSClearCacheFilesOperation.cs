﻿
namespace YooAsset
{
    internal abstract class FSClearCacheFilesOperation : AsyncOperationBase
    {
    }

    internal sealed class FSClearCacheFilesCompleteOperation : FSClearCacheFilesOperation
    {
        private readonly string _error;

        internal FSClearCacheFilesCompleteOperation()
        {
            _error = null;
        }
        internal FSClearCacheFilesCompleteOperation(string error)
        {
            _error = error;
        }
        internal override void InternalOnStart()
        {
            if (string.IsNullOrEmpty(_error))
            {
                Status = EOperationStatus.Succeed;
            }
            else
            {
                Status = EOperationStatus.Failed;
                Error = _error;
            }
        }
        internal override void InternalOnUpdate()
        {
        }
    }
}