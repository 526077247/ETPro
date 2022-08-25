﻿using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	[TaskAttribute("制作补丁包")]
	public class TaskCreatePatchPackage : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<BuildParametersContext>();
			var buildMode = buildParameters.Parameters.BuildMode;
			if (buildMode == EBuildMode.ForceRebuild || buildMode == EBuildMode.IncrementalBuild)
			{
				CopyPatchFiles(buildParameters);
			}
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPatchFiles(BuildParametersContext buildParameters)
		{
			int resourceVersion = buildParameters.Parameters.BuildVersion;
			string packageDirectory = buildParameters.GetPackageDirectory();
			BuildRunner.Log($"开始拷贝补丁文件到补丁包目录：{packageDirectory}");

			// 拷贝Report文件
			{
				string reportFileName = YooAssetSettingsData.GetReportFileName(buildParameters.Parameters.BuildVersion);
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{reportFileName}";
				string destPath = $"{packageDirectory}/{reportFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(resourceVersion)}";
				string destPath = $"{packageDirectory}/{YooAssetSettingsData.GetPatchManifestFileName(resourceVersion)}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝补丁清单哈希文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(resourceVersion)}";
				string destPath = $"{packageDirectory}/{YooAssetSettingsData.GetPatchManifestHashFileName(resourceVersion)}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝静态版本文件
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.VersionFileName}";
				string destPath = $"{packageDirectory}/{YooAssetSettings.VersionFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			if (buildParameters.Parameters.BuildPipeline == EBuildPipeline.ScriptableBuildPipeline)
			{
				// 拷贝构建日志
				{
					string sourcePath = $"{buildParameters.PipelineOutputDirectory}/buildlogtep.json";
					string destPath = $"{packageDirectory}/buildlogtep.json";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝代码防裁剪配置
				if (buildParameters.Parameters.SBPParameters.WriteLinkXML)
				{
					string sourcePath = $"{buildParameters.PipelineOutputDirectory}/link.xml";
					string destPath = $"{packageDirectory}/link.xml";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}
			else
			{
				// 拷贝UnityManifest序列化文件
				{
					string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}";
					string destPath = $"{packageDirectory}/{YooAssetSettings.OutputFolderName}";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}

				// 拷贝UnityManifest文本文件
				{
					string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
					string destPath = $"{packageDirectory}/{YooAssetSettings.OutputFolderName}.manifest";
					EditorTools.CopyFile(sourcePath, destPath, true);
				}
			}

			// 拷贝所有补丁文件
			int progressValue = 0;
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory, buildParameters.Parameters.BuildVersion);
			int patchFileTotalCount = patchManifest.BundleList.Count;
			foreach (var patchBundle in patchManifest.BundleList)
			{
				string sourcePath = $"{buildParameters.PipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{packageDirectory}/{patchBundle.FileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
			}
			EditorTools.ClearProgressBar();
		}
	}
}