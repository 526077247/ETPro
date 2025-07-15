﻿#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

internal class TTFSRequestPackageVersionOperation : FSRequestPackageVersionOperation
{
    private enum ESteps
    {
        None,
        RequestPackageVersion,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
    private readonly int _timeout;
    private RequestTiktokPackageVersionOperation _requestPackageVersionOp;
    private ESteps _steps = ESteps.None;


    internal TTFSRequestPackageVersionOperation(TiktokFileSystem fileSystem, int timeout)
    {
        _fileSystem = fileSystem;
        _timeout = timeout;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.RequestPackageVersion;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.RequestPackageVersion)
        {
            if (_requestPackageVersionOp == null)
            {
                _requestPackageVersionOp = new RequestTiktokPackageVersionOperation(_fileSystem, _timeout);
                OperationSystem.StartOperation(_fileSystem.PackageName, _requestPackageVersionOp);
            }

            Progress = _requestPackageVersionOp.Progress;
            if (_requestPackageVersionOp.IsDone == false)
                return;

            if (_requestPackageVersionOp.Status == EOperationStatus.Succeed)
            {
                _steps = ESteps.Done;
                PackageVersion = _requestPackageVersionOp.PackageVersion;
                Status = EOperationStatus.Succeed;
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _requestPackageVersionOp.Error;
            }
        }
    }
}
#endif