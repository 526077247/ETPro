using YooAsset;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ET
{
    [ObjectSystem]
    public class ResourcesComponentAwakeSystem : AwakeSystem<ResourcesComponent>
    {
        public override void Awake(ResourcesComponent self)
        {
            ResourcesComponent.Instance = self;
            self.ProcessingAddressablesAsyncLoaderCount = 0;
            self.Temp = new Dictionary<object, AssetOperationHandle>(1024);
            self.CachedAssetOperationHandles = new List<AssetOperationHandle>(1024);
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
            return self.ProcessingAddressablesAsyncLoaderCount > 0;
        }
        /// <summary>
        /// 同步加载Asset
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>(this ResourcesComponent self,string path) where T: UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                return null;
            }
            self.ProcessingAddressablesAsyncLoaderCount++;
            var op = YooAssets.LoadAssetSync<T>(path);
            self.ProcessingAddressablesAsyncLoaderCount--;
            self.Temp.Add(op.AssetObject,op);
            return op.AssetObject as T;

        }
        /// <summary>
        /// 异步加载Asset：协程形式
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ETTask<T> LoadAsync<T>(this ResourcesComponent self,string path, Action<T> callback = null) where T: UnityEngine.Object
        {
            ETTask<T> res = ETTask<T>.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                callback?.Invoke(null);
                res.SetResult(null);
                return res;
            }
            self.ProcessingAddressablesAsyncLoaderCount++;
            
            void OnCompleted(AssetOperationHandle handle)
            {
                handle.Completed -= OnCompleted;
                var obj = handle.AssetObject;
                self.ProcessingAddressablesAsyncLoaderCount--;
                callback?.Invoke(obj as T);
                res.SetResult(obj as T);
                if (!self.Temp.ContainsKey(obj))
                {
                    self.Temp.Add(obj, handle);
                }
                else
                {
                    handle.Release();
                }
            }
            
            var op = YooAssets.LoadAssetAsync<T>(path);
            op.Completed += OnCompleted;
            return res;

        }

        /// <summary>
        /// 异步加载Asset：协程形式
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ETTask LoadTask<T>(this ResourcesComponent self,string path,Action<T> callback)where T:UnityEngine.Object
        {
            ETTask task = ETTask.Create(true);
            self.LoadAsync<T>(path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path"></param>
        /// <param name="isAdditive"></param>
        /// <returns></returns>
        public static ETTask LoadSceneAsync(this ResourcesComponent self,string path, bool isAdditive)
        {
            ETTask res = ETTask.Create(true);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path err : " + path);
                return res;
            }
            self.ProcessingAddressablesAsyncLoaderCount++;
            var op = YooAssets.LoadSceneAsync(path,isAdditive?LoadSceneMode.Additive:LoadSceneMode.Single);
            op.Completed += (op) =>
            {
                self.ProcessingAddressablesAsyncLoaderCount--;
                res.SetResult();
            };
            return res;
        }


        /// <summary>
        /// 清理资源：切换场景时调用
        /// </summary>
        /// <param name="self"></param>
        /// <param name="excludeClearAssets"></param>
        public static void ClearAssetsCache(this ResourcesComponent self,UnityEngine.Object[] excludeClearAssets = null)
        {
            HashSetComponent<AssetOperationHandle> temp = null;
            if (excludeClearAssets != null)
            {
                temp = HashSetComponent<AssetOperationHandle>.Create();
                for (int i = 0; i < excludeClearAssets.Length; i++)
                {
                    temp.Add(self.Temp[excludeClearAssets[i]]);
                }
            }

            for (int i = self.CachedAssetOperationHandles.Count-1; i >=0; i--)
            {
                if (temp == null || !temp.Contains(self.CachedAssetOperationHandles[i]))
                {
                    self.Temp.Remove(self.CachedAssetOperationHandles[i].AssetObject);
                    self.CachedAssetOperationHandles[i].Release();
                    self.CachedAssetOperationHandles.RemoveAt(i);
                }
            }
            YooAssets.UnloadUnusedAssets();
        }

        public static void ReleaseAsset(this ResourcesComponent self,UnityEngine.Object pooledGo)
        {
            if(self==null) return;
            if (self.Temp.TryGetValue(pooledGo, out var op))
            {
                op.Release();
                self.Temp.Remove(pooledGo);
                self.CachedAssetOperationHandles.Remove(op);
            }
        }
            
    }
}