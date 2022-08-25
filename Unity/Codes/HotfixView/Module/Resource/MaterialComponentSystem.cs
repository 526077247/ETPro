using System.Collections.Generic;
using UnityEngine;
using System;
namespace ET
{
    [FriendClass(typeof(MaterialComponent))]
    public static class MaterialComponentSystem
    {
        [ObjectSystem]
        public class MaterialComponentAwakeSystem : AwakeSystem<MaterialComponent>
        {
            public override void Awake(MaterialComponent self)
            {
                MaterialComponent.Instance = self;
                self.m_cacheMaterial = new Dictionary<string, Material>();
            }
        }
        
        public static async ETTask<Material> LoadMaterialAsync(this MaterialComponent self,string address, Action<Material> callback = null)
        {
            Material res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, address.GetHashCode());
                if (!self.m_cacheMaterial.TryGetValue(address, out res))
                {
                    res = await ResourcesComponent.Instance.LoadAsync<Material>(address);
                    if (res != null)
                        self.m_cacheMaterial[address] = res;
                }
                callback?.Invoke(res);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        
        public static ETTask LoadMaterialTask(this MaterialComponent self,string address, Action<Material> callback = null)
        {
            ETTask task = ETTask.Create();
            self.LoadMaterialAsync(address, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }
    }
}