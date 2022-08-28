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
        //是否有加载任务正在进行
        public static bool IsProsessRunning(this ResourcesComponent self)
        {
            return self.ProcessingAddressablesAsyncLoaderCount > 0;
        }

        //异步加载Asset：协程形式
        public static ETTask<T> LoadAsync<T>(this ResourcesComponent self,string path, Action<T> callback = null) where T: UnityEngine.Object
        {
            ETTask<T> res = ETTask<T>.Create();
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("path is empty");
                callback?.Invoke(null);
                res.SetResult(null);
                return res;
            }
            self.ProcessingAddressablesAsyncLoaderCount++;
            var op = YooAssets.LoadAssetAsync<T>(path);
            op.Completed += (op) =>
            {
                self.ProcessingAddressablesAsyncLoaderCount--;
                callback?.Invoke(op.AssetObject as T);
                res.SetResult(op.AssetObject as T);
                if (!self.Temp.ContainsKey(op.AssetObject))
                {
                    self.Temp.Add(op.AssetObject, op);
                }
                else
                {
                    op.Release();
                }
            };
            return res;

        }

        public static ETTask LoadTask<T>(this ResourcesComponent self,string path,Action<T> callback)where T:UnityEngine.Object
        {
            ETTask task = ETTask.Create();
            self.LoadAsync<T>(path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }

        public static ETTask LoadSceneAsync(this ResourcesComponent self,string path, bool isAdditive)
        {
            ETTask res = ETTask.Create();
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


        //清理资源：切换场景时调用
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
            if (self.Temp.TryGetValue(pooledGo, out var op))
            {
                op.Release();
                self.Temp.Remove(pooledGo);
                self.CachedAssetOperationHandles.Remove(op);
            }
        }
            
    }
}