﻿
namespace YooAsset.Editor
{
    public class CollectCommand
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 忽略规则实例
        /// </summary>
        public IIgnoreRule IgnoreRule { private set; get; }


        /// <summary>
        /// 模拟构建模式
        /// </summary>
        public bool SimulateBuild { set; get; }

        /// <summary>
        /// 资源包名唯一化
        /// </summary>
        public bool UniqueBundleName { set; get; }

        /// <summary>
        /// 使用资源依赖数据库
        /// </summary>
        public bool UseAssetDependencyDB { set; get; }

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable { set; get; }

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower { set; get; }

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID { set; get; }

        /// <summary>
        /// 自动收集所有着色器
        /// </summary>
        public bool AutoCollectShaders { set; get; }

        private AssetDependencyCache _assetDependency;
        public AssetDependencyCache AssetDependency
        {
            get
            {
                if (_assetDependency == null)
                    _assetDependency = new AssetDependencyCache(UseAssetDependencyDB);
                return _assetDependency;
            }
        }

        public CollectCommand(string packageName, IIgnoreRule ignoreRule)
        {
            PackageName = packageName;
            IgnoreRule = ignoreRule;
        }
    }
}