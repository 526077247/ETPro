﻿using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

public static class TestPackageBuilder
{
    /// <summary>
    /// 构建资源包
    /// </summary>
    public static PackageInvokeBuildResult BuildPackage(PackageInvokeBuildParam buildParam)
    {
        string packageName = buildParam.PackageName;
        string buildPipelineName = buildParam.BuildPipelineName;

        if (buildPipelineName == EBuildPipeline.EditorSimulateBuildPipeline.ToString())
        {
            var buildParameters = new EditorSimulateBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.EditorSimulateBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.VirtualBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "ESBP_Simulate";
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.BuildinFileCopyParams = string.Empty;

            var pipeline = new EditorSimulateBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                var reulst = new PackageInvokeBuildResult();
                reulst.PackageRootDirectory = buildResult.OutputPackageDirectory;
                return reulst;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(EditorSimulateBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.ScriptableBuildPipeline.ToString())
        {
            // 内置着色器资源包名称
            var builtinShaderBundleName = GetBuiltinShaderBundleName(packageName);
            var buildParameters = new ScriptableBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "SBP_Simulate";
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.BuildinFileCopyParams = string.Empty;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;
            buildParameters.BuiltinShadersBundleName = builtinShaderBundleName;

            var pipeline = new ScriptableBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                var reulst = new PackageInvokeBuildResult();
                reulst.PackageRootDirectory = buildResult.OutputPackageDirectory;
                return reulst;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(ScriptableBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.BuiltinBuildPipeline.ToString())
        {
            var buildParameters = new BuiltinBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "BBP_Simulate";
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.BuildinFileCopyParams = string.Empty;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;

            var pipeline = new BuiltinBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                var reulst = new PackageInvokeBuildResult();
                reulst.PackageRootDirectory = buildResult.OutputPackageDirectory;
                return reulst;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(BuiltinBuildPipeline)} build failed !");
            }
        }
        else if (buildPipelineName == EBuildPipeline.RawFileBuildPipeline.ToString())
        {
            var buildParameters = new RawFileBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = EBuildPipeline.RawFileBuildPipeline.ToString();
            buildParameters.BuildBundleType = (int)EBuildBundleType.RawBundle;
            buildParameters.BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            buildParameters.PackageName = packageName;
            buildParameters.PackageVersion = "RFBP_Simulate";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.None;
            buildParameters.BuildinFileCopyParams = string.Empty;
            buildParameters.ClearBuildCacheFiles = true;
            buildParameters.UseAssetDependencyDB = true;

            var pipeline = new RawFileBuildPipeline();
            BuildResult buildResult = pipeline.Run(buildParameters, false);
            if (buildResult.Success)
            {
                var reulst = new PackageInvokeBuildResult();
                reulst.PackageRootDirectory = buildResult.OutputPackageDirectory;
                return reulst;
            }
            else
            {
                Debug.LogError(buildResult.ErrorInfo);
                throw new System.Exception($"{nameof(RawFileBuildPipeline)} build failed !");
            }
        }
        else
        {
            throw new System.NotImplementedException(buildPipelineName);
        }
    }

    /// <summary>
    /// 内置着色器资源包名称
    /// 注意：和自动收集的着色器资源包名保持一致！
    /// </summary>
    private static string GetBuiltinShaderBundleName(string packageName)
    {
        var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(packageName, uniqueBundleName);
    }
}