using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HybridCLR;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace ET
{
    public static class BuildAssemblieEditor
    {
        private static bool IsBuildCodeAuto;
        [MenuItem("Tools/Build/EnableAutoBuildCodeDebug _F1")]
        public static void SetAutoBuildCode()
        {
            PlayerPrefs.SetInt("AutoBuild", 1);
            ShowNotification("AutoBuildCode Enabled");
        }
        
        [MenuItem("Tools/Build/DisableAutoBuildCodeDebug _F2")]
        public static void CancelAutoBuildCode()
        {
            PlayerPrefs.DeleteKey("AutoBuild");
            ShowNotification("AutoBuildCode Disabled");
        }

        public static void BuildCodeAuto()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<BuildConfig>(jstr);
            string assemblyName = "Code" + config.Resver;
            BuildAssemblieEditor.BuildMuteAssembly(assemblyName, new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
                "Codes/Hotfix/",
                "Codes/HotfixView/"
            }, Array.Empty<string>(), CodeOptimization.Debug,true);
            
        }
        [MenuItem("Tools/Build/BuildCodeDebug _F5")]
        public static void BuildCodeDebug()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<BuildConfig>(jstr);
            string assemblyName = "Code" + config.Resver;
            BuildAssemblieEditor.BuildMuteAssembly(assemblyName, new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
                "Codes/Hotfix/",
                "Codes/HotfixView/"
            }, Array.Empty<string>(), CodeOptimization.Debug);

            AfterCompiling(assemblyName);
            
        }
        
        [MenuItem("Tools/Build/BuildCodeRelease _F6")]
        public static void BuildCodeRelease()
        {
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<BuildConfig>(jstr);
            string assemblyName = "Code" + config.Resver;
            BuildAssemblieEditor.BuildMuteAssembly(assemblyName, new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
                "Codes/Hotfix/",
                "Codes/HotfixView/"
            }, Array.Empty<string>(),HybridCLR.HybridCLR.IsWolong? CodeOptimization.Debug:CodeOptimization.Release);

            AfterCompiling(assemblyName);

        }
        
        [MenuItem("Tools/Build/BuildData _F7")]
        public static void BuildData()
        {
            BuildAssemblieEditor.BuildMuteAssembly("Data", new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
            }, Array.Empty<string>(), CodeOptimization.Debug);
        }
        
        
        [MenuItem("Tools/Build/BuildLogic _F8")]
        public static void BuildLogic()
        {
            string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Logic_*");
            foreach (string file in logicFiles)
            {
                File.Delete(file);
            }
            
            int random = RandomHelper.RandomNumber(100000000, 999999999);
            string logicFile = $"Logic_{random}";
            
            BuildAssemblieEditor.BuildMuteAssembly(logicFile, new []
            {
                "Codes/Hotfix/",
                "Codes/HotfixView/",
            }, new[]{Path.Combine(Define.BuildOutputDir, "Data.dll")}, CodeOptimization.Debug);
        }

        public static void BuildAOT()
        {
            if(!HybridCLR.HybridCLR.Setup())return;
            #region 防裁剪
            FileHelper.CopyDirectory("Codes", "Assets/Codes/Temp");
            AssetDatabase.Refresh();
            #endregion

            PlatformType activePlatform = PlatformType.None;
#if UNITY_ANDROID
			activePlatform = PlatformType.Android;
#elif UNITY_IOS
			activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
            activePlatform = PlatformType.PC;
#elif UNITY_STANDALONE_OSX
			activePlatform = PlatformType.MacOS;
#else
			activePlatform = PlatformType.None;
#endif
            
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "ET";
            string exeName = programName;
            string platform = "";
            switch (activePlatform)
            {
                case PlatformType.PC:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    // IFixEditor.Patch();
                    platform = "pc";
                    break;
                case PlatformType.Android:
                    BuildHelper.KeystoreSetting();
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

                
            // MethodBridgeHelper.MethodBridge_All();
            AssetDatabase.Refresh();
            string[] levels = {
                "Assets/AssetsPackage/Scenes/InitScene/Init.unity",
            };
            UnityEngine.Debug.Log("开始EXE打包");
            string relativeDirPrefix = "../Temp";
            if (!Directory.Exists(relativeDirPrefix))
            {
                Directory.CreateDirectory(relativeDirPrefix);
            }
            BuildPipeline.BuildPlayer(levels, $"{relativeDirPrefix}/{exeName}", buildTarget, BuildOptions.None);
            UnityEngine.Debug.Log("完成exe打包");
            for (int i = 0; i < CodeLoader.aotDllList.Length; i++)
            {
                var assemblyName = CodeLoader.aotDllList[i];
                File.Copy(Path.Combine(HybridCLR.BuildConfig.GetAssembliesPostIl2CppStripDir(buildTarget), $"{assemblyName}"), Path.Combine(Define.AOTDir, $"{assemblyName}.bytes"), true);
            }
            
            #region 防裁剪
            Directory.Delete("Assets/Codes/Temp",true);
            File.Delete("Assets/Codes/Temp.meta");
            AssetDatabase.Refresh();
            #endregion
            
        }

        [MenuItem("Tools/Build/BuildAOT _F9")]
        public static void BuildAOTCode()
        {
            BuildAOT();
            //反射获取当前Game视图，提示编译完成
            ShowNotification("Build Code Success");
        }
        
        private static void BuildMuteAssembly(string assemblyName, string[] CodeDirectorys, string[] additionalReferences, CodeOptimization codeOptimization,bool isAuto = false)
        {
            List<string> scripts = new List<string>();
            for (int i = 0; i < CodeDirectorys.Length; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(CodeDirectorys[i]);
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    scripts.Add(fileInfos[j].FullName);
                }
            }
            if (!Directory.Exists(Define.BuildOutputDir))
                Directory.CreateDirectory(Define.BuildOutputDir);

            string dllPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb");
            File.Delete(dllPath);
            File.Delete(pdbPath);

            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(dllPath, scripts.ToArray());
            
            //启用UnSafe
            //assemblyBuilder.compilerOptions.AllowUnsafeCode = true;

            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            assemblyBuilder.compilerOptions.CodeOptimization = codeOptimization;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            // assemblyBuilder.compilerOptions.ApiCompatibilityLevel = ApiCompatibilityLevel.NET_4_6;

            assemblyBuilder.additionalReferences = additionalReferences;
            
            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            //AssemblyBuilderFlags.None                 正常发布
            //AssemblyBuilderFlags.DevelopmentBuild     开发模式打包
            //AssemblyBuilderFlags.EditorAssembly       编辑器状态
            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;

            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;

            assemblyBuilder.buildTargetGroup = buildTargetGroup;

            assemblyBuilder.buildStarted += delegate(string assemblyPath) { Debug.LogFormat("build start：" + assemblyPath); };

            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                IsBuildCodeAuto = false;
                int errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Warnings: {0} - Errors: {1}", warningCount, errorCount);

                if (warningCount > 0)
                {
                    Debug.LogFormat("有{0}个Warning!!!", warningCount);
                }

                if (errorCount > 0||warningCount > 0)
                {
                    for (int i = 0; i < compilerMessages.Length; i++)
                    {
                        if (compilerMessages[i].type == CompilerMessageType.Error||compilerMessages[i].type == CompilerMessageType.Warning)
                        {
                            Debug.LogError(compilerMessages[i].message);
                        }
                    }
                }
            };
            if (isAuto)
            {
                IsBuildCodeAuto = true;
                EditorApplication.CallbackFunction Update = null;
                Update = () =>
                {
                    if(IsBuildCodeAuto||EditorApplication.isCompiling) return;
                    EditorApplication.update -= Update;
                    AfterBuild(assemblyName);
                };
                EditorApplication.update += Update;
            }
            //开始构建
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("build fail：" + assemblyBuilder.assemblyPath);
                return;
            }
        }

        private static void AfterCompiling(string assemblyName)
        {
            while (EditorApplication.isCompiling)
            {
                Debug.Log("Compiling wait1");
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1000);
                Debug.Log("Compiling wait2");
            }
            AfterBuild(assemblyName);
            //反射获取当前Game视图，提示编译完成
            ShowNotification("Build Code Success");
        }
        
        public static void AfterBuild(string assemblyName)
        {
            Debug.Log("Compiling finish");
            EditorNotification.hasChange = false;
            Directory.CreateDirectory(Define.HotfixDir);
            FileHelper.CleanDirectory(Define.HotfixDir);
            File.Copy(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll"), Path.Combine(Define.HotfixDir, $"{assemblyName}.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb"), Path.Combine(Define.HotfixDir, $"{assemblyName}.pdb.bytes"), true);
            AssetDatabase.Refresh();

            Debug.Log("build success!");
        }

        public static void ShowNotification(string tips)
        {
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }
    }
    
}