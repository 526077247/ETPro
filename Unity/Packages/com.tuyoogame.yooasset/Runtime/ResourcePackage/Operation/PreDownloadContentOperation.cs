﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 预下载内容
    /// 说明：目前只支持联机模式
    /// </summary>
    public abstract class PreDownloadContentOperation : AsyncOperationBase
    {
        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        public abstract ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60);
    }

    internal class EditorSimulateModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private readonly EditorSimulateModeImpl _impl;

        public EditorSimulateModePreDownloadContentOperation(EditorSimulateModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    }
    internal class OfflinePlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private readonly OfflinePlayModeImpl _impl;

        public OfflinePlayModePreDownloadContentOperation(OfflinePlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    }
    internal class HostPlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private enum ESteps
        {
            None,
            CheckParams,
            CheckActiveManifest,
            LoadPackageManifest,
            Done,
        }

        private readonly HostPlayModeImpl _impl;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private FSLoadPackageManifestOperation _loadPackageManifestOp;
        private PackageManifest _manifest;
        private ESteps _steps = ESteps.None;


        internal HostPlayModePreDownloadContentOperation(HostPlayModeImpl impl, string packageVersion, int timeout)
        {
            _impl = impl;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.CheckParams;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.CheckParams)
            {
                if (string.IsNullOrEmpty(_packageVersion))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Package version is null or empty.";
                    return;
                }

                _steps = ESteps.CheckActiveManifest;
            }

            if (_steps == ESteps.CheckActiveManifest)
            {
                // 检测当前激活的清单对象
                if (_impl.ActiveManifest != null)
                {
                    if (_impl.ActiveManifest.PackageVersion == _packageVersion)
                    {
                        _manifest = _impl.ActiveManifest;
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                        return;
                    }
                }
                _steps = ESteps.LoadPackageManifest;
            }

            if (_steps == ESteps.LoadPackageManifest)
            {
                if (_loadPackageManifestOp == null)
                {
                    _loadPackageManifestOp = _impl.CacheFileSystem.LoadPackageManifestAsync(_packageVersion, _timeout);
                }

                if (_loadPackageManifestOp.IsDone == false)
                    return;

                if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _manifest = _loadPackageManifestOp.Manifest;
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadPackageManifestOp.Error;
                }
            }
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByAll(_manifest, _impl.BuildinFileSystem, _impl.CacheFileSystem);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByTags(_manifest, new string[] { tag }, _impl.BuildinFileSystem, _impl.CacheFileSystem);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByTags(_manifest, tags, _impl.BuildinFileSystem, _impl.CacheFileSystem);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<AssetInfo> assetInfos = new List<AssetInfo>();
            var assetInfo = _manifest.ConvertLocationToAssetInfo(location, null);
            assetInfos.Add(assetInfo);

            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByPaths(_manifest, assetInfos.ToArray(), _impl.BuildinFileSystem, _impl.CacheFileSystem);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            if (Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"{nameof(PreDownloadContentOperation)} status is not succeed !");
                return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
            }

            List<AssetInfo> assetInfos = new List<AssetInfo>(locations.Length);
            foreach (var location in locations)
            {
                var assetInfo = _manifest.ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }

            List<BundleInfo> downloadList = PlayModeHelper.GetDownloadListByPaths(_manifest, assetInfos.ToArray(), _impl.BuildinFileSystem, _impl.CacheFileSystem);
            var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
            return operation;
        }
    }
    internal class WebPlayModePreDownloadContentOperation : PreDownloadContentOperation
    {
        private readonly WebPlayModeImpl _impl;

        public WebPlayModePreDownloadContentOperation(WebPlayModeImpl impl)
        {
            _impl = impl;
        }
        internal override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }
        internal override void InternalOnUpdate()
        {
        }

        public override ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
        public override ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
        }
    }
}