﻿using System;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public static class AssetBundleBuilderSetting
    {
        // EBuildPipeline
        public static EBuildPipeline GetPackageBuildPipeline(string packageName)
        {
            string key = $"{Application.productName}_{packageName}_{nameof(EBuildPipeline)}";
            return (EBuildPipeline)EditorPrefs.GetInt(key, (int)EBuildPipeline.BuiltinBuildPipeline);
        }
        public static void SetPackageBuildPipeline(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{nameof(EBuildPipeline)}";
            EditorPrefs.SetInt(key, (int)buildPipeline);
        }

        // ECompressOption
        public static ECompressOption GetPackageCompressOption(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(ECompressOption)}";
            return (ECompressOption)EditorPrefs.GetInt(key, (int)ECompressOption.LZ4);
        }
        public static void SetPackageCompressOption(string packageName, EBuildPipeline buildPipeline, ECompressOption compressOption)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(ECompressOption)}";
            EditorPrefs.SetInt(key, (int)compressOption);
        }

        // EFileNameStyle
        public static EFileNameStyle GetPackageFileNameStyle(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EFileNameStyle)}";
            return (EFileNameStyle)EditorPrefs.GetInt(key, (int)EFileNameStyle.HashName);
        }
        public static void SetPackageFileNameStyle(string packageName, EBuildPipeline buildPipeline, EFileNameStyle fileNameStyle)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EFileNameStyle)}";
            EditorPrefs.SetInt(key, (int)fileNameStyle);
        }

        // EBuildinFileCopyOption
        public static EBuildinFileCopyOption GetPackageBuildinFileCopyOption(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EBuildinFileCopyOption)}";
            return (EBuildinFileCopyOption)EditorPrefs.GetInt(key, (int)EBuildinFileCopyOption.None);
        }
        public static void SetPackageBuildinFileCopyOption(string packageName, EBuildPipeline buildPipeline, EBuildinFileCopyOption buildinFileCopyOption)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_{nameof(EBuildinFileCopyOption)}";
            EditorPrefs.SetInt(key, (int)buildinFileCopyOption);
        }

        // BuildFileCopyParams
        public static string GetPackageBuildinFileCopyParams(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_BuildFileCopyParams";
            return EditorPrefs.GetString(key, string.Empty);
        }
        public static void SetPackageBuildinFileCopyParams(string packageName, EBuildPipeline buildPipeline, string buildinFileCopyParams)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_BuildFileCopyParams";
            EditorPrefs.SetString(key, buildinFileCopyParams);
        }

        // EncyptionClassName
        public static string GetPackageEncyptionClassName(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_EncyptionClassName";
            return EditorPrefs.GetString(key, string.Empty);
        }
        public static void SetPackageEncyptionClassName(string packageName, EBuildPipeline buildPipeline, string encyptionClassName)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_EncyptionClassName";
            EditorPrefs.SetString(key, encyptionClassName);
        }

        // ClearBuildCache
        public static bool GetPackageClearBuildCache(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ClearBuildCache";
            return EditorPrefs.GetInt(key, 0) > 0;
        }
        public static void SetPackageClearBuildCache(string packageName, EBuildPipeline buildPipeline, bool clearBuildCache)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_ClearBuildCache";
            EditorPrefs.SetInt(key, clearBuildCache ? 1 : 0);
        }

        // UseAssetDependencyDB
        public static bool GetPackageUseAssetDependencyDB(string packageName, EBuildPipeline buildPipeline)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_UseAssetDependencyDB";
            return EditorPrefs.GetInt(key, 0) > 0;
        }
        public static void SetPackageUseAssetDependencyDB(string packageName, EBuildPipeline buildPipeline, bool useAssetDependencyDB)
        {
            string key = $"{Application.productName}_{packageName}_{buildPipeline}_UseAssetDependencyDB";
            EditorPrefs.SetInt(key, useAssetDependencyDB ? 1 : 0);
        }
    }
}