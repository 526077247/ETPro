using System;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;

namespace ET
{
	public class CodeLoader
	{
		[IgnoreDataMember]//防裁剪
		public static CodeLoader Instance = new CodeLoader();

		public Action Update;
		public Action LateUpdate;
		public Action FixedUpdate;
		public Action OnApplicationQuit;
		public Action<bool> OnApplicationFocus;
		
		private bool loadAOT = false;
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
			"System.Core.dll",
			"UnityEngine.CoreModule.dll"
		};
		public static string[] UserAotDllList = {
			"Unity.ThirdParty.dll",
			"Unity.Mono.dll"
		};
		/// <summary>
		/// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
		/// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
		/// </summary>
		public async ETTask LoadMetadataForAOTAssembly(EPlayMode mode)
		{
			if(loadAOT) return;
			if(this.CodeMode != CodeMode.Wolong && this.CodeMode != CodeMode.LoadFromUrl) return;
			// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
			// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
			foreach (var aotDllName in AllAotDllList)
			{
				byte[] dllBytes = null;
#if UNITY_EDITOR
				if (mode != YooAsset.EPlayMode.EditorSimulateMode)
#endif
				{
					var op = PackageManager.Instance.LoadAssetAsync<TextAsset>($"{Define.AOTLoadDir}{aotDllName}.bytes",Define.DefaultName);
					await op.Task;
					TextAsset v = op.AssetObject as TextAsset;
					dllBytes = v.bytes;
					op.Release();
				}
#if UNITY_EDITOR
				else
					dllBytes = (AssetDatabase.LoadAssetAtPath($"{Define.AOTDir}{aotDllName}.bytes", TypeInfo<TextAsset>.Type) as TextAsset).bytes;
#endif

				var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes,HomologousImageMode.SuperSet);
				Log.Info($"LoadMetadataForAOTAssembly:{aotDllName}. ret:{err}");
			}
			loadAOT = true;
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
		
		public async ETTask Start()
		{
			if ((Define.Debug || Debug.isDebugBuild) && UnityEngine.PlayerPrefs.GetInt("DEBUG_LoadFromUrl", 0) == 1)
			{
				CodeMode = CodeMode.LoadFromUrl;
			}
			
			switch (this.CodeMode)
			{
				case CodeMode.BuildIn:
				{
					foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
					{
						if (item.FullName.Contains("Unity.Codes"))
						{
							assembly = item;
							Log.Info("Get AOT Dll Success");
							break;
						}
					}

					if (assembly == null)
					{
						Log.Error("Get AOT Dll Fail, 请将Init上的CodeMode改为LoadDll，或者在打包选项上开启热更代码打AOT");
					}
					foreach (Type type in this.assembly.GetTypes())
					{
						this.monoTypes[type.FullName] = type;
						this.hotfixTypes[type.FullName] = type;
					}
					break;
				}
				case CodeMode.Wolong:
				case CodeMode.LoadDll:
				{
					byte[] assBytes = null;
					byte[] pdbBytes = null;
					int version = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName);
					if (this.assemblyVer != version) //dll版本不同
					{
						assembly = null;
#if !UNITY_EDITOR
						//和内置包版本一致，检查是否有可用AOT代码
						if (PackageManager.Instance.CdnConfig.BuildHotfixAssembliesAOT &&
						    PackageManager.Instance.BuildInPackageConfig.GetBuildInPackageVersion(Define.DefaultName)
						    == version)
						{
							foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
							{
								if (item.FullName.Contains("Unity.Codes"))
								{
									assembly = item;
									Log.Info("Get AOT Dll Success");
									break;
								}
							}
						}
#endif
					}
					//没有内置AOTdll，或者热更完dll版本不同
					if (this.assembly == null)
					{
						await LoadMetadataForAOTAssembly(PackageManager.Instance.PlayMode);
						(assBytes, pdbBytes) = await GetBytes();
						if (assBytes != null)
						{
							assembly = Assembly.Load(assBytes, pdbBytes);
							Log.Info("Get Dll Success ! version=" + version);
						}
						else
						{
							Log.Error("Get Dll Fail");
						}
					}
					
					foreach (Type type in this.assembly.GetTypes())
					{
						this.monoTypes[type.FullName] = type;
						this.hotfixTypes[type.FullName] = type;
					}
					break;
				}
				case CodeMode.LoadFromUrl:
				{
					int version = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName);
					var path = UnityEngine.PlayerPrefs.GetString("DEBUG_LoadFromUrlPath", "http://127.0.0.1:8081/cdn/");
					path += $"Code{version}.dll.bytes";

					UnityWebRequest www = UnityWebRequest.Get(path);
					ETTask task = ETTask.Create();
					var op = www.SendWebRequest();
					op.completed += (a) => { task.SetResult(); };
					await task;
					if (www.result == UnityWebRequest.Result.Success)
					{
						await LoadMetadataForAOTAssembly(PackageManager.Instance.PlayMode);
						assembly = Assembly.Load(www.downloadHandler.data);
					}
					else
					{
						Log.Error("下载dll失败： url: " + path);
					}
					foreach (Type type in this.assembly.GetTypes())
					{
						this.monoTypes[type.FullName] = type;
						this.hotfixTypes[type.FullName] = type;
					}
					break;
				}
				case CodeMode.Reload:
				{
					byte[] assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Data.dll"));
					byte[] pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Data.pdb"));
					
					assembly = Assembly.Load(assBytes, pdbBytes);
					this.LoadLogic();
					break;
				}
			}

			if (assembly != null)
			{
				this.assemblyVer = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName); //记录当前dll版本
				IStaticAction start = new MonoStaticAction(assembly, "ET.Entry", "Start");
				start.Run();
				IsInit = true;
			}
			else
			{
				Log.Error("assembly == null");
			}
		}

		private async ETTask<(byte[], byte[])> GetBytes()
		{
			int version = PackageManager.Instance.Config.GetPackageMaxVersion(Define.DefaultName);
			byte[] assBytes = null, pdbBytes = null;
			if (PackageManager.Instance.PlayMode != EPlayMode.EditorSimulateMode)
			{
				var op = PackageManager.Instance.LoadAssetAsync<TextAsset>(
					$"{Define.HotfixLoadDir}Code{version}.dll.bytes", Define.DefaultName);
				await op.Task;
				assBytes = (op.AssetObject as TextAsset)?.bytes;
				op.Release();
				if (Define.Debug)
				{
					op = PackageManager.Instance.LoadAssetAsync<TextAsset>(
						$"{Define.HotfixLoadDir}Code{version}.pdb.bytes", Define.DefaultName);
					await op.Task;
					pdbBytes = (op.AssetObject as TextAsset)?.bytes;
					op.Release();
				}
			}
#if UNITY_EDITOR
			else
			{
				assBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.dll.bytes",
							TypeInfo<TextAsset>.Type) as TextAsset)
						.bytes;
				pdbBytes = (AssetDatabase.LoadAssetAtPath($"{Define.HotfixDir}Code{version}.pdb.bytes",
							TypeInfo<TextAsset>.Type) as TextAsset)
						.bytes;
			}
#endif
			return (assBytes, pdbBytes);
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