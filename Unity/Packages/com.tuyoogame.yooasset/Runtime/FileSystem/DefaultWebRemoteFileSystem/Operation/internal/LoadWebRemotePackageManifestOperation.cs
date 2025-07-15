﻿
namespace YooAsset
{
    internal class LoadWebRemotePackageManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly DefaultWebRemoteFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly string _packageHash;
        private UnityWebDataRequestOperation _webDataRequestOp;
        private DeserializeManifestOperation _deserializer;
        private int _requestCount = 0;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        internal LoadWebRemotePackageManifestOperation(DefaultWebRemoteFileSystem fileSystem, string packageVersion, string packageHash)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
        }
        internal override void InternalOnStart()
        {
            _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(LoadWebRemotePackageManifestOperation));
            _steps = ESteps.RequestFileData;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestFileData)
            {
                if (_webDataRequestOp == null)
                {
                    string fileName = YooAssetSettingsData.GetManifestBinaryFileName(_fileSystem.PackageName, _packageVersion);
                    string url = GetWebRequestURL(fileName);
                    _webDataRequestOp = new UnityWebDataRequestOperation(url);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webDataRequestOp);
                }

                if (_webDataRequestOp.IsDone == false)
                    return;

                if (_webDataRequestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.VerifyFileData;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webDataRequestOp.Error;
                    WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(LoadWebRemotePackageManifestOperation));
                }
            }

            if (_steps == ESteps.VerifyFileData)
            {
                string fileHash = HashUtility.BytesCRC32(_webDataRequestOp.Result);
                if (fileHash == _packageHash)
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to verify web remote package manifest file!";
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializer == null)
                {
                    _deserializer = new DeserializeManifestOperation(_webDataRequestOp.Result);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _deserializer);
                }

                Progress = _deserializer.Progress;
                if (_deserializer.IsDone == false)
                    return;

                if (_deserializer.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _deserializer.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _deserializer.Error;
                }
            }
        }

        private string GetWebRequestURL(string fileName)
        {
            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                return _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
            else
                return _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);
        }
    }
}