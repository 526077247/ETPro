using YooAsset;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ET
{
    [ObjectSystem]
    public class ResourcesComponentAwakeSystem : AwakeSystem<ResourcesComponent>
    {
        public override void Awake(ResourcesComponent self)
        {
            ResourcesComponent.Instance = self;
            if(self.packageFinder==null)self.packageFinder = new DefaultPackageFinder();
            self.temp = new Dictionary<object, AssetHandle>(512);
            self.cachedAssetOperationHandles = new List<AssetHandle>(512);
            self.persistentAssetOperationHandles = new List<AssetHandle>(4);
            self.loadingOp = new HashSet<AssetHandle>();
        }
    }
    [ObjectSystem]
    public class  ResourcesComponentDestroySystem : DestroySystem<ResourcesComponent>
    {
        public override void Destroy(ResourcesComponent self)
        {
            ResourcesComponent.Instance = null;
        }
    }
    [FriendClass(typeof(ResourcesComponent))]
    public static class ResourcesComponentSystem
    {
        /// <summary>
        /// 是否有加载任务正在进行
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool IsProsessRunning(this ResourcesComponent self)
        {
            return self.loadingOp.Count > 0;
        }
        /// <summary>
        /// 异步加载Asset
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <param name="package"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ETTask<T> LoadAsync<T>(this ResourcesComponent self, string path, Action<T> callback = null, string package = null, bool isPersistent = false)
            where T : UnityEngine.Object
        {
            ETTask<T> res = ETTask<T>.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                callback?.Invoke(null);
                res.SetResult(null);
                return res;
            }

            if (package == null)
            {
                package = self.packageFinder.GetPackageName(path);
            }

            var op = PackageManager.Instance.LoadAssetAsync<T>(path, package);
            if (op == null)
            {
                Log.Error(package + "加载资源前未初始化！" + path);
                return default;
            }

            self.loadingOp.Add(op);
            op.Completed += (op) =>
            {
                var obj = op.AssetObject as T;
                self.loadingOp.Remove(op);
                if (obj != null && !self.temp.ContainsKey(obj))
                {
                    self.temp.Add(op.AssetObject, op);
                    if(isPersistent)
                        self.persistentAssetOperationHandles.Add(op);
                    else
                        self.cachedAssetOperationHandles.Add(op);
                }
                else
                {
                    op.Release();
                }
                callback?.Invoke(obj);
                res.SetResult(obj);
            };
            return res;

        }

        /// <summary>
        /// 异步加载Asset，返回ETTask
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <param name="package"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ETTask LoadTask<T>(this ResourcesComponent self,string path, Action<T> callback = null, string package = null)
            where T : UnityEngine.Object
        {
            ETTask task = ETTask.Create(true);
            if (package == null)
            {
                package = self.packageFinder.GetPackageName(path);
            }

            self.LoadAsync<T>(path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }, package).Coroutine();
            return task;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isAdditive"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        public static ETTask LoadSceneAsync(this ResourcesComponent self,string path, bool isAdditive, string package = null)
        {
            ETTask res = ETTask.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path err : " + path);
                return res;
            }

            if (package == null)
            {
                package = self.packageFinder.GetPackageName(path);
            }

            var op = PackageManager.Instance.LoadSceneAsync(path,
                isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single, package);
            if (op == null)
            {
                Log.Error(package + "加载资源前未初始化！" + path);
                return default;
            }

            op.Completed += (op) => { res.SetResult(); };
            return res;
        }

        /// <summary>
        /// 清理所有load出来的非持久资源
        /// </summary>
        /// <param name="ignoreClearAssets">不需要清除的</param>
        public static void CleanUp(this ResourcesComponent self,List<UnityEngine.Object> ignoreClearAssets = null)
        {
            HashSetComponent<AssetHandle> ignore = null;
            if (ignoreClearAssets != null)
            {
                ignore = HashSetComponent<AssetHandle>.Create();
                for (int i = 0; i < ignoreClearAssets.Count; i++)
                {
                    ignore.Add(self.temp[ignoreClearAssets[i]]);
                }
            }

            for (int i = self.cachedAssetOperationHandles.Count - 1; i >= 0; i--)
            {
                if (ignore == null || !ignore.Contains(self.cachedAssetOperationHandles[i]))
                {
                    self.temp.Remove(self.cachedAssetOperationHandles[i].AssetObject);
                    self.cachedAssetOperationHandles[i].Release();
                    self.cachedAssetOperationHandles.RemoveAt(i);
                }
            }
            ignore?.Dispose();
        }
        /// <summary>
        /// 清理所有load出来的资源
        /// </summary>
        public static void ClearAssetsCache(this ResourcesComponent self)
        {
            for (int i = self.cachedAssetOperationHandles.Count - 1; i >= 0; i--)
            {
                self.temp.Remove(self.cachedAssetOperationHandles[i].AssetObject);
                self.cachedAssetOperationHandles[i].Release();
                self.cachedAssetOperationHandles.RemoveAt(i);
            }
            
            for (int i = self.persistentAssetOperationHandles.Count - 1; i >= 0; i--)
            {
                self.temp.Remove(self.persistentAssetOperationHandles[i].AssetObject);
                self.persistentAssetOperationHandles[i].Release();
                self.persistentAssetOperationHandles.RemoveAt(i);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="pooledGo"></param>
        public static void ReleaseAsset(this ResourcesComponent self,UnityEngine.Object pooledGo)
        {
            if (self.temp.TryGetValue(pooledGo, out var op))
            {
                op.Release();
                self.temp.Remove(pooledGo);
                self.cachedAssetOperationHandles.Remove(op);
                self.persistentAssetOperationHandles.Remove(op);
            }
        }

        /// <summary>
        /// 同步加载json配置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async ETTask<string> LoadConfigJsonAsync(this ResourcesComponent self,string path)
        {
            if (string.IsNullOrEmpty(path)) return default;
            path += ".json";
            var file = await self.LoadAsync<TextAsset>(path);
            try
            {
                var text = file.text;
                self.ReleaseAsset(file);
                return text;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return null;
        }
        
        /// <summary>
        /// 同步加载二进制配置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async ETTask<byte[]> LoadConfigBytesAsync(this ResourcesComponent self,string path)
        {
            if (string.IsNullOrEmpty(path)) return default;
            path += ".bytes";
            var file = await self.LoadAsync<TextAsset>(path);
            try
            {
                var bytes = file.bytes;
                self.ReleaseAsset(file);
                return bytes;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }
            
    }
}