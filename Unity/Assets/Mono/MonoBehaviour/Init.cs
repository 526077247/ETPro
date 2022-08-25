using System;
using System.Collections;
using YooAsset;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
namespace ET
{
	// 1 mono模式 2 ILRuntime模式 3 mono热重载模式
	public enum CodeMode
	{
		Mono = 1,
		Reload = 2,
		Wolong = 3,
	}
	
	public class Init: MonoBehaviour
	{
		public CodeMode CodeMode = CodeMode.Mono;

		public YooAssets.EPlayMode PlayMode = YooAssets.EPlayMode.EditorSimulateMode;

		private bool IsInit = false;
		private void Awake()
		{
			StartCoroutine(AwakeAsync());
		}
		private IEnumerator AwakeAsync()
		{
#if !UNITY_EDITOR && !FORCE_UPDATE //编辑器模式下跳过更新
			Define.Networked = Application.internetReachability != NetworkReachability.NotReachable;
#endif
			
#if UNITY_EDITOR
			// 编辑器下的模拟模式
			if (PlayMode == YooAssets.EPlayMode.EditorSimulateMode)
			{
				YooAssetsMgr.Instance.Init(YooAssets.EPlayMode.EditorSimulateMode);
				var createParameters = new YooAssets.EditorSimulateModeParameters();
				createParameters.LocationServices = new AddressByPathLocationServices("Assets/AssetsPackage");
				//createParameters.SimulatePatchManifestPath = GetPatchManifestPath();
				yield return YooAssets.InitializeAsync(createParameters);
			}
			else
#endif
			// 单机运行模式
			if (PlayMode == YooAssets.EPlayMode.OfflinePlayMode)
			{
				YooAssetsMgr.Instance.Init(YooAssets.EPlayMode.OfflinePlayMode);
				var createParameters = new YooAssets.OfflinePlayModeParameters();
				createParameters.LocationServices = new AddressByPathLocationServices("Assets/AssetsPackage");
				yield return YooAssets.InitializeAsync(createParameters);
			}
			// 联机运行模式
			else
			{
				YooAssetsMgr.Instance.Init(YooAssets.EPlayMode.HostPlayMode);
				var createParameters = new YooAssets.HostPlayModeParameters();
				createParameters.LocationServices = new AddressByPathLocationServices("Assets/AssetsPackage");
				createParameters.DecryptionServices = new BundleDecryption();
				createParameters.ClearCacheWhenDirty = true;
				createParameters.DefaultHostServer = YooAssetsMgr.Instance.Config.RemoteCdnUrl+"/"+YooAssetsMgr.Instance.Config.Channel+"_"+PlatformUtil.GetStrPlatformIgnoreEditor();
				createParameters.FallbackHostServer = YooAssetsMgr.Instance.Config.RemoteCdnUrl2+"/"+YooAssetsMgr.Instance.Config.Channel+"_"+PlatformUtil.GetStrPlatformIgnoreEditor();
				createParameters.VerifyLevel = EVerifyLevel.High;
				yield return YooAssets.InitializeAsync(createParameters);

				// 先设置更新补丁清单
				UpdateManifestOperation operation2 = YooAssets.WeaklyUpdateManifestAsync(YooAssetsMgr.Instance.staticVersion);
				yield return operation2;
			}

			InitUnitySetting();
			

#if ENABLE_IL2CPP
			this.CodeMode = CodeMode.Wolong;
#else
			this.CodeMode = CodeMode.Mono;
#endif

			System.AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};
			
			SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Instance);
			
			DontDestroyOnLoad(gameObject);

			ETTask.ExceptionHandler += Log.Error;

			Log.ILog = new UnityLogger();

			Options.Instance = new Options();

			CodeLoader.Instance.CodeMode = this.CodeMode;
			Options.Instance.Develop = 1;
			Options.Instance.LogLevel = 0;
			IsInit = true;
			CodeLoader.Instance.Start();
		}

		private void Start()
		{
			
		}

		private void Update()
		{
			if (!IsInit) return;
			CodeLoader.Instance.Update?.Invoke();
			if (CodeLoader.Instance.isReStart)
			{
				StartCoroutine(ReStart());
			}
		}

		public IEnumerator ReStart()
		{
			CodeLoader.Instance.isReStart = false;
			// 先设置更新补丁清单
			UpdateManifestOperation operation2 = YooAssets.WeaklyUpdateManifestAsync(YooAssetsMgr.Instance.staticVersion);
			yield return operation2;
			Log.Debug("ReStart");
			CodeLoader.Instance.OnApplicationQuit();
			CodeLoader.Instance.Dispose();
			CodeLoader.Instance.Start();
		}

		private void LateUpdate()
		{
			CodeLoader.Instance.LateUpdate?.Invoke();
		}

		private void OnApplicationQuit()
		{
			CodeLoader.Instance.OnApplicationQuit();
			CodeLoader.Instance.Dispose();
		}
		
		// 一些unity的设置项目
		void InitUnitySetting()
		{
			Input.multiTouchEnabled = false;
			//设置帧率
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
			Application.runInBackground = true;
		}
	}
}