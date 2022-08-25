using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using UnityEngine;
using YooAsset;

namespace ET
{
	public class CodeLoader
	{
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action OnApplicationQuit;

		private Assembly assembly;
		
		public CodeMode CodeMode { get; set; }
		
		private MemoryStream assStream ;
		private MemoryStream pdbStream ;
		
		// 所有mono的类型
		private readonly Dictionary<string, Type> monoTypes = new Dictionary<string, Type>();
		
		// 热更层的类型
		private readonly Dictionary<string, Type> hotfixTypes = new Dictionary<string, Type>();
		

		public static string[] aotDllList = {
			"Unity.ThirdParty.dll",
			"Unity.Mono.dll",
			"mscorlib.dll",
			"System.dll",
			"System.Core.dll", // 如果使用了Linq，需要这个
			// "Newtonsoft.Json.dll",
			// "protobuf-net.dll",
			// "Google.Protobuf.dll",
			// "MongoDB.Bson.dll",
			// "DOTween.Modules.dll",
			// "UniTask.dll",
		};
		/// <summary>
		/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
		/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
		/// </summary>
		public static unsafe void LoadMetadataForAOTAssembly()
		{
			// 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
			// 我们在BuildProcessor_xxx里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

			/// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
			/// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误

			AssetBundle ab = null;
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
				ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/aot.bundle");

			foreach (var aotDllName in aotDllList)
			{
				byte[] dllBytes;
				if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
					dllBytes = ((TextAsset)ab.LoadAsset($"{Define.AOTDir}{aotDllName}.bytes", typeof(TextAsset))).bytes;
#if UNITY_EDITOR
				else
					dllBytes = (AssetDatabase.LoadAssetAtPath($"{Define.AOTDir}{aotDllName}.bytes", typeof(TextAsset)) as TextAsset).bytes;
#endif

				fixed (byte* ptr = dllBytes)
				{
					try
					{
						// 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
						int err = HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly((IntPtr) ptr, dllBytes.Length);
						Log.Info($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
					}
					catch (Exception ex)
					{
						Log.Error($"LoadMetadataForAOTAssembly:{aotDllName} error");
					}
				}
			}
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
				ab?.Unload(true);

		}
		private CodeLoader()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly ass in assemblies)
			{
				foreach (Type type in ass.GetTypes())
				{
					this.monoTypes[type.FullName] = type;
					this.monoTypes[type.AssemblyQualifiedName] = type;
				}
			}
		}
		
		public Type GetMonoType(string fullName)
		{
			this.monoTypes.TryGetValue(fullName, out Type type);
			return type;
		}
		
		public Type GetHotfixType(string fullName)
		{
			this.hotfixTypes.TryGetValue(fullName, out Type type);
			return type;
		}

		public void Dispose()
		{

		}
		
		public void Start()
		{
			switch (this.CodeMode)
			{
				case CodeMode.Wolong:
				case CodeMode.Mono:
				{
					if(this.CodeMode==CodeMode.Wolong) LoadMetadataForAOTAssembly();
					byte[] assBytes = null;
					byte[] pdbBytes= null;
					AssetBundle ab = null;

					if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
					{
						ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/hotfix.bundle");
						assBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Resver}.dll.bytes", typeof (TextAsset))).bytes;
						pdbBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Resver}.pdb.bytes", typeof (TextAsset))).bytes;
					}
#if UNITY_EDITOR
					else
					{
						string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
						var obj = JsonHelper.FromJson<BuildConfig>(jstr);
						int version = obj.Resver;
						assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes", typeof (TextAsset)) as TextAsset).bytes;
						pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes", typeof (TextAsset)) as TextAsset).bytes;
					}
#endif

					assembly = Assembly.Load(assBytes, pdbBytes);
					foreach (Type type in this.assembly.GetTypes())
					{
						this.monoTypes[type.FullName] = type;
						this.hotfixTypes[type.FullName] = type;
					}
					IStaticMethod start = new MonoStaticMethod(assembly, "ET.Entry", "Start");
					start.Run();
					if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
						ab?.Unload(true);

					break;
				}
				case CodeMode.Reload:
				{
					byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Data.dll"));
					byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Data.pdb"));
					
					assembly = Assembly.Load(assBytes, pdbBytes);
					this.LoadLogic();
					IStaticMethod start = new MonoStaticMethod(assembly, "ET.Entry", "Start");
					start.Run();
					break;
				}
			}
		}

		// 热重载调用下面三个方法
		// CodeLoader.Instance.LoadLogic();
		// Game.EventSystem.Add(CodeLoader.Instance.GetTypes());
		// Game.EventSystem.Load();
		public void LoadLogic()
		{
			if (this.CodeMode != CodeMode.Reload)
			{
				throw new Exception("CodeMode != Reload!");
			}
			
			// 傻屌Unity在这里搞了个傻逼优化，认为同一个路径的dll，返回的程序集就一样。所以这里每次编译都要随机名字
			string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Logic_*.dll");
			if (logicFiles.Length != 1)
			{
				throw new Exception("Logic dll count != 1");
			}

			string logicName = Path.GetFileNameWithoutExtension(logicFiles[0]);
			byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.dll"));
			byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, $"{logicName}.pdb"));

			Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);
			
			foreach (Type type in this.assembly.GetTypes())
			{
				this.monoTypes[type.FullName] = type;
				this.hotfixTypes[type.FullName] = type;
			}
			
			foreach (Type type in hotfixAssembly.GetTypes())
			{
				this.monoTypes[type.FullName] = type;
				this.hotfixTypes[type.FullName] = type;
			}
		}

		public Dictionary<string, Type> GetHotfixTypes()
		{
			return this.hotfixTypes;
		}

		public bool isReStart = false;
		public void ReStart()
		{
			YooAssets.ForceUnloadAllAssets();
			YooAssetsMgr.Instance.Init(YooAssets.PlayMode);
			Log.Debug("ReStart");
			isReStart = true;
		}
	}
}