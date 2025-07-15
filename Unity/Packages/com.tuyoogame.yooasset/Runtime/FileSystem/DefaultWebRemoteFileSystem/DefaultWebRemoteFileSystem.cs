﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace YooAsset
{
    /// <summary>
    /// Web文件系统
    /// </summary>
    [Preserve]
    internal class DefaultWebRemoteFileSystem : IFileSystem
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get
            {
                return 0;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 禁用Unity的网络缓存
        /// </summary>
        public bool DisableUnityWebCache { private set; get; } = false;

        /// <summary>
        /// 自定义参数：跨域下载服务接口
        /// </summary>
        public IRemoteServices RemoteServices { private set; get; } = null;

        /// <summary>
        ///  自定义参数：解密方法类
        /// </summary>
        public IWebDecryptionServices DecryptionServices { private set; get; }
        #endregion


        public DefaultWebRemoteFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DWRFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DWRFSLoadPackageManifestOperation(this, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DWRFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, string clearMode, object clearParam)
        {
            var operation = new FSClearCacheFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            throw new System.NotImplementedException();
        }
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
            {
                var operation = new DWRFSLoadAssetBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                string error = $"{nameof(DefaultWebRemoteFileSystem)} not support load bundle type : {bundle.BundleType}";
                var operation = new FSLoadBundleCompleteOperation(error);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE)
            {
                DisableUnityWebCache = (bool)value;
            }
            else if (name == FileSystemParametersDefine.REMOTE_SERVICES)
            {
                RemoteServices = (IRemoteServices)value;
            }
            else if (name == FileSystemParametersDefine.DECRYPTION_SERVICES)
            {
                DecryptionServices = (IWebDecryptionServices)value;
            }
            else
            {
                YooLogger.Warning($"Invalid parameter : {name}");
            }
        }
        public virtual void OnCreate(string packageName, string packageRoot)
        {
            PackageName = packageName;
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return true;
        }
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual string GetBundleFilePath(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }
        public virtual byte[] ReadBundleFileData(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }
        public virtual string ReadBundleFileText(PackageBundle bundle)
        {
            throw new System.NotImplementedException();
        }

        #region 内部方法
        #endregion
    }
}