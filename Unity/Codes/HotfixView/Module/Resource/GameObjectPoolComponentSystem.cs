using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ET
{
    
    [ObjectSystem]
    public class GameObjectPoolComponentAwakeSystem : AwakeSystem<GameObjectPoolComponent>
    {
        public override void Awake(GameObjectPoolComponent self)
        {
            GameObjectPoolComponent.Instance = self;
            self.__goPool = new LruCache<string, GameObject>();
            self.__goInstCountCache = new Dictionary<string, int>();
            self.__goChildsCountPool = new Dictionary<string, int>();
            self.__instCache = new Dictionary<string, List<GameObject>>();
            self.__instPathCache = new Dictionary<GameObject, string>();
            self.__persistentPathCache = new Dictionary<string, bool>();
            self.__detailGoChildsCount = new Dictionary<string, Dictionary<string, int>>();

            var go = GameObject.Find("GameObjectCacheRoot");
            if (go == null)
            {
                go = new GameObject("GameObjectCacheRoot");
            }
            GameObject.DontDestroyOnLoad(go);
            self.__cacheTransRoot = go.transform;

            self.__goPool.SetPopCallback((path, pooledGo) =>
            {
                self.__ReleaseAsset(path);
            });
            self.__goPool.SetCheckCanPopCallback((path, pooledGo) =>
            {
                var cnt = self.__goInstCountCache[path] - (self.__instCache.ContainsKey(path) ? self.__instCache[path].Count : 0);
                if (cnt > 0)
                    Log.Info(string.Format("path={0} __goInstCountCache={1} __instCache={2}", path, self.__goInstCountCache[path], 
                        (self.__instCache[path] != null ? self.__instCache[path].Count : 0)));
                return cnt == 0 && !self.__persistentPathCache.ContainsKey(path);
            });

        }
    }
	[ObjectSystem]
    public class GameObjectPoolComponentDestroy: DestroySystem<GameObjectPoolComponent>
    {
	    public override void Destroy(GameObjectPoolComponent self)
	    {
		    self.Cleanup();
		    GameObjectPoolComponent.Instance = null;
	    }
    }
    
    [FriendClass(typeof(GameObjectPoolComponent))]
    [FriendClass(typeof(UITransform))]
    public static class GameObjectPoolComponentSystem
    {
        public static async ETTask<T> GetUIGameObjectAsync<T>(this GameObjectPoolComponent self, string path) where T : Entity,IAwake
        {
            var obj = await self.GetGameObjectAsync(path);
            if (obj == null) return null;
            T res = self.AddChild<T>();
            res.AddUIComponent<UITransform,Transform>("", obj.transform);
            UIWatcherComponent.Instance.OnCreate(res);
            return res;
        }

        public static void RecycleUIGameObject<T>(this GameObjectPoolComponent self, T obj,bool isClear = false) where T : Entity
        {
            var uiTrans = obj.GetUIComponent<UITransform>();
            self.RecycleGameObject(uiTrans.transform.gameObject, isClear);
            obj.BeforeOnDestroy();
            UIWatcherComponent.Instance.OnDestroy(obj);
        }


		//预加载一系列资源
		public static async ETTask LoadDependency(this GameObjectPoolComponent self,List<string> res)
		{
			if (res.Count <= 0) return;
			using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
			{
				for (int i = 0; i < res.Count; i++)
				{
					TaskScheduler.Add(self.PreLoadGameObjectAsync(res[i], 1));
				}
				await ETTaskHelper.WaitAll(TaskScheduler);
			}
		}
		//尝试从缓存中获取
		public static bool TryGetFromCache(this GameObjectPoolComponent self,string path, out GameObject go)
		{
			go = null;
			if (!self.CheckHasCached(path)) return false;
			if (self.__instCache.TryGetValue(path, out var cachedInst))
			{
				if (cachedInst.Count > 0)
				{
					var inst = cachedInst[cachedInst.Count - 1];
					cachedInst.RemoveAt(cachedInst.Count - 1);
					go = inst;
					if (inst == null)
					{
						Log.Error("Something wrong, there gameObject instance in cache is null!");
						return false;
					}
					return true;
				}
			}
			if (self.__goPool.TryGet(path, out var pooledGo))
			{
				if (pooledGo != null)
				{
					var inst = GameObject.Instantiate(pooledGo);
					self.__goInstCountCache[path] = self.__goInstCountCache[path] + 1;
					self.__instPathCache[inst] = path;
					go = inst;
					return true;
				}
			}
			return false;
		}

		//预加载：可提供初始实例化个数
		public static async ETTask PreLoadGameObjectAsync(this GameObjectPoolComponent self,string path, int inst_count,Action callback = null)
		{
			CoroutineLock coroutineLock = null;
			try
			{
				coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, path.GetHashCode());
				if (self.CheckHasCached(path))
				{
					callback?.Invoke();
				}
				else
				{
					var go = await ResourcesComponent.Instance.LoadAsync<GameObject>(path);
					if (go != null)
					{
						self.CacheAndInstGameObject(path, go as GameObject, inst_count);
					}
					callback?.Invoke();
				}
			}
			finally
			{
				coroutineLock?.Dispose();
			}
		}
		//异步获取：必要时加载
		public static ETTask GetGameObjectTask(this GameObjectPoolComponent self, string path, Action<GameObject> callback = null)
		{
			ETTask task = ETTask.Create();
			self.GetGameObjectAsync(path, (data) =>
			{
				callback?.Invoke(data);
				task.SetResult();
			}).Coroutine();
			return task;
		}

		//异步获取：必要时加载
		public static async ETTask<GameObject> GetGameObjectAsync(this GameObjectPoolComponent self,string path,Action<GameObject> callback = null)
		{
			if (self.TryGetFromCache(path, out var inst))
			{
				self.InitInst(inst);
				callback?.Invoke(inst);
				return inst;
			}
			await self.PreLoadGameObjectAsync(path, 1);
			if (self.TryGetFromCache(path, out inst))
			{
				self.InitInst(inst);
				callback?.Invoke(inst);
				return inst;
			}
			callback?.Invoke(null);
			return null;
		}

		public static GameObject GetGameObject(this GameObjectPoolComponent self,string path)
		{
			if (self.TryGetFromCache(path, out var inst))
			{
				self.InitInst(inst);
				return inst;
			}
			return null;
		}

		//回收
		public static void RecycleGameObject(this GameObjectPoolComponent self,GameObject inst, bool isclear = false)
		{
			if (!self.__instPathCache.ContainsKey(inst))
			{
				Log.Error("RecycleGameObject inst not found from __instPathCache");
				return;
			}
			var path = self.__instPathCache[inst];
			if (!isclear)
			{
				self.__CheckRecycleInstIsDirty(path, inst, null);
				inst.transform.SetParent(self.__cacheTransRoot, false);
				inst.SetActive(false);
				if (!self.__instCache.ContainsKey(path))
				{
					self.__instCache[path] = new List<GameObject>();
				}
				self.__instCache[path].Add(inst);
			}
			else
			{
				self.DestroyGameObject(inst);
			}

			//self.CheckCleanRes(path);
		}
		//检测回收的时候是否需要清理资源(这里是检测是否清空 inst和缓存的go)
		//这里可以考虑加一个配置表来处理优先级问题，一些优先级较高的保留
		public static void CheckCleanRes(this GameObjectPoolComponent self,string path)
		{
			var cnt = self.__goInstCountCache[path] - (self.__instCache.ContainsKey(path) ? self.__instCache[path].Count : 0);
			if (cnt == 0 && !self.__persistentPathCache.ContainsKey(path))
				self.__ReleaseAsset(path);
		}

		//添加需要持久化的资源
		public static void AddPersistentPrefabPath(this GameObjectPoolComponent self,string path)
		{
			self.__persistentPathCache[path] = true;

		}

		//清理缓存
		public static void Cleanup(this GameObjectPoolComponent self,bool includePooledGo = true, List<string> excludePathArray = null)
		{
			Log.Info("GameObjectPool Cleanup ");
			foreach (var item in self.__instCache)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					var inst = item.Value[i];
					if (inst != null)
					{
						GameObject.Destroy(inst);
						self.__goInstCountCache[item.Key]--;
					}
					self.__instPathCache.Remove(inst);
				}
			}
			self.__instCache = new Dictionary<string, List<GameObject>>();

			if (includePooledGo)
			{
				Dictionary<string, bool> dict_excludepath = null;
				if (excludePathArray != null)
				{
					dict_excludepath = new Dictionary<string, bool>();
					for (int i = 0; i < excludePathArray.Count; i++)
					{
						dict_excludepath[excludePathArray[i]] = true;
					}
				}

				List<string> keys = self.__goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (dict_excludepath != null && !dict_excludepath.ContainsKey(path) && self.__goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && self.__CheckNeedUnload(path))
						{
							ResourcesComponent.Instance.ReleaseAsset(pooledGo);
							self.__goPool.Remove(path);
						}
					}
				}
			}
			Log.Info("GameObjectPool Cleanup Over");
		}
		//--释放asset
		//--注意这里需要保证外面没有引用这些path的inst了，不然会出现材质丢失的问题
		//--不要轻易调用，除非你对内部的资源的生命周期有了清晰的了解
		//--@param includePooledGo: 是否需要将预设也释放
		//--@param patharray： 需要释放的资源路径数组
		public static void CleanupWithPathArray(this GameObjectPoolComponent self,bool includePooledGo = true, List<string> patharray = null)
		{
			Debug.Log("GameObjectPool Cleanup ");
			Dictionary<string, bool> dict_path = null;
			if (patharray != null)
			{
				dict_path = new Dictionary<string, bool>();
				for (int i = 0; i < patharray.Count; i++)
				{
					dict_path[patharray[i]] = true;
				}
			}
			foreach (var item in self.__instCache)
			{
				if (dict_path.ContainsKey(item.Key))
				{
					for (int i = 0; i < item.Value.Count; i++)
					{
						var inst = item.Value[i];
						if (inst != null)
						{
							GameObject.Destroy(inst);
							self.__goInstCountCache[item.Key]-- ;
						}
						self.__instPathCache.Remove(inst);
					}
				}
			}
			for (int i = 0; i < patharray.Count; i++)
			{
				self.__instCache.Remove(patharray[i]);
			}

			if (includePooledGo)
			{
				List<string> keys = self.__goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (patharray != null && dict_path.ContainsKey(path) && self.__goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && self.__CheckNeedUnload(path))
						{
							ResourcesComponent.Instance.ReleaseAsset(pooledGo);
							self.__goPool.Remove(path);
						}
					}
				}
			}
		}
		
		public static GameObject GetCachedGoWithPath(this GameObjectPoolComponent self,string path)
		{
			if (self.__goPool.TryOnlyGet(path, out var res))
			{
				return res;
			}
			return null;
		}
		
				
		#region 私有方法
		        
		// 初始化inst
		static void InitInst(this GameObjectPoolComponent self,GameObject inst)
		{
			if (inst != null)
			{
				inst.SetActive(true);
			}
		}
		// 检测是否已经被缓存
		static bool CheckHasCached(this GameObjectPoolComponent self,string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				Log.Error("path err :\"" + path + "\"");
				return false;
			}
			if (!path.EndsWith(".prefab"))
			{
				Log.Error("GameObject must be prefab : \"" + path + "\"");
				return false;
			}

			if (self.__instCache.ContainsKey(path) && self.__instCache[path].Count > 0)
			{
				return true;
			}
			return self.__goPool.ContainsKey(path);
		}

		//缓存并实例化GameObject
		static void CacheAndInstGameObject(this GameObjectPoolComponent self,string path, GameObject go, int inst_count)
		{
			self.__goPool.Set(path, go);
			self.__InitGoChildCount(path, go);
			if (inst_count > 0)
			{
				List<GameObject> cachedInst;
				if (!self.__instCache.TryGetValue(path, out cachedInst))
					cachedInst = new List<GameObject>();
				for (int i = 0; i < inst_count; i++)
				{
					var inst = GameObject.Instantiate(go);
					inst.transform.SetParent(self.__cacheTransRoot);
					inst.SetActive(false);
					cachedInst.Add(inst);
					self.__instPathCache[inst] = path;
				}
				self.__instCache[path] = cachedInst;
				if (!self.__goInstCountCache.ContainsKey(path)) self.__goInstCountCache[path] = 0;
				self.__goInstCountCache[path] = self.__goInstCountCache[path] + inst_count;
			}
		}
		//删除gameobject 所有从GameObjectPool中
		static void DestroyGameObject(this GameObjectPoolComponent self,GameObject inst)
		{
			if (self.__instPathCache.TryGetValue(inst, out string path))
			{
				if (self.__goInstCountCache.TryGetValue(path, out int count))
				{
					if (count <= 0)
					{
						Log.Error("__goInstCountCache[path] must > 0");
					}
					else
					{
						self.__CheckRecycleInstIsDirty(path, inst, () =>
						{
							GameObject.Destroy(inst);
							self.__goInstCountCache[path] = self.__goInstCountCache[path] - 1;
						});
					}
				}
			}
			else
			{
				Log.Error("DestroyGameObject inst not found from __instPathCache");
			}
		}
		
		static void __CheckRecycleInstIsDirty(this GameObjectPoolComponent self,string path, GameObject inst, Action callback)
		{
			if (!self.__IsOpenCheck())
			{
				callback?.Invoke();
				return;
			}
			inst.SetActive(false);
			self.__CheckAfter(path, inst).Coroutine();
			callback?.Invoke();
		}

		static async ETTask __CheckAfter(this GameObjectPoolComponent self,string path, GameObject inst)
		{
			await TimerComponent.Instance.WaitAsync(2000);
			if (inst != null && inst.transform != null && self.__CheckInstIsInPool(path, inst))
			{
				var go_child_count = self.__goChildsCountPool[path];
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int inst_child_count = self.RecursiveGetChildCount(inst.transform, "", ref childsCountMap);
				if (go_child_count != inst_child_count)
				{
					Log.Error($"go_child_count({ go_child_count }) must equip inst_child_count({inst_child_count}) path = {path} ");
					foreach (var item in childsCountMap)
					{
						var k = item.Key;
						var v = item.Value;
						var unfair = false;
						if (!self.__detailGoChildsCount[path].ContainsKey(k))
							unfair = true;
						else if (self.__detailGoChildsCount[path][k] != v)
							unfair = true;
						if (unfair)
							Log.Error($"not match path on checkrecycle = { k}, count = {v}");
					}
				}
			}
		}

		static bool __CheckInstIsInPool(this GameObjectPoolComponent self,string path, GameObject inst)
		{
			if (self.__instCache.TryGetValue(path, out var inst_array))
			{
				for (int i = 0; i < inst_array.Count; i++)
				{
					if (inst_array[i] == inst) return true;
				}
			}
			return false;
		}
		static void __InitGoChildCount(this GameObjectPoolComponent self,string path, GameObject go)
		{
			if (!self.__IsOpenCheck()) return;
			if (!self.__goChildsCountPool.ContainsKey(path))
			{
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int total_child_count = self.RecursiveGetChildCount(go.transform, "", ref childsCountMap);
				self.__goChildsCountPool[path] = total_child_count;
				self.__detailGoChildsCount[path] = childsCountMap;
			}
		}

		// 释放资源
		public static void __ReleaseAsset(this GameObjectPoolComponent self,string path)
		{
			if (self.__instCache.ContainsKey(path))
			{
				for (int i = self.__instCache[path].Count - 1; i >= 0; i--)
				{
					self.__instPathCache.Remove(self.__instCache[path][i]);
					GameObject.Destroy(self.__instCache[path][i]);
					self.__instCache[path].RemoveAt(i);
				}
				self.__instCache.Remove(path);
				self.__goInstCountCache.Remove(path);
			}
			if (self.__goPool.TryOnlyGet(path, out var pooledGo) && self.__CheckNeedUnload(path))
			{
				ResourcesComponent.Instance.ReleaseAsset(pooledGo);
				self.__goPool.Remove(path);
			}
		}
		static bool __IsOpenCheck(this GameObjectPoolComponent self)
		{
			return Define.Debug;
		}

		static int RecursiveGetChildCount(this GameObjectPoolComponent self,Transform trans, string path, ref Dictionary<string, int> record)
		{
			int total_child_count = trans.childCount;
			for (int i = 0; i < trans.childCount; i++)
			{
				var child = trans.GetChild(i);
				if (child.name.Contains("Input Caret") || child.name.Contains("TMP SubMeshUI") || child.name.Contains("TMP UI SubObject") || /*child.GetComponent<LoopListViewItem2>()!=null
					 || child.GetComponent<LoopGridViewItem>() != null ||*/ (child.name.Contains("Caret") && child.parent.name.Contains("Text Area")))
				{
					//Input控件在运行时会自动生成个光标子控件，而prefab中是没有的，所以得过滤掉
					//TextMesh会生成相应字体子控件
					//TextMeshInput控件在运行时会自动生成个光标子控件，而prefab中是没有的，所以得过滤掉
					total_child_count = total_child_count - 1;
				}
				else
				{
					string cpath = path + "/" + child.name;
					if (record.ContainsKey(cpath))
					{
						record[cpath] += 1;
					}
					else
					{
						record[cpath] = 1;
					}
					total_child_count += self.RecursiveGetChildCount(child, cpath, ref record);
				}
			}
			return total_child_count;
		}
		
		/// <summary>
		/// 检查指定路径是否有未回收的预制体
		/// </summary>
		/// <param name="path"></param>
		private static bool __CheckNeedUnload(this GameObjectPoolComponent self,string path)
		{
			return !self.__instPathCache.ContainsValue(path);
		}
		
		#endregion
		
    }
}
