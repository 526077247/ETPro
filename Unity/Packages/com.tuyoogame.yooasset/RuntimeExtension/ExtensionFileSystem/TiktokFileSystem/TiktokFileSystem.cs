﻿#if UNITY_WEBGL && UNITY_WEBGL_TT
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using TTSDK;
using System.Linq;
using System;
using System.IO;
using UnityEngine.Scripting;

public static class TiktokFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteServices remoteServices)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.RuntimeExtension";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }

    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteServices remoteServices, IWebDecryptionServices decryptionServices)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.RuntimeExtension";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        fileSystemParams.AddParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, decryptionServices);
        return fileSystemParams;
    }
}

/// <summary>
/// 抖音小游戏文件系统
/// 参考：https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/know
/// </summary>
[Preserve]
internal class TiktokFileSystem : IFileSystem,IDefaultWebServerFileSystem
{
    private class WebRemoteServices : IRemoteServices
    {
        private readonly string _webPackageRoot;
        protected readonly Dictionary<string, string> _mapping = new Dictionary<string, string>(10000);

        public WebRemoteServices(string buildinPackRoot)
        {
            _webPackageRoot = buildinPackRoot;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return GetFileLoadURL(fileName);
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return GetFileLoadURL(fileName);
        }

        private string GetFileLoadURL(string fileName)
        {
            if (_mapping.TryGetValue(fileName, out string url) == false)
            {
                string filePath = PathUtility.Combine(_webPackageRoot, fileName);
                url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                _mapping.Add(fileName, url);
            }
            return url;
        }
    }
    public class FileWrapper
    {
        public string FileName { private set; get; }

        public FileWrapper(string fileName)
        {
            FileName = fileName;
        }
    }

    protected readonly Dictionary<string, FileWrapper> _wrappers = new Dictionary<string, FileWrapper>(10000);
    private readonly Dictionary<string, string> _cacheFilePathMapping = new Dictionary<string, string>(10000);
    private TTFileSystemManager _fileSystemMgr;
    private string _ttCacheRoot = string.Empty;

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
            return _ttCacheRoot;
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
    /// 自定义参数：远程服务接口
    /// </summary>
    public IRemoteServices RemoteServices { private set; get; } = null;

    /// <summary>
    ///  自定义参数：解密方法类
    /// </summary>
    public IWebDecryptionServices DecryptionServices { private set; get; }

    private bool isWebServerPackage;
    #endregion


    public TiktokFileSystem()
    {
    }
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new TTFSInitializeOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new TTFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new TTFSRequestPackageVersionOperation(this, timeout);
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
        param.MainURL = RemoteServices.GetRemoteMainURL(bundle.FileName);
        param.FallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName);
        var operation = new TTFSDownloadFileOperation(this, bundle, param);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        if (bundle.BundleType == (int)EBuildBundleType.AssetBundle)
        {
            var operation = new TTFSLoadBundleOperation(this, bundle);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
        else
        {
            string error = $"{nameof(TiktokFileSystem)} not support load bundle type : {bundle.BundleType}";
            var operation = new FSLoadBundleCompleteOperation(error);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }
    }

    public virtual void SetParameter(string name, object value)
    {
        if (name == FileSystemParametersDefine.REMOTE_SERVICES)
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
    public virtual void OnCreate(string packageName, string rootDirectory)
    {
        PackageName = packageName;
        _ttCacheRoot = rootDirectory;

        if (string.IsNullOrEmpty(_ttCacheRoot))
        {
            throw new System.Exception("请配置抖音小游戏的缓存根目录！");
        }

        isWebServerPackage = false;
        // 注意：CDN服务未启用的情况下，使用抖音WEB服务器
        if (RemoteServices == null)
        {
            isWebServerPackage = true;
            string webRoot = PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _fileSystemMgr = TT.GetFileSystemManager();
    }
    public virtual void OnUpdate()
    {
    }

    public virtual bool Belong(PackageBundle bundle)
    {
        if (!isWebServerPackage) return true;
        return _wrappers.ContainsKey(bundle.BundleGUID);
    }
    public virtual bool Exists(PackageBundle bundle)
    {
        return CheckCacheFileExist(bundle);
    }
    public virtual bool NeedDownload(PackageBundle bundle)
    {
        if (Belong(bundle) == false)
            return false;

        return Exists(bundle) == false;
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
        return GetCacheFileLoadPath(bundle);
    }
    public virtual byte[] ReadBundleFileData(PackageBundle bundle)
    {
        if (CheckCacheFileExist(bundle))
        {
            string filePath = GetCacheFileLoadPath(bundle);
            return _fileSystemMgr.ReadFileSync(filePath);
        }
        else
        {
            return Array.Empty<byte>();
        }
    }
    public virtual string ReadBundleFileText(PackageBundle bundle)
    {
        if (CheckCacheFileExist(bundle))
        {
            string filePath = GetCacheFileLoadPath(bundle);
            return _fileSystemMgr.ReadFileSync(filePath, "utf8");
        }
        else
        {
            return string.Empty;
        }
    }

    #region 内部方法
    public TTFileSystemManager GetFileSystemMgr()
    {
        return _fileSystemMgr;
    }
    public bool CheckCacheFileExist(PackageBundle bundle)
    {
        string url = RemoteServices.GetRemoteMainURL(bundle.FileName);
        return _fileSystemMgr.IsUrlCached(url);
    }
    private string GetCacheFileLoadPath(PackageBundle bundle)
    {
        if (_cacheFilePathMapping.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = _fileSystemMgr.GetLocalCachedPathForUrl(bundle.FileName);
            _cacheFilePathMapping.Add(bundle.BundleGUID, filePath);
        }
        return filePath;
    }

    /// <summary>
    /// 加载加密资源文件
    /// </summary>
    public AssetBundle LoadEncryptedAssetBundle(PackageBundle bundle, byte[] fileData)
    {
        WebDecryptFileInfo fileInfo = new WebDecryptFileInfo();
        fileInfo.BundleName = bundle.BundleName;
        fileInfo.FileLoadCRC = bundle.UnityCRC;
        fileInfo.FileData = fileData;
        var decryptResult = DecryptionServices.LoadAssetBundle(fileInfo);
        return decryptResult.Result;
    }
    
    public string GetCatalogFileLoadPath()
    {
        string fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
        return YooAssetSettingsData.GetYooResourcesLoadPath(PackageName, fileName);
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
    
    public bool IsBuildInVersion(string packageVersion)
    {
        return BuildInPackageVersion == packageVersion;
    }
    #endregion
}
#endif