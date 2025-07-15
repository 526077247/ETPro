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
    internal class DefaultWebServerFileSystem : IFileSystem,IDefaultWebServerFileSystem
    {
        public class FileWrapper
        {
            public string FileName { private set; get; }

            public FileWrapper(string fileName)
            {
                FileName = fileName;
            }
        }

        protected readonly Dictionary<string, FileWrapper> _wrappers = new Dictionary<string, FileWrapper>(10000);
        protected readonly Dictionary<string, string> _webFilePathMapping = new Dictionary<string, string>(10000);
        protected string _webPackageRoot = string.Empty;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }
        /// <summary>
        /// 内置包裹版本
        /// </summary>
        public string BuildInPackageVersion { internal set; get; }
        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get
            {
                return _webPackageRoot;
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
        ///  自定义参数：解密方法类
        /// </summary>
        public IWebDecryptionServices DecryptionServices { private set; get; }
        #endregion


        public DefaultWebServerFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DWSFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DWSFSLoadPackageManifestOperation(this, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DWSFSRequestPackageVersionOperation(this, timeout);
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
                var operation = new DWSFSLoadAssetBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                string error = $"{nameof(DefaultWebServerFileSystem)} not support load bundle type : {bundle.BundleType}";
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

            if (string.IsNullOrEmpty(packageRoot))
                _webPackageRoot = GetDefaultWebPackageRoot(packageName);
            else
                _webPackageRoot = packageRoot;
        }
        public virtual void OnUpdate()
        {
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            return _wrappers.ContainsKey(bundle.BundleGUID);
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

        public bool IsBuildInVersion(string packageVersion)
        {
            return BuildInPackageVersion == packageVersion;
        }

        #region 内部方法
        protected string GetDefaultWebPackageRoot(string packageName)
        {
            string rootDirectory = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }
        public string GetWebFileLoadPath(PackageBundle bundle)
        {
            if (_webFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_webPackageRoot, bundle.FileName);
                _webFilePathMapping.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }
        public string GetWebPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetWebPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetWebPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }
        public string GetCatalogFileLoadPath()
        {
            string fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
            return YooAssetSettingsData.GetYooResourcesLoadPath(PackageName, fileName);
        }

        /// <summary>
        /// 记录内置文件信息
        /// </summary>
        public bool RecordCatalogFile(string bundleGUID, FileWrapper wrapper)
        {
            if (_wrappers.ContainsKey(bundleGUID))
            {
                YooLogger.Error($"{nameof(DefaultWebServerFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }
        #endregion
    }
}