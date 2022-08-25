using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR
{
    public static partial class BuildConfig
    {

        public static string ProjectDir => Directory.GetParent(Application.dataPath).ToString();

        public static string ScriptingAssembliesJsonFile { get; } = "ScriptingAssemblies.json";

        public static string HybridCLRBuildCacheDir => Application.dataPath + "/HybridCLRBuildCache";

        public static string HotFixDllsOutputDir => $"{HybridCLRDataDir}/HotFixDlls";

        public static string AssetBundleOutputDir => $"{HybridCLRBuildCacheDir}/AssetBundleOutput";

        public static string AssetBundleSourceDataTempDir => $"{HybridCLRBuildCacheDir}/AssetBundleSourceData";

        public static string HybridCLRDataDir { get; } = $"{ProjectDir}/HybridCLRData";

        public static string AssembliesPostIl2CppStripDir => $"{HybridCLRDataDir}/AssembliesPostIl2CppStrip";

        public static string LocalIl2CppDir => $"{HybridCLRDataDir}/LocalIl2CppData/il2cpp";

        public static string MethodBridgeCppDir => $"{LocalIl2CppDir}/libil2cpp/hybridclr/interpreter";

        public static string Il2CppBuildCacheDir { get; } = $"{ProjectDir}/Library/Il2cppBuildCache";

        public static string GetHotFixDllsOutputDirByTarget(BuildTarget target)
        {
            return $"{HotFixDllsOutputDir}/{target}";
        }

        public static string GetAssembliesPostIl2CppStripDir(BuildTarget target)
        {
            return $"{AssembliesPostIl2CppStripDir}/{target}";
        }

        public static string GetOriginBuildStripAssembliesDir(BuildTarget target)
        {
#if UNITY_2021_1_OR_NEWER
#if UNITY_STANDALONE_WIN
            return $"{ProjectDir}/Library/Bee/artifacts/WinPlayerBuildProgram/ManagedStripped";
#elif UNITY_ANDROID
            return $"{ProjectDir}/Library/Bee/artifacts/Android/ManagedStripped";
#elif UNITY_IOS
            return $"{ProjectDir}/Library/PlayerDataCache/iOS/Data/Managed";
#elif UNITY_WEBGL
            return $"{ProjectDir}/Library/Bee/artifacts/WebGL/ManagedStripped";
#else
            throw new NotSupportedException("GetOriginBuildStripAssembliesDir");
#endif
#else
            return target == BuildTarget.Android ?
                $"{ProjectDir}/Temp/StagingArea/assets/bin/Data/Managed" :
                $"{ProjectDir}/Temp/StagingArea/Data/Managed/";
#endif
        }

        public static string GetAssetBundleOutputDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleOutputDir}/{target}";
        }

        public static string GetAssetBundleTempDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleSourceDataTempDir}/{target}";
        }

    }
}
