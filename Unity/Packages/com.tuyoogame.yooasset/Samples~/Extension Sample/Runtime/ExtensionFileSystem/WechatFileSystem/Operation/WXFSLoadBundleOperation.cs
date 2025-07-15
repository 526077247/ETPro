﻿#if UNITY_WEBGL && WEIXINMINIGAME
using YooAsset;

internal class WXFSLoadBundleOperation : FSLoadBundleOperation
{
    private enum ESteps
    {
        None,
        DownloadAssetBundle,
        Done,
    }

    private readonly WechatFileSystem _fileSystem;
    private readonly PackageBundle _bundle;
    private DownloadAssetBundleOperation _downloadAssetBundleOp;
    private ESteps _steps = ESteps.None;

    internal WXFSLoadBundleOperation(WechatFileSystem fileSystem, PackageBundle bundle)
    {
        _fileSystem = fileSystem;
        _bundle = bundle;
    }
    internal override void InternalOnStart()
    {
        _steps = ESteps.DownloadAssetBundle;
    }
    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.DownloadAssetBundle)
        {
            if (_downloadAssetBundleOp == null)
            {
                DownloadParam downloadParam = new DownloadParam(int.MaxValue, 60);
                downloadParam.MainURL = _fileSystem.RemoteServices.GetRemoteMainURL(_bundle.FileName); ;
                downloadParam.FallbackURL = _fileSystem.RemoteServices.GetRemoteFallbackURL(_bundle.FileName);

                if (_bundle.Encrypted)
                {
                    _downloadAssetBundleOp = new DownloadWebEncryptAssetBundleOperation(false, _fileSystem.DecryptionServices, _bundle, downloadParam);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _downloadAssetBundleOp);
                }
                else
                {
                    _downloadAssetBundleOp = new DownloadWechatAssetBundleOperation(_bundle, downloadParam);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _downloadAssetBundleOp);
                }
            }

            DownloadProgress = _downloadAssetBundleOp.DownloadProgress;
            DownloadedBytes = (long)_downloadAssetBundleOp.DownloadedBytes;
            Progress = DownloadProgress;
            if (_downloadAssetBundleOp.IsDone == false)
                return;

            if (_downloadAssetBundleOp.Status == EOperationStatus.Succeed)
            {
                var assetBundle = _downloadAssetBundleOp.Result;
                if (assetBundle == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(DownloadAssetBundleOperation)} loaded asset bundle is null !";
                }
                else
                {
                    _steps = ESteps.Done;
                    Result = new WXAssetBundleResult(_fileSystem, _bundle, assetBundle);
                    Status = EOperationStatus.Succeed;
                }
            }
            else
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = _downloadAssetBundleOp.Error;
            }
        }
    }
    internal override void InternalWaitForAsyncComplete()
    {
        if (_steps != ESteps.Done)
        {
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = "Wechat platform not support sync load method !";
            UnityEngine.Debug.LogError(Error);
        }
    }
    public override void AbortDownloadOperation()
    {
        if (_steps == ESteps.DownloadAssetBundle)
        {
            if (_downloadAssetBundleOp != null)
                _downloadAssetBundleOp.SetAbort();
        }
    }
}
#endif