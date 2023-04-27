using System.Collections.Generic;
using System.IO;
using System.Text;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using YooAsset;
namespace ET
{
    public static class BuildHelper
    {
        public const string relativeDirPrefix = "../Release";

        public static readonly Dictionary<PlatformType, BuildTarget> buildmap = new Dictionary<PlatformType, BuildTarget>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTarget.Android },
            { PlatformType.Windows , BuildTarget.StandaloneWindows64 },
            { PlatformType.IOS , BuildTarget.iOS },
            { PlatformType.MacOS , BuildTarget.StandaloneOSX },
            { PlatformType.Linux , BuildTarget.StandaloneLinux64 },
        };

        public static readonly Dictionary<PlatformType, BuildTargetGroup> buildGroupmap = new Dictionary<PlatformType, BuildTargetGroup>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTargetGroup.Android },
            { PlatformType.Windows , BuildTargetGroup.Standalone },
            { PlatformType.IOS , BuildTargetGroup.iOS },
            { PlatformType.MacOS , BuildTargetGroup.Standalone },
            { PlatformType.Linux , BuildTargetGroup.Standalone },
        };
        public static void KeystoreSetting()
        {
            PlayerSettings.Android.keystoreName = "ET.keystore";
            PlayerSettings.Android.keyaliasName = "et";
            PlayerSettings.keyaliasPass = "123456";
            PlayerSettings.keystorePass = "123456";
        }

        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder,bool buildHotfixAssembliesAOT,bool buildResourceAll)
        {
            // EditorUserSettings.SetConfigValue(AddressableTools.is_packing, "1");
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildHandle(type, buildOptions, isBuildExe,clearFolder,buildHotfixAssembliesAOT,buildResourceAll);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate ()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildHandle(type, buildOptions, isBuildExe, clearFolder,buildHotfixAssembliesAOT,buildResourceAll);
                    }
                };
                if(buildGroupmap.TryGetValue(type,out var group))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, buildmap[type]);
                }
                else
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildmap[type]);
                }
               
            }
        }
        private static void BuildInternal(BuildTarget buildTarget,bool isBuildExe,bool isBuildAll)
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<BuildConfig>(jstr);
            int buildVersion = obj.Resver;
            Debug.Log($"开始构建 : {buildTarget}");

            // 命令行参数

            // 构建参数
            string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
            BuildParameters buildParameters = new BuildParameters();
            buildParameters.OutputRoot = defaultOutputRoot;
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline;
            buildParameters.SBPParameters = new BuildParameters.SBPBuildParameters();
            buildParameters.BuildMode = isBuildExe?EBuildMode.ForceRebuild:EBuildMode.IncrementalBuild;
            buildParameters.BuildVersion = buildVersion;
            string tags = isBuildAll?null:"buildin";
            if (!isBuildExe)
            {
                var PipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(defaultOutputRoot, buildTarget);
                var oldPatchManifest = AssetBundleBuilderHelper.GetOldPatchManifest(PipelineOutputDirectory);
                tags = oldPatchManifest.BuildinTags;
            }

            buildParameters.BuildinTags = tags;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.EnableAddressable = true;
            buildParameters.CopyBuildinTagFiles = true;
            buildParameters.EncryptionServices = new GameEncryption();
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.DisableWriteTypeTree = true;//禁止写入类型树结构（可以降低包体和内存并提高加载效率）
            
            // 执行构建
            AssetBundleBuilder builder = new AssetBundleBuilder();
            var buildResult = builder.Run(buildParameters);
            if (buildResult.Success)
                Debug.Log($"构建成功!");
        }
        public static void HandleAltas()
        {
            //清除图集
            AltasHelper.ClearAllAtlas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //生成图集
            AltasHelper.GeneratingAtlas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            

        }
        static void BuildHandle(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder,bool buildHotfixAssembliesAOT,bool buildResourceAll)
        {
            
            
            BuildTarget buildTarget = buildmap[type];
            string programName = "ET";
            string exeName = programName;
            string platform = "";
            switch (type)
            {
                case PlatformType.Windows:
                    exeName += ".exe";
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    exeName += ".apk";
                    platform = "android";
                    break;
                case PlatformType.IOS:
                    platform = "ios";
                    break;
                case PlatformType.MacOS:
                    platform = "pc";
                    break;
                case PlatformType.Linux:
                    buildTarget = BuildTarget.StandaloneLinux64;
                    platform = "pc";
                    break;
            }
            //打程序集(旧版本可以选择不更新整包，所以不管打不打aot，为了方便起见，热更dll都打进版本)
            FileHelper.CleanDirectory(Define.HotfixDir);
            BuildAssemblieEditor.BuildCodeRelease();
            //打AOT程序集
            if (isBuildExe)
            {
                BuildAssemblieEditor.BuildUserAOT();
            }

            //处理图集资源
            // HandleAltas();
            //打ab
            BuildInternal(buildTarget, isBuildExe,buildResourceAll);
            DirectoryInfo info = new DirectoryInfo(relativeDirPrefix);
            if (Directory.Exists(info.FullName))
            {
                if (clearFolder)
                {
                    FileHelper.CleanDirectory(info.FullName);
                }
            }
            else
            {
                Directory.CreateDirectory(info.FullName);
            }

            if (isBuildExe)
            {
                #region 防裁剪
                FileHelper.CopyDirectory("Codes", "Assets/Codes/Temp");
                AssetDatabase.Refresh();
                #endregion
                SettingsUtil.buildHotfixAssembliesAOT = buildHotfixAssembliesAOT;
                HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
                
                AssetDatabase.Refresh();
                string[] levels = {
                    "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
                };
                UnityEngine.Debug.Log("开始EXE打包");
                BuildPipeline.BuildPlayer(levels, $"{info.FullName}/{exeName}", buildTarget, buildOptions);
                UnityEngine.Debug.Log("完成exe打包");
                
                #region 防裁剪
                Directory.Delete("Assets/Codes/Temp",true);
                File.Delete("Assets/Codes/Temp.meta");
                AssetDatabase.Refresh();
                #endregion
            }
            
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<BuildConfig>(jstr);
            
            string fold = $"{AssetBundleBuilderHelper.GetDefaultOutputRoot()}/{buildTarget}/{obj.Resver}";
            
            string targetPath = Path.Combine(info.FullName, $"{obj.Channel}_{platform}");
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            FileHelper.CleanDirectory(targetPath);
            FileHelper.CopyFiles(fold, targetPath);
            
            UnityEngine.Debug.Log("完成cdn资源打包");
#if UNITY_EDITOR
            Application.OpenURL($"file:///{info.FullName}");
#endif
        }
        
    }
}
