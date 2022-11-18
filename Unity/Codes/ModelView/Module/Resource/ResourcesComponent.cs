using System.Collections.Generic;
using YooAsset;
namespace ET
{
    
    /// <summary>
    /// <para>资源管理系统：提供资源加载管理</para>
    /// <para>注意：</para>
    /// <para>1、对于串行执行一连串的异步操作，建议使用协程（用同步形式的代码写异步逻辑），回调方式会使代码难读</para>
    /// <para>2、理论上做到逻辑层脚本对AB名字是完全透明的，所有资源只有packagePath的概念，这里对路径进行处理</para>
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class ResourcesComponent: Entity,IAwake,IDestroy
    {
        public static ResourcesComponent Instance;
        public int ProcessingAddressablesAsyncLoaderCount = 0;
        public Dictionary<object, AssetOperationHandle> Temp;
        public List<AssetOperationHandle> CachedAssetOperationHandles;
    }
}