﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using YooAsset;
namespace ET
{
    public static class BuildHelper
    {
        public const string relativeDirPrefix = "../Release";

        static Dictionary<PlatformType, BuildTarget> buildmap = new Dictionary<PlatformType, BuildTarget>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTarget.Android },
            { PlatformType.PC , BuildTarget.StandaloneWindows64 },
            { PlatformType.IOS , BuildTarget.Android },
            { PlatformType.MacOS , BuildTarget.StandaloneOSX },
        };

        static Dictionary<PlatformType, BuildTargetGroup> buildGroupmap = new Dictionary<PlatformType, BuildTargetGroup>(PlatformTypeComparer.Instance)
        {
            { PlatformType.Android , BuildTargetGroup.Android },
            { PlatformType.PC , BuildTargetGroup.Standalone },
            { PlatformType.IOS , BuildTargetGroup.iOS },
            { PlatformType.MacOS , BuildTargetGroup.Standalone },
        };
        public static void KeystoreSetting()
        {
            PlayerSettings.Android.keystoreName = "ET.keystore";
            PlayerSettings.Android.keyaliasName = "et";
            PlayerSettings.keyaliasPass = "123456";
            PlayerSettings.keystorePass = "123456";
        }

        public static void Build(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder)
        {
            // EditorUserSettings.SetConfigValue(AddressableTools.is_packing, "1");
            if (buildmap[type] == EditorUserBuildSettings.activeBuildTarget)
            {
                //pack
                BuildHandle(type, buildOptions, isBuildExe,clearFolder);
            }
            else
            {
                EditorUserBuildSettings.activeBuildTargetChanged = delegate ()
                {
                    if (EditorUserBuildSettings.activeBuildTarget == buildmap[type])
                    {
                        //pack
                        BuildHandle(type, buildOptions, isBuildExe, clearFolder);
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
        private static void BuildInternal(BuildTarget buildTarget,bool isBuildExe)
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
            buildParameters.BuildinTags = "buildin";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.EnableAddressable = true;
            buildParameters.CopyBuildinTagFiles = true;
            buildParameters.EncryptionServices = new GameEncryption();
            buildParameters.CompressOption = ECompressOption.LZ4;
    
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
        static void BuildHandle(PlatformType type, BuildOptions buildOptions, bool isBuildExe,bool clearFolder)
        {
            
            
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "ET";
            string exeName = programName;
            string platform = "";
            switch (type)
            {
                case PlatformType.PC:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    // IFixEditor.Patch();
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    KeystoreSetting();
                    buildTarget = BuildTarget.Android;
                    exeName += ".apk";
                    // IFixEditor.CompileToAndroid();
                    platform = "android";
                    break;
                case PlatformType.IOS:
                    buildTarget = BuildTarget.iOS;
                    // IFixEditor.CompileToIOS();
                    platform = "ios";
                    break;
                case PlatformType.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    // IFixEditor.Patch();
                    platform = "pc";
                    break;
            }
            //打程序集
            BuildAssemblieEditor.BuildCodeRelease();
            //打AOT程序集
            if (isBuildExe)
            {
                BuildAssemblieEditor.BuildAOT();
            }
            
            // if (isInject)
            // {
            //     //Inject
            //     IFixEditor.InjectAssemblys();
            // }
            //处理图集资源
            // HandleAltas();
            //打ab
            BuildInternal(buildTarget, isBuildExe);

            if (clearFolder && Directory.Exists(relativeDirPrefix))
            {
                Directory.Delete(relativeDirPrefix, true);
                Directory.CreateDirectory(relativeDirPrefix);
            }
            else
            {
                Directory.CreateDirectory(relativeDirPrefix);
            }

            if (isBuildExe)
            {
                // MethodBridgeHelper.MethodBridge_All();
                #region 防裁剪
                FileHelper.CopyDirectory("Codes", "Assets/Codes/Temp");
                AssetDatabase.Refresh();
                #endregion
                
                AssetDatabase.Refresh();
                string[] levels = {
                    "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
                };
                UnityEngine.Debug.Log("开始EXE打包");
                BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, buildOptions);
                UnityEngine.Debug.Log("完成exe打包");
                
                #region 防裁剪
                Directory.Delete("Assets/Codes/Temp",true);
                File.Delete("Assets/Codes/Temp.meta");
                AssetDatabase.Refresh();
                #endregion
            }
            
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var obj = JsonHelper.FromJson<BuildConfig>(jstr);

            // var settings = AASUtility.GetSettings();
            string fold = $"{AssetBundleBuilderHelper.GetDefaultOutputRoot()}/{buildTarget}/{obj.Resver}";
            
            string targetPath = Path.Combine(relativeDirPrefix, $"{obj.Channel}_{platform}");
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            FileHelper.CleanDirectory(targetPath);
            FileHelper.CopyFiles(fold, targetPath);
            
            UnityEngine.Debug.Log("完成cdn资源打包");
            
        }
        
    }
}
