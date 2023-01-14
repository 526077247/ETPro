﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
	internal sealed class AssetBundleFileLoader : AssetBundleLoaderBase
	{
		private enum ESteps
		{
			None = 0,
			Download,
			CheckDownload,
			LoadFile,
			CheckLoadFile,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private string _fileLoadPath;
		private bool _isWaitForAsyncComplete = false;
		private bool _isShowWaitForAsyncError = false;
		private DownloaderBase _downloader;
		private AssetBundleCreateRequest _createRequest;


		public AssetBundleFileLoader(BundleInfo bundleInfo) : base(bundleInfo)
		{
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public override void Update()
		{
			if (_steps == ESteps.Done)
				return;

			if (_steps == ESteps.None)
			{
				if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromRemote)
				{
					_steps = ESteps.Download;
					_fileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromStreaming)
				{
					_steps = ESteps.LoadFile;
					_fileLoadPath = MainBundleInfo.Bundle.StreamingFilePath;
				}
				else if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
				{
					_steps = ESteps.LoadFile;
					_fileLoadPath = MainBundleInfo.Bundle.CachedFilePath;
				}
				else
				{
					throw new System.NotImplementedException(MainBundleInfo.LoadMode.ToString());
				}
			}

			// 1. 从服务器下载
			if (_steps == ESteps.Download)
			{
				int failedTryAgain = int.MaxValue;
				_downloader = DownloadSystem.BeginDownload(MainBundleInfo, failedTryAgain);
				_steps = ESteps.CheckDownload;
			}

			// 2. 检测服务器下载结果
			if (_steps == ESteps.CheckDownload)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = _downloader.GetLastError();
				}
				else
				{
					_steps = ESteps.LoadFile;
				}
			}

			// 3. 加载AssetBundle
			if (_steps == ESteps.LoadFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(_fileLoadPath) == false)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"Not found assetBundle file : {_fileLoadPath}";
					YooLogger.Error(LastError);
					return;
				}
#endif

				// Load assetBundle file
				if (MainBundleInfo.Bundle.IsEncrypted)
				{
					if (AssetSystem.DecryptionServices == null)
						throw new Exception($"{nameof(AssetBundleFileLoader)} need {nameof(IDecryptionServices)} : {MainBundleInfo.Bundle.BundleName}");

					DecryptionFileInfo fileInfo = new DecryptionFileInfo();
					fileInfo.BundleName = MainBundleInfo.Bundle.BundleName;
					fileInfo.FileHash = MainBundleInfo.Bundle.FileHash;
					ulong offset = AssetSystem.DecryptionServices.GetFileOffset(fileInfo);
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(_fileLoadPath, 0, offset);
					else
						_createRequest = AssetBundle.LoadFromFileAsync(_fileLoadPath, 0, offset);
				}
				else
				{
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(_fileLoadPath);
					else
						_createRequest = AssetBundle.LoadFromFileAsync(_fileLoadPath);
				}
				_steps = ESteps.CheckLoadFile;
			}

			// 4. 检测AssetBundle加载结果
			if (_steps == ESteps.CheckLoadFile)
			{
				if (_createRequest != null)
				{
					if (_isWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						YooLogger.Warning("Suspend the main thread to load unity bundle.");
						CacheBundle = _createRequest.assetBundle;
					}
					else
					{
						if (_createRequest.isDone == false)
							return;
						CacheBundle = _createRequest.assetBundle;
					}
				}

				// Check error			
				if (CacheBundle == null)
				{
					_steps = ESteps.Done;
					Status = EStatus.Failed;
					LastError = $"Failed to load assetBundle : {MainBundleInfo.Bundle.BundleName}";
					YooLogger.Error(LastError);

					// 注意：当缓存文件的校验等级为Low的时候，并不能保证缓存文件的完整性。
					// 在AssetBundle文件加载失败的情况下，我们需要重新验证文件的完整性！
					if (MainBundleInfo.LoadMode == BundleInfo.ELoadMode.LoadFromCache)
					{
						string cacheLoadPath = MainBundleInfo.Bundle.CachedFilePath;
						if (CacheSystem.VerifyBundle(MainBundleInfo.Bundle, EVerifyLevel.High) != EVerifyResult.Succeed)
						{
							if (File.Exists(cacheLoadPath))
							{
								YooLogger.Error($"Delete the invalid cache file : {cacheLoadPath}");
								File.Delete(cacheLoadPath);
							}
						}
					}
				}
				else
				{
					_steps = ESteps.Done;
					Status = EStatus.Succeed;
				}
			}
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public override void WaitForAsyncComplete()
		{
			_isWaitForAsyncComplete = true;

			int frame = 1000;
			while (true)
			{
				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
				{
					if (_isShowWaitForAsyncError == false)
					{
						_isShowWaitForAsyncError = true;
						YooLogger.Error($"WaitForAsyncComplete failed ! Try load bundle : {MainBundleInfo.Bundle.BundleName} from remote with sync load method !");
					}
					break;
				}

				// 驱动流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
		}
	}
}