#if UNITY_WEBGL && UNITY_WEBGL_TT
using System;
using System.IO;
using System.Collections;
using YooAsset;
using UnityEngine;

internal sealed class LoadTTCatalogFileOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        LoadCatalog,
        Done,
    }

    private readonly TiktokFileSystem _fileSystem;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 内置清单版本
    /// </summary>
    public string PackageVersion { private set; get; }

    internal LoadTTCatalogFileOperation(TiktokFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    internal override void InternalOnStart()
    {
        _steps = ESteps.LoadCatalog;
    }

    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadCatalog)
        {
            string catalogFilePath = _fileSystem.GetCatalogFileLoadPath();
            var catalog = Resources.Load<DefaultBuildinFileCatalog>(catalogFilePath);
            if (catalog == null)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = $"Failed to load web server catalog file : {catalogFilePath}";
                return;
            }

            if (catalog.PackageName != _fileSystem.PackageName)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error =
                    $"Web server catalog file package name {catalog.PackageName} cannot match the file system package name {_fileSystem.PackageName}";
                return;
            }

            PackageVersion = catalog.PackageVersion;
            _fileSystem.BuildInPackageVersion = PackageVersion;
            foreach (var wrapper in catalog.Wrappers)
            {
                var fileWrapper = new TiktokFileSystem.FileWrapper(wrapper.FileName);
                _fileSystem.RecordCatalogFile(wrapper.BundleGUID, fileWrapper);
            }

            YooLogger.Log($"Package '{_fileSystem.PackageName}' catalog files count : {catalog.Wrappers.Count}");
            _steps = ESteps.Done;
            Status = EOperationStatus.Succeed;
        }
    }
}
#endif