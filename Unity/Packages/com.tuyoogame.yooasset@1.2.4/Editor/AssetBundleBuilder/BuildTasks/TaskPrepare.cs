﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	[TaskAttribute("资源构建准备工作")]
	public class TaskPrepare : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			buildParameters.BeginWatch();

			var buildMode = buildParameters.Parameters.BuildMode;

			// 检测构建平台是否合法
			if (buildParameters.Parameters.BuildTarget == BuildTarget.NoTarget)
				throw new Exception("请选择目标平台");

			// 检测构建版本是否合法
			if (buildParameters.Parameters.BuildVersion <= 0)
				throw new Exception("请先设置版本号");

			// 检测输出目录是否为空
			if (string.IsNullOrEmpty(buildParameters.PipelineOutputDirectory))
				throw new Exception("输出目录不能为空");

			if (buildMode != EBuildMode.SimulateBuild)
			{
				// 检测当前是否正在构建资源包
				if (BuildPipeline.isBuildingPlayer)
					throw new Exception("当前正在构建资源包，请结束后再试");

				// 检测是否有未保存场景
				if (EditorTools.HasDirtyScenes())
					throw new Exception("检测到未保存的场景文件");

				// 保存改动的资源
				AssetDatabase.SaveAssets();
			}

			// 增量更新时候的必要检测
			if (buildMode == EBuildMode.IncrementalBuild)
			{
				// 检测历史版本是否存在
				if (AssetBundleBuilderHelper.HasAnyPackageVersion(buildParameters.Parameters.BuildTarget, buildParameters.Parameters.OutputRoot))
				{
					// 检测构建版本是否合法
					int maxPackageVersion = AssetBundleBuilderHelper.GetMaxPackageVersion(buildParameters.Parameters.BuildTarget, buildParameters.Parameters.OutputRoot);
					if (buildParameters.Parameters.BuildVersion <= maxPackageVersion)
						throw new Exception("构建版本不能小于历史版本");

					// 检测补丁包是否已经存在
					string packageDirectory = buildParameters.GetPackageDirectory();
					if (Directory.Exists(packageDirectory))
						throw new Exception($"补丁包已经存在：{packageDirectory}");

					// 检测内置资源分类标签是否一致
					var oldPatchManifest = AssetBundleBuilderHelper.GetOldPatchManifest(buildParameters.PipelineOutputDirectory);
					if (buildParameters.Parameters.BuildinTags != oldPatchManifest.BuildinTags)
						throw new Exception($"增量更新时内置资源标签必须一致：{buildParameters.Parameters.BuildinTags} != {oldPatchManifest.BuildinTags}");
				}
			}

			// 如果是强制重建
			if (buildMode == EBuildMode.ForceRebuild)
			{
				// 删除平台总目录
				string platformDirectory = $"{buildParameters.Parameters.OutputRoot}/{buildParameters.Parameters.BuildTarget}";
				if (EditorTools.DeleteDirectory(platformDirectory))
				{
					BuildRunner.Log($"删除平台总目录：{platformDirectory}");
				}
			}

			// 如果输出目录不存在
			if (EditorTools.CreateDirectory(buildParameters.PipelineOutputDirectory))
			{
				BuildRunner.Log($"创建输出目录：{buildParameters.PipelineOutputDirectory}");
			}
		}
	}
}