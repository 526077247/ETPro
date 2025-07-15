﻿using System;
using System.Threading;

namespace YooAsset
{
    /// <summary>
    /// 下载文件验证（线程版）
    /// </summary>
    internal sealed class VerifyTempFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            VerifyFile,
            Waiting,
            Done,
        }

        private readonly TempFileElement _element;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 验证结果
        /// </summary>
        public EFileVerifyResult VerifyResult { private set; get; }


        internal VerifyTempFileOperation(TempFileElement element)
        {
            _element = element;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.VerifyFile;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.VerifyFile)
            {
                bool succeed = ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), _element);
                if (succeed)
                {
                    _steps = ESteps.Waiting;
                }
            }

            if (_steps == ESteps.Waiting)
            {
                int result = _element.Result;
                if (result == 0)
                    return;

                VerifyResult = (EFileVerifyResult)result;
                if (VerifyResult == EFileVerifyResult.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to verify file : {_element.TempFilePath} ! ErrorCode : {VerifyResult}";
                }
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                //TODO 等待子线程验证文件完毕，该操作会挂起主线程
                InternalOnUpdate();
                if (IsDone)
                    break;
            }
        }

        private void VerifyInThread(object obj)
        {
            TempFileElement element = (TempFileElement)obj;
            int result = (int)FileVerifyHelper.FileVerify(element.TempFilePath, element.TempFileSize, element.TempFileCRC, EFileVerifyLevel.High);
            element.Result = result;
        }
    }
}