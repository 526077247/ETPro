using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HybridCLR;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Compilation;

namespace ET
{
    public static class BuildAssemblieEditor
    {
        private static bool IsBuildCodeAuto;
        [MenuItem("Tools/Build/EnableAutoBuildCodeDebug _F1",true)]
        public static bool SetAutoBuildCodeValidateFunction()
        {
            return PlayerPrefs.GetInt("AutoBuild", 0) == 0;
        }
        [MenuItem("Tools/Build/EnableAutoBuildCodeDebug _F1")]
        public static void SetAutoBuildCode()
        {
            PlayerPrefs.SetInt("AutoBuild", 1);
            ShowNotification("AutoBuildCode Enabled");
        }
        [MenuItem("Tools/Build/DisableAutoBuildCodeDebug _F2",true)]
        public static bool CancelAutoBuildCodeValidateFunction()
        {
            return PlayerPrefs.GetInt("AutoBuild", 0) == 1;
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
            string assemblyName = "Code" + config.Dllver;
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


            if (Directory.Exists("Assets/Codes/Temp"))
            {
                Directory.Delete("Assets/Codes/Temp", true);
                File.Delete("Assets/Codes/Temp.meta");
                AssetDatabase.Refresh();
            }

            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<BuildConfig>(jstr);
            string assemblyName = "Code" + config.Dllver;
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
            if (Directory.Exists("Assets/Codes/Temp"))
            {
                Directory.Delete("Assets/Codes/Temp", true);
                File.Delete("Assets/Codes/Temp.meta");
                AssetDatabase.Refresh();
            }
            
            string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
            var config = JsonHelper.FromJson<BuildConfig>(jstr);
            string assemblyName = "Code" + config.Dllver;
            BuildAssemblieEditor.BuildMuteAssembly(assemblyName, new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
                "Codes/Hotfix/",
                "Codes/HotfixView/"
            }, Array.Empty<string>(),CodeOptimization.Release);

            AfterCompiling(assemblyName);

        }
        
        [MenuItem("Tools/Build/BuildData _F7")]
        public static void BuildData()
        {
            if (Directory.Exists("Assets/Codes/Temp"))
            {
                Directory.Delete("Assets/Codes/Temp", true);
                File.Delete("Assets/Codes/Temp.meta");
                AssetDatabase.Refresh();
            }
            
            BuildAssemblieEditor.BuildMuteAssembly("Data", new []
            {
                "Codes/Model/",
                "Codes/ModelView/",
            }, Array.Empty<string>(), CodeOptimization.Debug);
        }
        
        
        [MenuItem("Tools/Build/BuildLogic _F8")]
        public static void BuildLogic()
        {
            if (Directory.Exists("Assets/Codes/Temp"))
            {
                Directory.Delete("Assets/Codes/Temp", true);
                File.Delete("Assets/Codes/Temp.meta");
                AssetDatabase.Refresh();
            }
            
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

        /// <summary>
        /// 获取裁剪后的系统AOT
        /// </summary>
        public static void BuildSystemAOT()
        {
           
            PlatformType activePlatform = PlatformType.None;
#if UNITY_ANDROID
			activePlatform = PlatformType.Android;
#elif UNITY_IOS
			activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
            activePlatform = PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
			activePlatform = PlatformType.MacOS;
#elif UNITY_STANDALONE_LINUX
            activePlatform = PlatformType.Linux;
#else
			activePlatform = PlatformType.None;
#endif
            
            BuildTarget buildTarget = BuildHelper.buildmap[activePlatform];
            BuildTargetGroup group = BuildHelper.buildGroupmap[activePlatform];
            if(!HybridCLR.HybridCLRHelper.Setup(group))return;
            
            #region 防裁剪
            FileHelper.CopyDirectory("Codes", "Assets/Codes/Temp");
            AssetDatabase.Refresh();
            #endregion

            string programName = "ET";
            PlayerSettings.SetScriptingBackend(group,ScriptingImplementation.IL2CPP);
            string relativeDirPrefix = "../Temp";
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
            {
                scenes = new string[] { "Assets/AssetsPackage/Scenes/InitScene/Init.unity" },
                locationPathName = $"{relativeDirPrefix}/{programName}",
                options = BuildOptions.None,
                target = buildTarget,
                targetGroup = group,
            };
            
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("开始EXE打包");
            
            if (Directory.Exists(relativeDirPrefix))
            {
                Directory.Delete(relativeDirPrefix,true);
            }
            Directory.CreateDirectory(relativeDirPrefix);
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            UnityEngine.Debug.Log("完成exe打包");
            try
            {
                for (int i = 0; i < CodeLoader.SystemAotDllList.Length; i++)
                {
                    var assemblyName = CodeLoader.SystemAotDllList[i];
                    File.Copy(
                        Path.Combine(HybridCLR.Editor.SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget),
                            $"{assemblyName}"), Path.Combine(Define.AOTDir, $"{assemblyName}.bytes"), true);
                }
            }
            catch (Exception ex)
            {
                //检查是否已开启IL2CPP
                Debug.LogError(ex);
            }
            
            #region 防裁剪
            Directory.Delete("Assets/Codes/Temp",true);
            File.Delete("Assets/Codes/Temp.meta");
            AssetDatabase.Refresh();
            #endregion
            
        }

        /// <summary>
        /// 获取没裁剪的AOT
        /// </summary>
        [MenuItem("Tools/Build/BuildUserAOT _F9")]
        public static void BuildUserAOT()
        {
            try
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                var buildDir = HybridCLR.Editor.SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
                var group = BuildPipeline.GetBuildTargetGroup(target);

                ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
                scriptCompilationSettings.group = group;
                scriptCompilationSettings.target = target;
                Directory.CreateDirectory(buildDir);
                ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
                // foreach (var ass in scriptCompilationResult.assemblies)
                // {
                //     //Debug.LogFormat("compile assemblies:{1}/{0}", ass, buildDir);
                // }
                Debug.Log("compile finish!!!");
                for (int i = 0; i < CodeLoader.UserAotDllList.Length; i++)
                {
                    var assemblyName = CodeLoader.UserAotDllList[i];
                    File.Copy(
                        Path.Combine(buildDir,
                            $"{assemblyName}"), Path.Combine(Define.AOTDir, $"{assemblyName}.bytes"), true);
                }
                ShowNotification("Build Code Success");
            }
            catch (Exception ex)
            {
                //檢查是否已開啟IL2CPP
                Debug.LogError(ex);
            }
        }

        [MenuItem("Tools/Build/BuildSystemAOT _F10")]
        public static void BuildAOTCode()
        {
            BuildSystemAOT();
            
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
            assemblyBuilder.compilerOptions.AllowUnsafeCode = PlayerSettings.allowUnsafeCode;
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
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning&&!m.message.Contains("CS0436"));

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
                            if(!compilerMessages[i].message.Contains("CS0436"))
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

        private static void AfterCompiling(string assemblyName,bool isAOT= false)
        {
            while (EditorApplication.isCompiling)
            {
                Debug.Log("Compiling wait1");
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1000);
                Debug.Log("Compiling wait2");
            }
            AfterBuild(assemblyName,isAOT);
            ShowNotification("Build Code Success");
        }
        
        public static void AfterBuild(string assemblyName,bool isAOT = false)
        {
            Debug.Log("Compiling finish");
            EditorNotification.hasChange = false;
            Directory.CreateDirectory(isAOT?Define.AOTDir:Define.HotfixDir);
            FileHelper.CleanDirectory(isAOT?Define.AOTDir:Define.HotfixDir);
            File.Copy(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll"), Path.Combine(isAOT?Define.AOTDir:Define.HotfixDir, $"{assemblyName}.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb"), Path.Combine(isAOT?Define.AOTDir:Define.HotfixDir, $"{assemblyName}.pdb.bytes"), true);
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