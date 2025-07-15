﻿using System;
using System.IO;

namespace YooAsset
{
    internal class DBFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private enum ESteps
        {
            None,
            InitUnpackFileSystem,
            CopyBuildinManifest,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private CopyBuildinPackageManifestOperation _copyBuildinPackageManifestOp;
        private FSInitializeFileSystemOperation _initUnpackFIleSystemOp;
        private LoadBuildinCatalogFileOperation _loadCatalogFileOp;
        private ESteps _steps = ESteps.None;

        internal DBFSInitializeOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        internal override void InternalOnStart()
        {
#if UNITY_WEBGL
            _steps = ESteps.Done;
            Status = EOperationStatus.Failed;
            Error = $"{nameof(DefaultBuildinFileSystem)} is not support WEBGL platform !";
#else
            _steps = ESteps.InitUnpackFileSystem;
#endif
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;
            
            if (_steps == ESteps.InitUnpackFileSystem)
            {
                if (_initUnpackFIleSystemOp == null)
                    _initUnpackFIleSystemOp = _fileSystem.InitializeUpackFileSystem();

                Progress = _initUnpackFIleSystemOp.Progress;
                if (_initUnpackFIleSystemOp.IsDone == false)
                    return;

                if (_initUnpackFIleSystemOp.Status == EOperationStatus.Succeed)
                {
                    if (_fileSystem.CopyBuildinPackageManifest)
                    {
                        _steps = ESteps.CopyBuildinManifest;
                    }
                    else if (_fileSystem.DisableCatalogFile)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    else
                    {
                        _steps = ESteps.LoadCatalogFile;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _initUnpackFIleSystemOp.Error;
                }
            }
            
            if (_steps == ESteps.CopyBuildinManifest)
            {
                if (_copyBuildinPackageManifestOp == null)
                {
                    _copyBuildinPackageManifestOp = new CopyBuildinPackageManifestOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _copyBuildinPackageManifestOp);
                }

                if (_copyBuildinPackageManifestOp.IsDone == false)
                    return;

                if (_copyBuildinPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    if (_fileSystem.DisableCatalogFile)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                    else
                    {
                        _steps = ESteps.LoadCatalogFile;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _copyBuildinPackageManifestOp.Error;
                }
            }
            
            if (_steps == ESteps.LoadCatalogFile)
            {
                if (_loadCatalogFileOp == null)
                {
#if UNITY_EDITOR
                    // 兼容性初始化
                    // 说明：内置文件系统在编辑器下运行时需要动态生成
                    string packageRoot = _fileSystem.FileRoot;
                    bool result = DefaultBuildinFileSystemBuild.CreateBuildinCatalogFile(_fileSystem.PackageName, packageRoot);
                    if (result == false)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Create package catalog file failed ! See the detail error in console !";
                        return;
                    }
#endif

                    _loadCatalogFileOp = new LoadBuildinCatalogFileOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCatalogFileOp);
                }

                if (_loadCatalogFileOp.IsDone == false)
                    return;

                if (_loadCatalogFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCatalogFileOp.Error;
                }
            }
        }
    }
}