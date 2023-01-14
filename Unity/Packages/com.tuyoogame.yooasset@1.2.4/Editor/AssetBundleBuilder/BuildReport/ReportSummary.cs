using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[Serializable]
	public class ReportSummary
	{
		/// <summary>
		/// YooAsset版本
		/// </summary>
		public string YooVersion;

		/// <summary>
		/// 引擎版本
		/// </summary>
		public string UnityVersion;

		/// <summary>
		/// 构建时间
		/// </summary>
		public string BuildDate;
		
		/// <summary>
		/// 构建耗时（单位：秒）
		/// </summary>
		public int BuildSeconds;

		/// <summary>
		/// 构建平台
		/// </summary>
		public BuildTarget BuildTarget;

		/// <summary>
		/// 构建管线
		/// </summary>
		public EBuildPipeline BuildPipeline;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode;

		/// <summary>
		/// 构建版本
		/// </summary>
		public int BuildVersion;

		/// <summary>
		/// 内置资源标签
		/// </summary>
		public string BuildinTags;

		/// <summary>
		/// 启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable;

		/// <summary>
		/// 拷贝内置资源文件
		/// </summary>
		public bool CopyBuildinTagFiles;

		/// <summary>
		/// 加密服务类名称
		/// </summary>
		public string EncryptionServicesClassName;

		// 构建参数
		public EOutputNameStyle OutputNameStyle;
		public ECompressOption CompressOption;
		public bool DisableWriteTypeTree;
		public bool IgnoreTypeTreeChanges;

		// 构建结果
		public int AssetFileTotalCount;
		public int MainAssetTotalCount;
		public int AllBundleTotalCount;
		public long AllBundleTotalSize;
		public int BuildinBundleTotalCount;
		public long BuildinBundleTotalSize;
		public int EncryptedBundleTotalCount;
		public long EncryptedBundleTotalSize;
		public int RawBundleTotalCount;
		public long RawBundleTotalSize;
	}
}