﻿
namespace YooAsset
{
    public class PackageDetails
    {
        /// <summary>
        /// 文件版本
        /// </summary>
        public string FileVersion;

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable;

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower;

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID;

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public int OutputNameStyle;

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType;

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        /// 资源包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 资源包裹的版本信息
        /// </summary>
        public string PackageVersion;

        /// <summary>
        /// 资源包裹的备注信息
        /// </summary>
        public string PackageNote;

        /// <summary>
        /// 主资源文件总数
        /// </summary>
        public int AssetTotalCount;

        /// <summary>
        /// 资源包文件总数
        /// </summary>
        public int BundleTotalCount;
    }
}