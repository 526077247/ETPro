﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace YooAsset
{
	/// <summary>
	/// 更新清单操作
	/// </summary>
	public abstract class UpdateManifestOperation : AsyncOperationBase
	{
		/// <summary>
		/// 是否发现了新的补丁清单
		/// </summary>
		public bool FoundNewManifest { protected set; get; }
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal sealed class EditorPlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的更新清单操作
	/// </summary>
	internal sealed class OfflinePlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 网络模式的更新清单操作
	/// </summary>
	internal sealed class HostPlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			None,
			LoadWebManifestHash,
			CheckWebManifestHash,
			LoadWebManifest,
			CheckWebManifest,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private static int RequestCount = 0;
		private readonly HostPlayModeImpl _impl;
		private readonly int _resourceVersion;
		private readonly int _timeout;
		private ESteps _steps = ESteps.None;
		private UnityWebDataRequester _downloader1;
		private UnityWebDataRequester _downloader2;
		private VerifyManager _verifyManager = new VerifyManager();
		private float _verifyTime;

		internal HostPlayModeUpdateManifestOperation(HostPlayModeImpl impl, int resourceVersion, int timeout)
		{
			_impl = impl;
			_resourceVersion = resourceVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadWebManifestHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadWebManifestHash)
			{
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestHashFileName(_resourceVersion));
				YooLogger.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloader1 = new UnityWebDataRequester();
				_downloader1.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifestHash;
			}

			if (_steps == ESteps.CheckWebManifestHash)
			{
				if (_downloader1.IsDone() == false)
					return;

				// Check error
				if (_downloader1.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader1.GetError();
				}
				else
				{
					string webManifestHash = _downloader1.GetText();
					string cachedManifestHash = GetSandboxPatchManifestFileHash(_resourceVersion);

					// 如果补丁清单文件的哈希值相同
					if (cachedManifestHash == webManifestHash)
					{
						YooLogger.Log($"Patch manifest file hash is not change : {webManifestHash}");
						LoadSandboxPatchManifest(_resourceVersion);
						FoundNewManifest = false;
						_steps = ESteps.InitVerifyingCache;
					}
					else
					{
						YooLogger.Log($"Patch manifest hash is change : {webManifestHash} -> {cachedManifestHash}");
						FoundNewManifest = true;
						_steps = ESteps.LoadWebManifest;
					}
				}
				_downloader1.Dispose();
			}

			if (_steps == ESteps.LoadWebManifest)
			{
				string webURL = GetPatchManifestRequestURL(YooAssetSettingsData.GetPatchManifestFileName(_resourceVersion));
				YooLogger.Log($"Beginning to request patch manifest : {webURL}");
				_downloader2 = new UnityWebDataRequester();
				_downloader2.SendRequest(webURL, _timeout);
				_steps = ESteps.CheckWebManifest;
			}

			if (_steps == ESteps.CheckWebManifest)
			{
				if (_downloader2.IsDone() == false)
					return;

				// Check error
				if (_downloader2.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader2.GetError();
				}
				else
				{
					// 解析补丁清单			
					if (ParseAndSaveRemotePatchManifest(_resourceVersion, _downloader2.GetText()))
					{
						_steps = ESteps.InitVerifyingCache;
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"URL : {_downloader2.URL} Error : remote patch manifest content is invalid";
					}
				}
				_downloader2.Dispose();
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				_verifyManager.InitVerifyingCache(_impl.AppPatchManifest, _impl.LocalPatchManifest, false);
				_verifyTime = UnityEngine.Time.realtimeSinceStartup;
				_steps = ESteps.UpdateVerifyingCache;
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _verifyManager.GetVerifyProgress();
				if (_verifyManager.UpdateVerifyingCache())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_verifyManager.VerifySuccessCount}, Fail {_verifyManager.VerifyFailCount}, Elapsed time {costTime} seconds");
				}
			}
		}

		/// <summary>
		/// 获取补丁清单请求地址
		/// </summary>
		private string GetPatchManifestRequestURL(string fileName)
		{
			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				return _impl.GetPatchDownloadFallbackURL(fileName);
			else
				return _impl.GetPatchDownloadMainURL(fileName);
		}

		/// <summary>
		/// 解析并保存远端请求的补丁清单
		/// </summary>
		private bool ParseAndSaveRemotePatchManifest(int updateResourceVersion, string content)
		{
			try
			{
				var remotePatchManifest = PatchManifest.Deserialize(content);
				_impl.SetLocalPatchManifest(remotePatchManifest);

				YooLogger.Log("Save remote patch manifest file.");
				string savePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
				PatchManifest.Serialize(savePath, remotePatchManifest);
				return true;
			}
			catch (Exception e)
			{
				YooLogger.Error(e.ToString());
				return false;
			}
		}

		/// <summary>
		/// 加载沙盒内的补丁清单
		/// 注意：在加载本地补丁清单之前，已经验证过文件的哈希值
		/// </summary>
		private void LoadSandboxPatchManifest(int updateResourceVersion)
		{
			YooLogger.Log("Load sandbox patch manifest file.");
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
			string jsonData = File.ReadAllText(filePath);
			var sandboxPatchManifest = PatchManifest.Deserialize(jsonData);
			_impl.SetLocalPatchManifest(sandboxPatchManifest);
		}

		/// <summary>
		/// 获取沙盒内补丁清单文件的哈希值
		/// 注意：如果沙盒内补丁清单文件不存在，返回空字符串
		/// </summary>
		private string GetSandboxPatchManifestFileHash(int updateResourceVersion)
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
			if (File.Exists(filePath))
				return HashUtility.FileMD5(filePath);
			else
				return string.Empty;
		}
	}

	/// <summary>
	/// 网络模式的更新清单操作（弱联网）
	/// </summary>
	internal sealed class HostPlayModeWeaklyUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			None,
			LoadSandboxManifestHash,
			InitVerifyingCache,
			UpdateVerifyingCache,
			Done,
		}

		private readonly HostPlayModeImpl _impl;
		private readonly int _resourceVersion;
		private ESteps _steps = ESteps.None;
		private VerifyManager _verifyManager = new VerifyManager();
		private float _verifyTime;

		internal HostPlayModeWeaklyUpdateManifestOperation(HostPlayModeImpl impl, int resourceVersion)
		{
			_impl = impl;
			_resourceVersion = resourceVersion;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadSandboxManifestHash;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadSandboxManifestHash)
			{
				LoadSandboxPatchManifest(_resourceVersion);
				_steps = ESteps.InitVerifyingCache;
			}

			if (_steps == ESteps.InitVerifyingCache)
			{
				if (_verifyManager.InitVerifyingCache(_impl.AppPatchManifest, _impl.LocalPatchManifest, true))
				{
					_verifyTime = UnityEngine.Time.realtimeSinceStartup;
					_steps = ESteps.UpdateVerifyingCache;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"The resource version {_resourceVersion} content is not complete !";
				}
			}

			if (_steps == ESteps.UpdateVerifyingCache)
			{
				Progress = _verifyManager.GetVerifyProgress();
				if (_verifyManager.UpdateVerifyingCache())
				{
					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyTime;
					YooLogger.Log($"Verify result : Success {_verifyManager.VerifySuccessCount}, Fail {_verifyManager.VerifyFailCount}, Elapsed time {costTime} seconds");
					if (_verifyManager.VerifyFailCount > 0)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"The resource version {_resourceVersion} content has verify failed file !";
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
			}
		}

		/// <summary>
		/// 加载沙盒内的补丁清单
		/// 注意：在加载本地补丁清单之前，未验证过文件的哈希值
		/// </summary>
		private void LoadSandboxPatchManifest(int updateResourceVersion)
		{
			string filePath = PathHelper.MakePersistentLoadPath(YooAssetSettingsData.GetPatchManifestFileName(updateResourceVersion));
			if (File.Exists(filePath))
			{
				YooLogger.Log("Load sandbox patch manifest file.");
				string jsonData = File.ReadAllText(filePath);
				var sandboxPatchManifest = PatchManifest.Deserialize(jsonData);
				_impl.SetLocalPatchManifest(sandboxPatchManifest);
			}
		}
	}

	/// <summary>
	/// 本地缓存文件验证管理器
	/// </summary>
	internal class VerifyManager
	{
		private class ThreadInfo
		{
			public bool Result = false;
			public string FilePath { private set; get; }
			public PatchBundle Bundle { private set; get; }
			public ThreadInfo(string filePath, PatchBundle bundle)
			{
				FilePath = filePath;
				Bundle = bundle;
			}
		}

		private readonly List<PatchBundle> _waitingList = new List<PatchBundle>(1000);
		private readonly List<PatchBundle> _verifyingList = new List<PatchBundle>(100);
		private readonly ThreadSyncContext _syncContext = new ThreadSyncContext();
		private int _verifyMaxNum = 32;
		private int _verifyTotalCount = 0;

		public int VerifySuccessCount { private set; get; } = 0;
		public int VerifyFailCount { private set; get; } = 0;

		public bool InitVerifyingCache(PatchManifest appPatchManifest, PatchManifest localPatchManifest, bool weaklyUpdate)
		{
			// 遍历所有文件然后验证并缓存合法文件
			foreach (var patchBundle in localPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (appPatchManifest.TryGetPatchBundle(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				// 注意：在弱联网模式下，我们需要验证指定资源版本的所有资源完整性
				if (weaklyUpdate)
				{
					string filePath = SandboxHelper.MakeCacheFilePath(patchBundle.FileName);
					if (File.Exists(filePath))
						_waitingList.Add(patchBundle);
					else
						return false;
				}
				else
				{
					string filePath = SandboxHelper.MakeCacheFilePath(patchBundle.FileName);
					if (File.Exists(filePath))
						_waitingList.Add(patchBundle);
				}
			}

			// 设置同时验证的最大数
			ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
			YooLogger.Log($"Work threads : {workerThreads}, IO threads : {ioThreads}");
			_verifyMaxNum = Math.Min(workerThreads, ioThreads);
			_verifyTotalCount = _waitingList.Count;
			return true;
		}
		public bool UpdateVerifyingCache()
		{
			_syncContext.Update();

			if (_waitingList.Count == 0 && _verifyingList.Count == 0)
				return true;

			if (_verifyingList.Count >= _verifyMaxNum)
				return false;

			for (int i = _waitingList.Count - 1; i >= 0; i--)
			{
				if (_verifyingList.Count >= _verifyMaxNum)
					break;

				var patchBundle = _waitingList[i];
				if (RunThread(patchBundle))
				{
					_waitingList.RemoveAt(i);
					_verifyingList.Add(patchBundle);
				}
				else
				{
					YooLogger.Warning("The thread pool is failed queued.");
					break;
				}
			}

			return false;
		}
		public float GetVerifyProgress()
		{
			if (_verifyTotalCount == 0)
				return 1f;
			return (float)(VerifySuccessCount + VerifyFailCount) / _verifyTotalCount;
		}

		private bool RunThread(PatchBundle patchBundle)
		{
			string filePath = SandboxHelper.MakeCacheFilePath(patchBundle.FileName);
			ThreadInfo info = new ThreadInfo(filePath, patchBundle);
			return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyInThread), info);
		}
		private void VerifyInThread(object infoObj)
		{
			ThreadInfo info = (ThreadInfo)infoObj;
			info.Result = DownloadSystem.CheckContentIntegrity(info.FilePath, info.Bundle.SizeBytes, info.Bundle.CRC);
			_syncContext.Post(VerifyCallback, info);
		}
		private void VerifyCallback(object obj)
		{
			ThreadInfo info = (ThreadInfo)obj;
			if (info.Result)
			{
				VerifySuccessCount++;
				DownloadSystem.CacheVerifyFile(info.Bundle.Hash, info.Bundle.FileName);
			}
			else
			{
				VerifyFailCount++;

				// NOTE：不期望删除断点续传的资源文件
				/*
				YooLogger.Warning($"Failed to verify file : {info.FilePath}");
				if (File.Exists(info.FilePath))
					File.Delete(info.FilePath);
				*/
			}
			_verifyingList.Remove(info.Bundle);
		}
	}
}