﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace YooAsset
{
    /// <summary>
    /// 内置文件系统
    /// </summary>
    [Preserve]
    internal class DefaultBuildinFileSystem : IFileSystem
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
        protected readonly Dictionary<string, string> _buildinFilePathMapping = new Dictionary<string, string>(10000);
        protected IFileSystem _unpackFileSystem;
        protected string _packageRoot;

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
                return _packageRoot;
            }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get
            {
                return _wrappers.Count;
            }
        }

        #region 自定义参数
        /// <summary>
        /// 自定义参数：初始化的时候缓存文件校验级别
        /// </summary>
        public EFileVerifyLevel FileVerifyLevel { private set; get; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 自定义参数：数据文件追加文件格式
        /// </summary>
        public bool AppendFileExtension { private set; get; } = false;

        /// <summary>
        /// 自定义参数：禁用Catalog目录查询文件
        /// </summary>
        public bool DisableCatalogFile { private set; get; } = false;

        /// <summary>
        /// 自定义参数：拷贝内置清单
        /// </summary>
        public bool CopyBuildinPackageManifest { private set; get; } = false;

        /// <summary>
        /// 自定义参数：拷贝内置清单的目标目录
        /// 注意：该参数为空的时候，会获取默认的沙盒目录！
        /// </summary>
        public string CopyBuildinPackageManifestDestRoot { private set; get; }

        /// <summary>
        ///  自定义参数：解密方法类
        /// </summary>
        public IDecryptionServices DecryptionServices { private set; get; }
        #endregion


        public DefaultBuildinFileSystem()
        {
        }
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DBFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DBFSLoadPackageManifestOperation(this, packageVersion);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DBFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        public virtual FSClearCacheFilesOperation ClearCacheFilesAsync(PackageManifest manifest, string clearMode, object clearParam)
        {
            return _unpackFileSystem.ClearCacheFilesAsync(manifest, clearMode, clearParam);
        }
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            // 注意：业务层的解压下载器会依赖内置文件系统的下载方法
            param.ImportFilePath = GetBuildinFileLoadPath(bundle);
            return _unpackFileSystem.DownloadFileAsync(bundle, param);
        }
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            if (IsUnpackBundleFile(bundle))
            {
                return _unpackFileSystem.LoadBundleFile(bundle);
            }

            if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
            {
                var operation = new DBFSLoadAssetBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else if (bundle.BundleType == (int)EBuildBundleType.RawBundle)
            {
                var operation = new DBFSLoadRawBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                string error = $"{nameof(DefaultBuildinFileSystem)} not support load bundle type : {bundle.BundleType}";
                var operation = new FSLoadBundleCompleteOperation(error);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }

        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.FILE_VERIFY_LEVEL)
            {
                FileVerifyLevel = (EFileVerifyLevel)value;
            }
            else if (name == FileSystemParametersDefine.APPEND_FILE_EXTENSION)
            {
                AppendFileExtension = (bool)value;
            }
            else if (name == FileSystemParametersDefine.DISABLE_CATALOG_FILE)
            {
                DisableCatalogFile = (bool)value;
            }
            else if (name == FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST)
            {
                CopyBuildinPackageManifest = (bool)value;
            }
            else if (name == FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST_DEST_ROOT)
            {
                CopyBuildinPackageManifestDestRoot = (string)value;
            }
            else if (name == FileSystemParametersDefine.DECRYPTION_SERVICES)
            {
                DecryptionServices = (IDecryptionServices)value;
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
                _packageRoot = GetDefaultBuildinPackageRoot(packageName);
            else
                _packageRoot = packageRoot;

            // 创建解压文件系统
            var remoteServices = new DefaultUnpackRemoteServices(_packageRoot);
            _unpackFileSystem = new DefaultUnpackFileSystem();
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.FILE_VERIFY_LEVEL, FileVerifyLevel);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, AppendFileExtension);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, DecryptionServices);
            _unpackFileSystem.OnCreate(packageName, null);
        }
        public virtual void OnUpdate()
        {
            _unpackFileSystem.OnUpdate();
        }

        public virtual bool Belong(PackageBundle bundle)
        {
            if (DisableCatalogFile)
                return true;
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }
        public virtual bool Exists(PackageBundle bundle)
        {
            if (DisableCatalogFile)
                return true;
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            return false;
        }
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            if (IsUnpackBundleFile(bundle))
            {
                return _unpackFileSystem.Exists(bundle) == false;
            }
            else
            {
                return false;
            }
        }
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        public virtual string GetBundleFilePath(PackageBundle bundle)
        {
            if (IsUnpackBundleFile(bundle))
                return _unpackFileSystem.GetBundleFilePath(bundle);

            return GetBuildinFileLoadPath(bundle);
        }
        public virtual byte[] ReadBundleFileData(PackageBundle bundle)
        {
            if (IsUnpackBundleFile(bundle))
                return _unpackFileSystem.ReadBundleFileData(bundle);

            if (Exists(bundle) == false)
                return null;

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    YooLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                string filePath = GetBuildinFileLoadPath(bundle);
                var fileInfo = new DecryptFileInfo()
                {
                    BundleName = bundle.BundleName,
                    FileLoadCRC = bundle.UnityCRC,
                    FileLoadPath = filePath,
                };
                return DecryptionServices.ReadFileData(fileInfo);
            }
            else
            {
                string filePath = GetBuildinFileLoadPath(bundle);
                return FileUtility.ReadAllBytes(filePath);
            }
        }
        public virtual string ReadBundleFileText(PackageBundle bundle)
        {
            if (IsUnpackBundleFile(bundle))
                return _unpackFileSystem.ReadBundleFileText(bundle);

            if (Exists(bundle) == false)
                return null;

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    YooLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                string filePath = GetBuildinFileLoadPath(bundle);
                var fileInfo = new DecryptFileInfo()
                {
                    BundleName = bundle.BundleName,
                    FileLoadCRC = bundle.UnityCRC,
                    FileLoadPath = filePath,
                };
                return DecryptionServices.ReadFileText(fileInfo);
            }
            else
            {
                string filePath = GetBuildinFileLoadPath(bundle);
                return FileUtility.ReadAllText(filePath);
            }
        }

        #region 内部方法
        protected string GetDefaultBuildinPackageRoot(string packageName)
        {
            string rootDirectory = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            return PathUtility.Combine(rootDirectory, packageName);
        }
        public string GetBuildinFileLoadPath(PackageBundle bundle)
        {
            if (_buildinFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
            {
                filePath = PathUtility.Combine(_packageRoot, bundle.FileName);
                _buildinFilePathMapping.Add(bundle.BundleGUID, filePath);
            }
            return filePath;
        }
        public string GetBuildinPackageVersionFilePath()
        {
            string fileName = YooAssetSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        public string GetBuildinPackageHashFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        public string GetBuildinPackageManifestFilePath(string packageVersion)
        {
            string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(_packageRoot, fileName);
        }
        public string GetCatalogFileLoadPath()
        {
            string fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
            return YooAssetSettingsData.GetYooResourcesLoadPath(PackageName, fileName);
        }

        /// <summary>
        /// 是否属于解压资源包文件
        /// </summary>
        protected bool IsUnpackBundleFile(PackageBundle bundle)
        {
            if (Belong(bundle) == false)
                return false;

#if UNITY_ANDROID
            if (bundle.BundleType == (int)EBuildBundleType.RawBundle || bundle.Encrypted)
                return true;
            else
                return false;
#else
            return false;
#endif
        }

        /// <summary>
        /// 记录文件信息
        /// </summary>
        public bool RecordCatalogFile(string bundleGUID, FileWrapper wrapper)
        {
            if (_wrappers.ContainsKey(bundleGUID))
            {
                YooLogger.Error($"{nameof(DefaultBuildinFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }

        /// <summary>
        /// 初始化解压文件系统
        /// </summary>
        public FSInitializeFileSystemOperation InitializeUpackFileSystem()
        {
            return _unpackFileSystem.InitializeFileSystemAsync();
        }

        /// <summary>
        /// 加载加密的资源文件
        /// </summary>
        public DecryptResult LoadEncryptedAssetBundle(PackageBundle bundle)
        {
            string filePath = GetBuildinFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };
            return DecryptionServices.LoadAssetBundle(fileInfo);
        }

        /// <summary>
        /// 加载加密的资源文件
        /// </summary>
        public DecryptResult LoadEncryptedAssetBundleAsync(PackageBundle bundle)
        {
            string filePath = GetBuildinFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo()
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };
            return DecryptionServices.LoadAssetBundleAsync(fileInfo);
        }
        #endregion
    }
}