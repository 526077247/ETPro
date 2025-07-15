# CHANGELOG

All notable changes to this package will be documented in this file.

## [2.2.12] - 2025-02-14

### Improvements

- WebGL网页平台支持文件加密。
- 微信小游戏平台支持文件加密。
- 抖音小游戏平台支持文件加密。

### Fixed

- (#466) 修复了微信小游戏文件系统查询机制不生效！
- (#341) 修复了微信小游戏的下载进度异常问题。
- (#471) 修复了Unity2019,Unity2020平台上，TableView视图无法显示的问题。

### Added

- 新增了ResourcePackage.UnloadAllAssetsAsync(UnloadAllAssetsOptions options)方法

  ```csharp
  public sealed class UnloadAllAssetsOptions
  {
      /// <summary>
      /// 释放所有资源句柄，防止卸载过程中触发完成回调！
      /// </summary>
      public bool ReleaseAllHandles = true;
       
      /// <summary>
      /// 卸载过程中锁定加载操作，防止新的任务请求！
      /// </summary>
      public bool LockLoadOperation = true;
  }
  ```

## [2.2.11] - 2025-02-10

### Improvements

- AssetArtScanner配置和生成报告的容错性检测。

### Fixed

- (#465) 修复了特殊情况下，没有配置资源包文件后缀名构建失败的问题。
- (#468) 修复了安卓平台二次启动加载原生文件或加密文件失败的问题。

## [2.2.10] - 2025-02-08

### Improvements

- 新增了可扩展的AssetArtScanner资源扫描工具，详细请见官方说明文档。
- 优化了AssetBundleReporter页面。
- 优化了AssetBundleDebugger页面。
- 优化了微信小游戏文件系统的缓存查询机制。
- 优化了抖音小游戏文件系统的缓存查询机制。

### Fixed

- (#447) 修复了Unity2019平台代码编译错误问题。
- (#456) 修复了在Package未激活有效清单之前，无法销毁的问题。
- (#452) 修复了内置文件系统类NeedPack方法总是返回TRUE的问题。
- (#424) 适配了Unity6000版本替换了过时方法。

### Added

- 新增了SBP构建管线构建参数：BuiltinShadersBundleName

- 新增了SBP构建管线构建参数：MonoScriptsBundleName

- 新增了全局构建管线构建参数：SingleReferencedPackAlone

  ```csharp
  /// <summary>
  /// 对单独引用的共享资源进行独立打包
  /// 说明：关闭该选项单独引用的共享资源将会构建到引用它的资源包内！
  /// </summary>
  public bool SingleReferencedPackAlone = true;
  ```

- 新增了内置文件系统初始化参数：COPY_BUILDIN_PACKAGE_MANIFEST

  ```csharp
  // 内置文件系统初始化的时候，自动拷贝内置清单到沙盒目录。
  var systemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
  systemParameters.AddParameter(FileSystemParametersDefine.COPY_BUILDIN_PACKAGE_MANIFEST, true);
  ```

## [2.2.9] - 2025-01-14

### Fixed

- (#438) 修复了纯血鸿蒙加载本地文件失败的问题。
- (#445) 修复了小游戏扩展文件系统脚本编译错误。

### Changed

- EditorSimulateModeHelper.SimulateBuild()方法变更

  ```csharp
  public static PackageInvokeBuildResult SimulateBuild(string packageName);
  ```

## [2.2.8-preview] - 2025-01-03

新增了单元测试用例。

### Improvements

- EditorSimulateModeHelper.SimulateBuild()方法提供指定自定义构建类

  ```csharp
  public class EditorSimulateBuildParam
  {
      /// <summary>
      /// 模拟构建类所属程序集名称
      /// </summary>
      public string InvokeAssmeblyName = "YooAsset.Editor";
  
      /// <summary>
      /// 模拟构建执行的类名全称       
      /// 注意：类名必须包含命名空间！  
      /// </summary>    
      public string InvokeClassFullName = "YooAsset.Editor.AssetBundleSimulateBuilder";
  
      /// <summary>     
      /// 模拟构建执行的方法名称    
      /// 注意：执行方法必须满足 BindingFlags.Public | BindingFlags.Static      
      /// </summary>       
      public string InvokeMethodName = "SimulateBuild";
  }
  ```

- 文件清理方式新增清理缓存清单。

  ```csharp
  /// <summary>
  /// 文件清理方式
  /// </summary>
  public enum EFileClearMode
  {
      /// <summary>
      /// 清理所有清单
      /// </summary>
      ClearAllManifestFiles,
  
      /// <summary>
      /// 清理未在使用的清单 
      /// </summary> 
      ClearUnusedManifestFiles,    
  }
  ```

### Fixed

- (#426) 修复了鸿蒙next平台加载内置文件路径报错的问题。
- (#428) 修复了鸿蒙next平台加载内置文件路径报错的问题。
- (#434) 修复了2.2版本 catalog文件对Json格式原生文件不记录的问题。
- (#435) 修复了WebGL平台调用MD5算法触发异常的问题。

### Added

- 新增了视频打包规则。

  ```csharp
  /// <summary>
  /// 打包视频文件
  /// </summary>
  [DisplayName("打包视频文件")]
  public class PackVideoFile : IPackRule
  ```

### Changed

- 重命名FileSystemParameters.RootDirectory字段为PackageRoot
- 重命名ResourcePackage.ClearCacheBundleFilesAsync()方法为ClearCacheFilesAsync()

## [2.2.7-preview] - 2024-12-30

### Improvements

- 重构了下载器的委托方法。

- YooAssetSettings配置文件新增Package Manifest Prefix参数。

  ```csharp
  /// <summary>
  /// 资源清单前缀名称（默认为空)
  /// </summary>
  public string PackageManifestPrefix = string.Empty;
  ```

### Fixed

- (#422) 修复了同步加载场景的NotImplementedException异常报错。
- (#418) 修复了web远程文件系统初始化不正确的问题
- (#392) 修复了引擎版本代码兼容相关的警告。
- (#332) 修复了当用户的设备中有特殊字符时，URL路径无法被正确识别的问题。

### Added

- 新增代码字段：AsyncOperationBase.PackageName

### Changed

- 重命名DownloaderOperation.OnDownloadOver()方法为DownloaderFinish()
- 重命名DownloaderOperation.OnDownloadProgress()方法为DownloadUpdate()
- 重命名DownloaderOperation.OnDownloadError()方法为DownloadError()
- 重命名DownloaderOperation.OnStartDownloadFile()方法为DownloadFileBegin()

## [2.2.6-preview] - 2024-12-27

### Improvements

- 增强了对Steam平台DLC拓展包的支持。

  ```csharp
  // 新增参数关闭Catalog目录查询内置文件的功能
  var fileSystemParams = CreateDefaultBuildinFileSystemParameters();
  fileSystemParams .AddParameter(FileSystemParametersDefine.DISABLE_CATALOG_FILE, true);
  ```

- 资源句柄基类提供了统一的Release方法。

  ```csharp
  public abstract class HandleBase : IEnumerator, IDisposable
  {
      /// <summary>
      /// 释放资源句柄
      /// </summary>
      public void Release();
  
      /// <summary>
      /// 释放资源句柄
      /// </summary>
      public void Dispose();
  }
  ```

- 优化了场景卸载逻辑。

  ```csharp
  //框架内不在区分主场景和附加场景。
  //场景卸载后自动释放资源句柄。
  ```

### Fixed

- 修复了Unity2020版本提示的脚本编译错误。
- (#417) 修复了DefaultWebServerFileSystem文件系统内Catalog未起效的问题。

### Added

- 新增示例文件 GetCacheBundleSizeOperation.cs

  可以获取指定Package的缓存资源总大小。

### Removed

- 移除了SceneHandle.IsMainScene()方法。

## [2.2.5-preview] - 2024-12-25

依赖的ScriptableBuildPipeline (SBP) 插件库版本切换为1.21.25版本！

重构了ResourceManager相关的核心代码，方便借助文件系统扩展和支持更复杂的需求！

### Editor

- 新增了编辑器模拟构建管线 EditorSimulateBuildPipeline
- 移除了EBuildMode枚举类型，构建界面有变动。
- IActiveRule分组激活接口新增GroupData类。

### Improvements

- 增加抖音小游戏文件系统，见扩展示例代码。

- 微信小游戏文件系统支持删除无用缓存文件和全部缓存文件。

- 资源构建管线现在默认剔除了Gizmos和编辑器资源。

- 优化了资源构建管线里资源收集速度。

  资源收集速度提升100倍！

  ```csharp
  class BuildParameters
  {
      /// <summary>
      /// 使用资源依赖缓存数据库
      /// 说明：开启此项可以极大提高资源收集速度
      /// </summary>
      public bool UseAssetDependencyDB = false;
  }
  ```

- WebPlayMode支持跨域加载。

  ```csharp
  // 创建默认的WebServer文件系统参数
  public static FileSystemParameters CreateDefaultWebServerFileSystemParameters(bool disableUnityWebCache = false)
  
  // 创建默认的WebRemote文件系统参数（支持跨域加载）
  public static FileSystemParameters CreateDefaultWebRemoteFileSystemParameters(IRemoteServices remoteServices, bool disableUnityWebCache = false)
  ```

- 编辑器模拟文件系统新增初始化参数：支持异步模拟加载帧数。

  ```csharp
  /// <summary>
  /// 异步模拟加载最小帧数
  /// </summary>
  FileSystemParametersDefine.ASYNC_SIMULATE_MIN_FRAME
  
  /// <summary>
  /// 异步模拟加载最大帧数
  /// </summary>
  FileSystemParametersDefine.ASYNC_SIMULATE_MAX_FRAME
  ```

- 缓存文件系统新增初始化参数：支持设置下载器最大并发连接数和单帧最大请求数

  ```csharp
  var fileSystremParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters();
  fileSystremParams .AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_CONCURRENCY, 99);
  fileSystremParams .AddParameter(FileSystemParametersDefine.DOWNLOAD_MAX_REQUEST_PER_FRAME, 10);
  ```

### Fixed

- (#349) 修复了在加载清单的时候，即使本地存在缓存文件还会去远端下载。
- (#361) 修复了协程里等待的asset handle被release，会无限等待并输出警告信息。
- (#359) 修复了SubAssetsHandle.GetSubAssetObject会获取到同名的主资源。
- (#387) 修复了加密后文件哈希冲突的时候没有抛出异常错误。
- (#404) 修复了Unity2022.3.8版本提示编译错误：Cannot resolve symbol 'AsyncInstantiateOperation' 

### Added

- 新增示例文件 CopyBuildinManifestOperation.cs

- 新增示例文件 LoadGameObjectOperation.cs

- 新增了获取配置清单详情的方法

  ```csharp
  class ResourcePackage
  {
     public PackageDetails GetPackageDetails() 
  }
  ```

- 新增了获取所有资源信息的方法

  ```csharp
  class ResourcePackage
  {
      public AssetInfo[] GetAllAssetInfos() 
  }
  ```

- 新增了清理缓存文件的通用方法

  ```csharp
  /// <summary>
  /// 文件清理方式
  /// </summary>
  public enum EFileClearMode
  {
      /// <summary>
      /// 清理所有文件
      /// </summary>
      ClearAllBundleFiles = 1,
      /// <summary>
      /// 清理未在使用的文件
      /// </summary>
      ClearUnusedBundleFiles = 2,
      /// <summary>   
      /// 清理指定标签的文件   
      /// 说明：需要指定参数，可选：string, string[], List<string>   
      /// </summary>   
      ClearBundleFilesByTags = 3,
  }
  class ResourcePackage
  {
      /// <summary>
      /// 清理缓存文件
      /// </summary>
      /// <param name="clearMode">清理方式</param>
      /// <param name="clearParam">执行参数</param>
      public ClearCacheBundleFilesOperation ClearCacheBundleFilesAsync(EFileClearMode clearMode, object clearParam = null)
  }
  ```

### Changed

- 修改了EditorSimulateModeHelper.SimulateBuild()方法

- 重命名ResourcePackage.GetAssetsInfoByTags()方法为GetAssetInfosByTags()

- 实例化对象方法增加激活参数。

  ```csharp
  public InstantiateOperation InstantiateAsync(bool actived = true)
  ```

- 清单文件的版本提升到2.2.5版本

  ```csharp
  /// <summary>
  /// 资源包裹的备注信息
  /// </summary>
  public string PackageNote;
  ```
  

### Removed

- 移除了HostPlayModeParameters.DeliveryFileSystemParameters字段
- 移除了ResourcePackage.ClearAllBundleFilesAsync()方法
- 移除了ResourcePackage.ClearUnusedBundleFilesAsync()方法
- 移除了FileSystemParameters.CreateDefaultBuildinRawFileSystemParameters()方法
- 移除了FileSystemParameters.CreateDefaultCacheRawFileSystemParameters()方法
- 移除了枚举类型：EDefaultBuildPipeline
- 移除了配置参数：YooAssetSettings.ManifestFileName

## [2.2.4-preview] - 2024-08-15

### Fixed

- 修复了HostPlayMode初始化卡死的问题。

## [2.2.3-preview] - 2024-08-13

### Fixed

- (#311) 修复了断点续传下载器极小概率报错 : “416 Range Not Satisfiable”

### Improvements

- 原生文件构建管线支持原生文件加密。

- HostPlayMode模式下内置文件系统初始化参数可以为空。

- 场景加载增加了LocalPhysicsMode参数来控制物理运行模式。

- 默认的内置文件系统和缓存文件系统增加解密方法。

  ```csharp
  /// <summary>
  /// 创建默认的内置文件系统参数
  /// </summary>
  /// <param name="decryptionServices">加密文件解密服务类</param>
  /// <param name="verifyLevel">缓存文件的校验等级</param>
  /// <param name="rootDirectory">内置文件的根路径</param>
  public static FileSystemParameters CreateDefaultBuildinFileSystemParameters(IDecryptionServices decryptionServices, EFileVerifyLevel verifyLevel, string rootDirectory);
  
  /// <summary>
  /// 创建默认的缓存文件系统参数
  /// </summary>
  /// <param name="remoteServices">远端资源地址查询服务类</param>
  /// <param name="decryptionServices">加密文件解密服务类</param>
  /// <param name="verifyLevel">缓存文件的校验等级</param>
  /// <param name="rootDirectory">文件系统的根目录</param>
  public static FileSystemParameters CreateDefaultCacheFileSystemParameters(IRemoteServices remoteServices, IDecryptionServices decryptionServices, EFileVerifyLevel verifyLevel, string rootDirectory);
  ```

## [2.2.2-preview] - 2024-07-31

### Fixed

- (#321) 修复了在Unity2022里编辑器下离线模式运行失败的问题。
- (#325) 修复了在Unity2019里编译报错问题。

## [2.2.1-preview] - 2024-07-10

统一了所有PlayMode的初始化逻辑，EditorSimulateMode和OfflinePlayMode初始化不再主动加载资源清单！

### Added

- 新增了IFileSystem.ReadFileData方法，支持原生文件自定义获取文本和二进制数据。

### Improvements

- 优化了DefaultWebFileSystem和DefaultBuildFileSystem文件系统的内部初始化逻辑。

## [2.2.0-preview] - 2024-07-07

重构了运行时代码，新增了文件系统接口（IFileSystem）方便开发者扩展特殊需求。

新增微信小游戏文件系统示例代码，详细见Extension Sample/Runtime/WechatFileSystem

### Added

- 新增了ResourcePackage.DestroyAsync方法

- 新增了FileSystemParameters类帮助初始化文件系统

  内置了编辑器文件系统参数，内置文件系统参数，缓存文件系统参数，Web文件系统参数。

  ```csharp
  public class FileSystemParameters
  {
      /// <summary>
      /// 文件系统类
      /// </summary>
      public string FileSystemClass { private set; get; }
      
      /// <summary>
      /// 文件系统的根目录
      /// </summary>
      public string RootDirectory { private set; get; }   
      
      /// <summary>
      /// 添加自定义参数
      /// </summary>
      public void AddParameter(string name, object value)    
  }
  ```

### Changed

- 重构了InitializeParameters初始化参数
- 重命名YooAssets.DestroyPackage方法为RemovePackage
- 重命名ResourcePackage.UpdatePackageVersionAsync方法为RequestPackageVersionAsync
- 重命名ResourcePackage.UnloadUnusedAssets方法为UnloadUnusedAssetsAsync
- 重命名ResourcePackage.ForceUnloadAllAssets方法为UnloadAllAssetsAsync
- 重命名ResourcePackage.ClearUnusedCacheFilesAsync方法为ClearUnusedBundleFilesAsync
- 重命名ResourcePackage.ClearAllCacheFilesAsync方法为ClearAllBundleFilesAsync

### Removed

- 移除了YooAssets.Destroy方法
- 移除了YooAssets.SetDownloadSystemClearFileResponseCode方法
- 移除了YooAssets.SetCacheSystemDisableCacheOnWebGL方法
- 移除了ResourcePackage.GetPackageBuildinRootDirectory方法
- 移除了ResourcePackage.GetPackageSandboxRootDirectory方法
- 移除了ResourcePackage.ClearPackageSandbox方法
- 移除了IBuildinQueryServices接口
- 移除了IDeliveryLoadServices接口
- 移除了IDeliveryQueryServices接口


## [2.1.2] - 2024-05-16

SBP库依赖版本升级至2.1.3

### Fixed

- (#236) 修复了资源配置界面AutoCollectShader复选框没有刷新的问题。
- (#244) 修复了导入器在安卓平台导入本地下载的资源失败的问题。
- (#268) 修复了挂起场景未解除状态前无法卸载的问题。
- (#269) 优化场景挂起流程，支持中途取消挂起操作。
- (#276) 修复了HostPlayMode模式下，如果内置清单是最新版本，每次运行都会触发拷贝行为。
- (#289) 修复了Unity2019版本脚本IWebRequester编译报错。
- (#295) 解决了在安卓移动平台，华为和三星真机上有极小概率加载资源包失败 : Unable to open archive file

### Added

- 新增GetAllCacheFileInfosOperation()获取缓存文件信息的方法。

- 新增LoadSceneSync()同步加载场景的方法。

- 新增IIgnoreRule接口，资源收集流程可以自定义。

- 新增IWechatQueryServices接口，用于微信平台本地文件查询。

  后续将会通过虚拟文件系统来支持！

### Changed

- 调整了UnloadSceneOperation代码里场景的卸载顺序。

### Improvements

- 优化了资源清单的解析过程。
- 移除资源包名里的空格字符。
- 支持华为鸿蒙系统。

## [2.1.1] - 2024-01-17

### Fixed

- (#224)  修复了编辑器模式打包时 SimulateBuild 报错的问题。
- (#223)  修复了资源构建界面读取配置导致的报错问题。

### Added

- 支持共享资源打包规则，可以定制化独立的构建规则。

  ```c#
  public class BuildParameters
  {
     /// <summary>
      /// 是否启用共享资源打包
      /// </summary>
      public bool EnableSharePackRule = false; 
  }
  ```

- 微信小游戏平台，资源下载器支持底层缓存查询。

## [2.1.0] - 2023-12-27

升级了 Scriptable build pipeline (SBP) 的版本，来解决图集引用的精灵图片冗余问题。

### Fixed

- (#195) 修复了在EditorPlayMode模式下，AssetHandle.GetDownloadStatus()发生异常的问题。
- (#201) 修复了断点续传失效的问题。
- (#202) 修复了打包参数FileNameStyle设置为BundleName后，IQueryServices会一直返回true的问题。
- (#205) 修复了HybridCLR插件里创建资源下载器触发的异常。
- (#210) 修复了DownloaderOperation在未开始下载前，内部的PackageName为空的问题。
- (#220) 修复了资源收集界面关闭后，撤回操作还会生效的问题。
- 修复了下载器合并后重新计算下载字节数不正确的问题。

### Improvements

- (#198) 资源收集界面禁用的分组不再检测合法性。
- (#203) 资源构建类容许自定义打包的输出目录。
- 资源构建报告增加未依赖的资源信息列表。

### Changed

- IBuildinQueryServices和IDeliveryQueryServices查询方法变更。

  ```c#
      public interface IBuildinQueryServices
      {
          /// <summary>
          /// 查询是否为应用程序内置的资源文件
          /// </summary>
          /// <param name="packageName">包裹名称</param>
          /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
          /// <param name="fileCRC">文件哈希值</param>
          /// <returns>返回查询结果</returns>
          bool Query(string packageName, string fileName, string fileCRC);
      }
  
     	public interface IDeliveryQueryServices
      {
          /// <summary>
          /// 查询是否为开发者分发的资源文件
          /// </summary>
          /// <param name="packageName">包裹名称</param>
          /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
          /// <param name="fileCRC">文件哈希值</param>
          /// <returns>返回查询结果</returns>
          bool Query(string packageName, string fileName, string fileCRC);
      }
  ```

  

### Removed

- (#212)  移除了构建报告里的资源冗余信息列表。

