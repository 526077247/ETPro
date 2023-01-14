﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YooAsset
{
	internal sealed class FileDownloader : DownloaderBase
	{
		private readonly bool _breakResume;
		private UnityWebRequest _webRequest = null;
		private DownloadHandlerFileRange _downloadHandle = null;

		// 重置变量
		private bool _isAbort = false;
		private ulong _latestDownloadBytes;
		private float _latestDownloadRealtime;
		private float _tryAgainTimer;


		public FileDownloader(BundleInfo bundleInfo, bool breakResume) : base(bundleInfo)
		{
			_breakResume = breakResume;
		}
		public override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			// 检测本地文件
			if (_steps == ESteps.CheckLocalFile)
			{
				var verifyResult = CacheSystem.VerifyAndCacheBundle(_bundleInfo.Bundle, EVerifyLevel.High);
				if (verifyResult == EVerifyResult.Succeed)
				{
					_steps = ESteps.Succeed;
				}
				else
				{
					if (verifyResult == EVerifyResult.FileOverflow)
					{
						string cacheFilePath = _bundleInfo.Bundle.CachedFilePath;
						if (File.Exists(cacheFilePath))
							File.Delete(cacheFilePath);
					}
					_steps = ESteps.CreateDownload;
				}
			}

			// 创建下载器
			if (_steps == ESteps.CreateDownload)
			{
				string fileSavePath = _bundleInfo.Bundle.CachedFilePath;

				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;
				_isAbort = false;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;
				_tryAgainTimer = 0f;

				// 是否开启断点续传下载	
				if (_breakResume)
				{
					long fileLength = -1;
					if (File.Exists(fileSavePath))
					{
						FileInfo fileInfo = new FileInfo(fileSavePath);
						fileLength = fileInfo.Length;
					}

					_requestURL = GetRequestURL();
					_webRequest = UnityWebRequest.Get(_requestURL);

#if UNITY_2019_4_OR_NEWER
					var handler = new DownloadHandlerFile(fileSavePath, true);
					handler.removeFileOnAbort = false;
#else
					var handler = new DownloadHandlerFileRange(fileSavePath, _bundleInfo.Bundle.FileSize, _webRequest);
					_downloadHandle = handler;
#endif

					_webRequest.downloadHandler = handler;
					_webRequest.disposeDownloadHandlerOnDispose = true;
					if (fileLength > 0)
						_webRequest.SetRequestHeader("Range", $"bytes={fileLength}-");
					_webRequest.SendWebRequest();
					_steps = ESteps.CheckDownload;
				}
				else
				{
					_requestURL = GetRequestURL();
					_webRequest = new UnityWebRequest(_requestURL, UnityWebRequest.kHttpVerbGET);
					DownloadHandlerFile handler = new DownloadHandlerFile(fileSavePath);
					handler.removeFileOnAbort = true;
					_webRequest.downloadHandler = handler;
					_webRequest.disposeDownloadHandlerOnDispose = true;
					_webRequest.SendWebRequest();
					_steps = ESteps.CheckDownload;
				}
			}

			// 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = _webRequest.downloadProgress;
				_downloadedBytes = _webRequest.downloadedBytes;
				if (_webRequest.isDone == false)
				{
					CheckTimeout();
					return;
				}

				bool hasError = false;

				// 检查网络错误
#if UNITY_2020_3_OR_NEWER
				if (_webRequest.result != UnityWebRequest.Result.Success)
				{
					hasError = true;
					_lastError = _webRequest.error;
				}
#else
				if (_webRequest.isNetworkError || _webRequest.isHttpError)
				{
					hasError = true;
					_lastError = _webRequest.error;
				}
#endif

				// 检查文件完整性
				if (hasError == false)
				{
					var verifyResult = CacheSystem.VerifyAndCacheBundle(_bundleInfo.Bundle, EVerifyLevel.High);
					if (verifyResult != EVerifyResult.Succeed)
					{
						hasError = true;
						_lastError = $"Verify bundle content failed : {_bundleInfo.Bundle.FileName}";

						// 验证失败后删除文件
						string cacheFilePath = _bundleInfo.Bundle.CachedFilePath;
						if (File.Exists(cacheFilePath))
							File.Delete(cacheFilePath);
					}
				}

				// 如果下载失败
				if (hasError)
				{
					// 注意：非断点续传下载失败后删除文件
					if (_breakResume == false)
					{
						string cacheFilePath = _bundleInfo.Bundle.CachedFilePath;
						if (File.Exists(cacheFilePath))
							File.Delete(cacheFilePath);
					}

					// 失败后重新尝试
					if (_failedTryAgain > 0)
					{
						ReportWarning();
						_steps = ESteps.TryAgain;
					}
					else
					{
						ReportError();
						_steps = ESteps.Failed;
					}
				}
				else
				{
					_lastError = string.Empty;
					_steps = ESteps.Succeed;
				}

				// 释放下载器
				DisposeWebRequest();
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				_tryAgainTimer += Time.unscaledDeltaTime;
				if (_tryAgainTimer > 1f)
				{
					_failedTryAgain--;
					_steps = ESteps.CreateDownload;
					YooLogger.Warning($"Try again download : {_requestURL}");
				}
			}
		}
		public override void Abort()
		{
			if (IsDone() == false)
			{
				_steps = ESteps.Failed;
				_lastError = "user abort";
				DisposeWebRequest();
			}
		}

		private void CheckTimeout()
		{
			// 注意：在连续时间段内无新增下载数据及判定为超时
			if (_isAbort == false)
			{
				if (_latestDownloadBytes != DownloadedBytes)
				{
					_latestDownloadBytes = DownloadedBytes;
					_latestDownloadRealtime = Time.realtimeSinceStartup;
				}

				float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
				if (offset > _timeout)
				{
					YooLogger.Warning($"Web file request timeout : {_requestURL}");
					_webRequest.Abort();
					_isAbort = true;
				}
			}
		}
		private void DisposeWebRequest()
		{
			if(_downloadHandle != null)
			{
				_downloadHandle.Cleanup();
				_downloadHandle = null;
			}

			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
			}
		}
	}
}