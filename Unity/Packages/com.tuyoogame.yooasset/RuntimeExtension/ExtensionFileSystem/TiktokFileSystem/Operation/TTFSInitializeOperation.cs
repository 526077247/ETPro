#if UNITY_WEBGL && UNITY_WEBGL_TT
using YooAsset;

internal partial class TTFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly TiktokFileSystem _fileSystem;

    private enum ESteps
    {
        None,
        LoadCatalogFile,
        Done,
    }

    private LoadTTCatalogFileOperation _loadCatalogFileOp;
    private ESteps _steps = ESteps.None;

    public TTFSInitializeOperation(TiktokFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    internal override void InternalOnStart()
    {
        if (_fileSystem.RemoteServices == null)
        {
            _steps = ESteps.Done;
            Status = EOperationStatus.Succeed;
        }
        else
        {
            _steps = ESteps.LoadCatalogFile;
        }
    }

    internal override void InternalOnUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadCatalogFile)
        {
            if (_loadCatalogFileOp == null)
            {
#if UNITY_EDITOR
                // 兼容性初始化
                // 说明：内置文件系统在编辑器下运行时需要动态生成
                string packageRoot = _fileSystem.FileRoot;
                bool result =
                    DefaultBuildinFileSystemBuild.CreateBuildinCatalogFile(_fileSystem.PackageName, packageRoot);
                if (result == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Create package catalog file failed ! See the detail error in console !";
                    return;
                }
#endif

                _loadCatalogFileOp = new LoadTTCatalogFileOperation(_fileSystem);
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
#endif