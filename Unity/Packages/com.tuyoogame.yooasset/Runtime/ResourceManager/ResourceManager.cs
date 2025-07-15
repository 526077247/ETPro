﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    internal class ResourceManager
    {
        internal readonly Dictionary<string, ProviderOperation> ProviderDic = new Dictionary<string, ProviderOperation>(5000);
        internal readonly Dictionary<string, LoadBundleFileOperation> LoaderDic = new Dictionary<string, LoadBundleFileOperation>(5000);
        internal readonly List<SceneHandle> SceneHandles = new List<SceneHandle>(100);
        private long _sceneCreateIndex = 0;
        private IBundleQuery _bundleQuery;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 锁定加载操作
        /// </summary>
        public bool LockLoadOperation = false;


        public ResourceManager(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(InitializeParameters initializeParameters, IBundleQuery bundleServices)
        {
            _bundleQuery = bundleServices;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// 销毁管理器
        /// </summary>
        public void Destroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 尝试卸载指定资源的资源包（包括依赖资源）
        /// </summary>
        public void TryUnloadUnusedAsset(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to unload asset ! {assetInfo.Error}");
                return;
            }

            // 卸载主资源包加载器
            string mainBundleName = _bundleQuery.GetMainBundleName(assetInfo);
            var mainLoader = TryGetBundleFileLoader(mainBundleName);
            if (mainLoader != null)
            {
                mainLoader.TryDestroyProviders();
                if (mainLoader.CanDestroyLoader())
                {
                    string bundleName = mainLoader.LoadBundleInfo.Bundle.BundleName;
                    mainLoader.DestroyLoader();
                    LoaderDic.Remove(bundleName);
                }
            }

            // 卸载依赖资源包加载器
            string[] dependBundleNames = _bundleQuery.GetDependBundleNames(assetInfo);
            foreach (var dependBundleName in dependBundleNames)
            {
                var dependLoader = TryGetBundleFileLoader(dependBundleName);
                if (dependLoader != null)
                {
                    if (dependLoader.CanDestroyLoader())
                    {
                        string bundleName = dependLoader.LoadBundleInfo.Bundle.BundleName;
                        dependLoader.DestroyLoader();
                        LoaderDic.Remove(bundleName);
                    }
                }
            }
        }

        /// <summary>
        /// 加载场景对象
        /// 注意：返回的场景句柄是唯一的，每个场景句柄对应自己的场景提供者对象。
        /// 注意：业务逻辑层应该避免同时加载一个子场景。
        /// </summary>
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool suspendLoad, uint priority)
        {
            if (LockLoadOperation)
            {
                string error = $"The load operation locked !";
                YooLogger.Error(error);
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(error);
                return completedProvider.CreateHandle<SceneHandle>();
            }

            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load scene ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(assetInfo.Error);
                return completedProvider.CreateHandle<SceneHandle>();
            }

            // 注意：同一个场景的ProviderGUID每次加载都会变化
            string providerGUID = $"{assetInfo.GUID}-{++_sceneCreateIndex}";
            ProviderOperation provider;
            {
                provider = new SceneProvider(this, providerGUID, assetInfo, loadSceneParams, suspendLoad);
                provider.InitSpawnDebugInfo();
                ProviderDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            var handle = provider.CreateHandle<SceneHandle>();
            handle.PackageName = PackageName;
            SceneHandles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 加载资源对象
        /// </summary>
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority)
        {
            if (LockLoadOperation)
            {
                string error = $"The load operation locked !";
                YooLogger.Error(error);
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(error);
                return completedProvider.CreateHandle<AssetHandle>();
            }

            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load asset ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(assetInfo.Error);
                return completedProvider.CreateHandle<AssetHandle>();
            }

            string providerGUID = nameof(LoadAssetAsync) + assetInfo.GUID;
            ProviderOperation provider = TryGetAssetProvider(providerGUID);
            if (provider == null)
            {
                provider = new AssetProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                ProviderDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AssetHandle>();
        }

        /// <summary>
        /// 加载子资源对象
        /// </summary>
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (LockLoadOperation)
            {
                string error = $"The load operation locked !";
                YooLogger.Error(error);
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(error);
                return completedProvider.CreateHandle<SubAssetsHandle>();
            }

            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load sub assets ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(assetInfo.Error);
                return completedProvider.CreateHandle<SubAssetsHandle>();
            }

            string providerGUID = nameof(LoadSubAssetsAsync) + assetInfo.GUID;
            ProviderOperation provider = TryGetAssetProvider(providerGUID);
            if (provider == null)
            {
                provider = new SubAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                ProviderDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<SubAssetsHandle>();
        }

        /// <summary>
        /// 加载所有资源对象
        /// </summary>
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority)
        {
            if (LockLoadOperation)
            {
                string error = $"The load operation locked !";
                YooLogger.Error(error);
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(error);
                return completedProvider.CreateHandle<AllAssetsHandle>();
            }

            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load all assets ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(assetInfo.Error);
                return completedProvider.CreateHandle<AllAssetsHandle>();
            }

            string providerGUID = nameof(LoadAllAssetsAsync) + assetInfo.GUID;
            ProviderOperation provider = TryGetAssetProvider(providerGUID);
            if (provider == null)
            {
                provider = new AllAssetsProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                ProviderDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<AllAssetsHandle>();
        }

        /// <summary>
        /// 加载原生文件
        /// </summary>
        public RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority)
        {
            if (LockLoadOperation)
            {
                string error = $"The load operation locked !";
                YooLogger.Error(error);
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(error);
                return completedProvider.CreateHandle<RawFileHandle>();
            }

            if (assetInfo.IsInvalid)
            {
                YooLogger.Error($"Failed to load raw file ! {assetInfo.Error}");
                CompletedProvider completedProvider = new CompletedProvider(this, assetInfo);
                completedProvider.SetCompletedWithError(assetInfo.Error);
                return completedProvider.CreateHandle<RawFileHandle>();
            }

            string providerGUID = nameof(LoadRawFileAsync) + assetInfo.GUID;
            ProviderOperation provider = TryGetAssetProvider(providerGUID);
            if (provider == null)
            {
                provider = new RawFileProvider(this, providerGUID, assetInfo);
                provider.InitSpawnDebugInfo();
                ProviderDic.Add(providerGUID, provider);
                OperationSystem.StartOperation(PackageName, provider);
            }

            provider.Priority = priority;
            return provider.CreateHandle<RawFileHandle>();
        }

        internal LoadBundleFileOperation CreateMainBundleFileLoader(AssetInfo assetInfo)
        {
            BundleInfo bundleInfo = _bundleQuery.GetMainBundleInfo(assetInfo);
            return CreateBundleFileLoaderInternal(bundleInfo);
        }
        internal List<LoadBundleFileOperation> CreateDependBundleFileLoaders(AssetInfo assetInfo)
        {
            BundleInfo[] bundleInfos = _bundleQuery.GetDependBundleInfos(assetInfo);
            List<LoadBundleFileOperation> result = new List<LoadBundleFileOperation>(bundleInfos.Length);
            foreach (var bundleInfo in bundleInfos)
            {
                var bundleLoader = CreateBundleFileLoaderInternal(bundleInfo);
                result.Add(bundleLoader);
            }
            return result;
        }
        internal void RemoveBundleProviders(List<ProviderOperation> removeList)
        {
            foreach (var provider in removeList)
            {
                ProviderDic.Remove(provider.ProviderGUID);
            }
        }
        internal bool HasAnyLoader()
        {
            return LoaderDic.Count > 0;
        }

        private LoadBundleFileOperation CreateBundleFileLoaderInternal(BundleInfo bundleInfo)
        {
            // 如果加载器已经存在
            string bundleName = bundleInfo.Bundle.BundleName;
            LoadBundleFileOperation loaderOperation = TryGetBundleFileLoader(bundleName);
            if (loaderOperation != null)
                return loaderOperation;

            // 新增下载需求
            loaderOperation = new LoadBundleFileOperation(this, bundleInfo);
            OperationSystem.StartOperation(PackageName, loaderOperation);
            LoaderDic.Add(bundleName, loaderOperation);
            return loaderOperation;
        }
        private LoadBundleFileOperation TryGetBundleFileLoader(string bundleName)
        {
            if (LoaderDic.TryGetValue(bundleName, out LoadBundleFileOperation value))
                return value;
            else
                return null;
        }
        private ProviderOperation TryGetAssetProvider(string providerGUID)
        {
            if (ProviderDic.TryGetValue(providerGUID, out ProviderOperation value))
                return value;
            else
                return null;
        }
        private void OnSceneUnloaded(Scene scene)
        {
            List<SceneHandle> removeList = new List<SceneHandle>();
            foreach (var sceneHandle in SceneHandles)
            {
                if (sceneHandle.IsValid)
                {
                    if (sceneHandle.SceneObject == scene)
                    {
                        sceneHandle.Release();
                        removeList.Add(sceneHandle);
                    }
                }
            }
            foreach (var sceneHandle in removeList)
            {
                SceneHandles.Remove(sceneHandle);
            }
        }

        #region 调试信息
        internal List<DebugProviderInfo> GetDebugReportInfos()
        {
            List<DebugProviderInfo> result = new List<DebugProviderInfo>(ProviderDic.Count);
            foreach (var provider in ProviderDic.Values)
            {
                DebugProviderInfo providerInfo = new DebugProviderInfo();
                providerInfo.AssetPath = provider.MainAssetInfo.AssetPath;
                providerInfo.SpawnScene = provider.SpawnScene;
                providerInfo.SpawnTime = provider.SpawnTime;
                providerInfo.LoadingTime = provider.LoadingTime;
                providerInfo.RefCount = provider.RefCount;
                providerInfo.Status = provider.Status.ToString();
                providerInfo.DependBundleInfos = new List<DebugBundleInfo>();
                provider.GetBundleDebugInfos(providerInfo.DependBundleInfos);
                result.Add(providerInfo);
            }
            return result;
        }
        #endregion
    }
}