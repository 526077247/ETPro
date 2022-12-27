using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace ET
{
	public class CodeLoader
	{
		[IgnoreDataMember]//防裁剪
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action FrameFinishUpdate;
		public Action OnApplicationQuit;

		private int assemblyVer;
		private Assembly assembly;

		public CodeMode CodeMode { get; set; }

		private MemoryStream assStream;
		private MemoryStream pdbStream;
		private byte[] optionBytes;//todo：dhe

		public bool IsInit = false;

		// 所有mono的类型
		private readonly Dictionary<string, Type> monoTypes = new Dictionary<string, Type>();

		// 热更层的类型
		private readonly Dictionary<string, Type> hotfixTypes = new Dictionary<string, Type>();

		public static List<string> AllAotDllList
		{
			get
			{
				var res = new List<string>();
				res.AddRange(SystemAotDllList);
				res.AddRange(UserAotDllList);
				return res;
			}
		}
		public static string[] SystemAotDllList = {
			"mscorlib.dll", 
			"System.dll", 
			"System.Core.dll"
		};
		public static string[] UserAotDllList = {
			"Unity.ThirdParty.dll",
			"Unity.Mono.dll"
		};
		/// <summary>
		/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
		/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
		/// </summary>
		public void LoadMetadataForAOTAssembly()
		{
			if(this.CodeMode!=CodeMode.Wolong) return;
			// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
			// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
			AssetBundle ab = null;
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
			{
				ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/aot.bundle");
				// optionBytes = ((TextAsset) ab.LoadAsset($"{Define.AOTDir}Unity.Codes.dhao.bytes", typeof (TextAsset)))?.bytes;
			}
// #if UNITY_EDITOR
// 			else
// 				optionBytes = (AssetDatabase.LoadAssetAtPath($"{Define.AOTDir}Unity.Codes.dhao.bytes", typeof(TextAsset)) as TextAsset)?.bytes;
// #endif
			foreach (var aotDllName in AllAotDllList)
			{
				byte[] dllBytes = null;
				if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
					dllBytes = ((TextAsset)ab.LoadAsset($"{Define.AOTDir}{aotDllName}.bytes", typeof(TextAsset))).bytes;
#if UNITY_EDITOR
				else
					dllBytes = (AssetDatabase.LoadAssetAtPath($"{Define.AOTDir}{aotDllName}.bytes", typeof(TextAsset)) as TextAsset).bytes;
#endif
				var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes,HomologousImageMode.SuperSet);
				Log.Info($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
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
					AssetBundle ab = null;
					byte[] assBytes = null;
					byte[] pdbBytes = null;
					//第一次启动用AOT或者加载dhao
					if (!this.IsInit)
					{
#if !UNITY_EDITOR
						bool isLoadAot = YooAssetsMgr.Instance.IsDllBuildIn;//dll和aot版本相同
						if (this.optionBytes != null)//打了dhao
						{
							// GetBytes(out ab, out assBytes, out pdbBytes);
							//todo: 
							// RuntimeApi.UseDifferentialHybridAOTAssembly("Unity.Codes.dll");
							// var err = RuntimeApi.LoadDifferentialHybridAssembly(assBytes, this.optionBytes);
							// Log.Info($"LoadDifferentialHybridAssembly:Unity.Codes. ret:{err}");
							// isLoadAot = true;//通过dhe技术
						}

						if (isLoadAot)
						{
							foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
							{
								if (item.FullName.Contains("Unity.Codes"))
								{
									assembly = item;
									break;
								}
							}
						}
#endif
					}
					else//热更完
					{
						if (this.assemblyVer != YooAssetsMgr.Instance.Config.Dllver)//dll版本不同
						{
							this.assembly = null;
						}
					}
					//没有内置AOTdll，或者热更完dll版本不同
					if(this.assembly == null)
					{
						GetBytes(out ab, out assBytes, out pdbBytes);
						assembly = Assembly.Load(assBytes, pdbBytes);
						Debug.Log("Get Dll Success");
					}
					foreach (Type type in this.assembly.GetTypes())
					{
						this.monoTypes[type.FullName] = type;
						this.hotfixTypes[type.FullName] = type;
					}
					this.assemblyVer = YooAssetsMgr.Instance.Config.Dllver;//记录当前dll版本
					IStaticAction start = new MonoStaticAction(assembly, "ET.Entry", "Start");
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
					IStaticAction start = new MonoStaticAction(assembly, "ET.Entry", "Start");
					start.Run();
					break;
				}
			}

			IsInit = true;
		}

		private void GetBytes(out AssetBundle ab,out byte[] assBytes,out byte[] pdbBytes)
		{
			assBytes = null;
			pdbBytes= null;
			ab = null;
			if (YooAssets.PlayMode != YooAssets.EPlayMode.EditorSimulateMode)
			{
				ab = YooAssetsMgr.Instance.SyncLoadAssetBundle("assets/assetspackage/code/hotfix.bundle");
				assBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.dll.bytes",
					typeof (TextAsset))).bytes;
				pdbBytes = ((TextAsset) ab.LoadAsset($"{Define.HotfixDir}Code{YooAssetsMgr.Instance.Config.Dllver}.pdb.bytes",
					typeof (TextAsset))).bytes;
			}
#if UNITY_EDITOR
			else
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				var obj = JsonHelper.FromJson<BuildConfig>(jstr);
				int version = obj.Dllver;
				assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes", typeof (TextAsset)) as TextAsset)
						.bytes;
				pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes", typeof (TextAsset)) as TextAsset)
						.bytes;
			}
#endif
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
			Log.Debug("ReStart");
			isReStart = true;
		}

		public Dictionary<string, EntityView> GetAllEntitys()
		{
			IStaticFunc<Dictionary<string, EntityView>> GetAllEntitys = 
					new MonoStaticFunc<Dictionary<string, EntityView>>(assembly, "ET.ViewEditorHelper", "GetAllEntitys");
			return GetAllEntitys.Run();
		}

		public EntityData GetEntityData()
		{
			IStaticFunc<EntityData> GetEntityData = new MonoStaticFunc<EntityData>(assembly, "ET.ViewEditorHelper", "GetEntityData");
			return GetEntityData.Run();
		}
	}
}