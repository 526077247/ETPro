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
            self.goPool = new LruCache<string, GameObject>();
            self.goInstCountCache = new Dictionary<string, int>();
            self.goChildsCountPool = new Dictionary<string, int>();
            self.instCache = new Dictionary<string, List<GameObject>>();
            self.instPathCache = new Dictionary<GameObject, string>();
            self.persistentPathCache = new Dictionary<string, bool>();
            self.detailGoChildsCount = new Dictionary<string, Dictionary<string, int>>();

            var go = GameObject.Find("GameObjectCacheRoot");
            if (go == null)
            {
                go = new GameObject("GameObjectCacheRoot");
            }
            GameObject.DontDestroyOnLoad(go);
            self.cacheTransRoot = go.transform;

            self.goPool.SetPopCallback((path, pooledGo) =>
            {
                self.ReleaseAsset(path);
            });
            self.goPool.SetCheckCanPopCallback((path, pooledGo) =>
            {
                var cnt = self.goInstCountCache[path] - (self.instCache.ContainsKey(path) ? self.instCache[path].Count : 0);
                if (cnt > 0)
                    Log.Info(string.Format("path={0} goInstCountCache={1} instCache={2}", path, self.goInstCountCache[path], 
                        (self.instCache[path] != null ? self.instCache[path].Count : 0)));
                return cnt == 0 && !self.persistentPathCache.ContainsKey(path);
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
	    /// <summary>
	    /// 从池子获取UI组件
	    /// </summary>
	    /// <param name="self"></param>
	    /// <param name="path"></param>
	    /// <typeparam name="T"></typeparam>
	    /// <returns></returns>
        public static async ETTask<T> GetUIGameObjectAsync<T>(this GameObjectPoolComponent self, string path) where T : Entity,IAwake,IOnCreate
        {
            var obj = await self.GetGameObjectAsync(path);
            if (obj == null) return null;
            T res = self.AddChild<T>();
            res.AddUIComponent<UITransform,Transform>("", obj.transform);
            UIWatcherComponent.Instance.OnCreate(res);
            return res;
        }

	    /// <summary>
	    /// 池子回收UI组件
	    /// </summary>
	    /// <param name="self"></param>
	    /// <param name="obj"></param>
	    /// <param name="isClear"></param>
	    /// <typeparam name="T"></typeparam>
        public static void RecycleUIGameObject<T>(this GameObjectPoolComponent self, T obj,bool isClear = false) where T : Entity,IAwake,IOnCreate
        {
            var uiTrans = obj.GetUIComponent<UITransform>();
            self.RecycleGameObject(uiTrans.transform.gameObject, isClear);
            obj.BeforeOnDestroy();
            UIWatcherComponent.Instance.OnDestroy(obj);
        }


		/// <summary>
		/// 预加载一系列资源
		/// </summary>
		/// <param name="self"></param>
		/// <param name="res"></param>
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
		/// <summary>
		/// 尝试从缓存中获取
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="go"></param>
		/// <returns></returns>
		public static bool TryGetFromCache(this GameObjectPoolComponent self,string path, out GameObject go)
		{
			go = null;
			if (!self.CheckHasCached(path)) return false;
			if (self.instCache.TryGetValue(path, out var cachedInst))
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
			if (self.goPool.TryGet(path, out var pooledGo))
			{
				if (pooledGo != null)
				{
					var inst = GameObject.Instantiate(pooledGo);
					if(self.goInstCountCache.ContainsKey(path))
						self.goInstCountCache[path]++;
					else 
						self.goInstCountCache[path] = 1;
					self.instPathCache[inst] = path;
					go = inst;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 预加载：可提供初始实例化个数
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="instCount">初始实例化个数</param>
		/// <param name="callback"></param>
		public static async ETTask PreLoadGameObjectAsync(this GameObjectPoolComponent self,string path, int instCount,Action callback = null)
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
						self.CacheAndInstGameObject(path, go as GameObject, instCount);
					}
					callback?.Invoke();
				}
			}
			finally
			{
				coroutineLock?.Dispose();
			}
		}
		/// <summary>
		/// 异步获取：必要时加载
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 异步获取：必要时加载
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
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

		/// <summary>
		/// 同步取已加载的，没加加载过则返回null
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static GameObject GetGameObject(this GameObjectPoolComponent self,string path)
		{
			if (self.TryGetFromCache(path, out var inst))
			{
				self.InitInst(inst);
				return inst;
			}
			return null;
		}
		/// <summary>
		/// 回收
		/// </summary>
		/// <param name="self"></param>
		/// <param name="inst"></param>
		/// <param name="isclear"></param>
		public static void RecycleGameObject(this GameObjectPoolComponent self,GameObject inst, bool isclear = false)
		{
			if(self==null||self.IsDisposed) return;
			if (!self.instPathCache.ContainsKey(inst))
			{
				Log.Error("RecycleGameObject inst not found from instPathCache");
				return;
			}
			var path = self.instPathCache[inst];
			if (!isclear)
			{
				self.CheckRecycleInstIsDirty(path, inst, null);
				inst.transform.SetParent(self.cacheTransRoot, false);
				inst.SetActive(false);
				if (!self.instCache.ContainsKey(path))
				{
					self.instCache[path] = new List<GameObject>();
				}
				self.instCache[path].Add(inst);
			}
			else
			{
				self.DestroyGameObject(inst);
			}

			//self.CheckCleanRes(path);
		}
		/// <summary>
		/// <para>检测回收的时候是否需要清理资源(这里是检测是否清空 inst和缓存的go)</para>
		/// <para>这里可以考虑加一个配置表来处理优先级问题，一些优先级较高的保留</para>
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		public static void CheckCleanRes(this GameObjectPoolComponent self,string path)
		{
			var cnt = self.goInstCountCache[path] - (self.instCache.ContainsKey(path) ? self.instCache[path].Count : 0);
			if (cnt == 0 && !self.persistentPathCache.ContainsKey(path))
				self.ReleaseAsset(path);
		}

		/// <summary>
		/// <para>添加需要持久化的资源</para>
		/// </summary>
		/// <param name="path"></param>
		public static void AddPersistentPrefabPath(this GameObjectPoolComponent self,string path)
		{
			self.persistentPathCache[path] = true;

		}

		/// <summary>
		/// <para>清理缓存</para>
		/// </summary>
		/// <param name="self"></param>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		/// <param name="excludePathArray">忽略的</param>
		public static void Cleanup(this GameObjectPoolComponent self,bool includePooledGo = true, List<string> excludePathArray = null)
		{
			Log.Info("GameObjectPool Cleanup ");
			foreach (var item in self.instCache)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					var inst = item.Value[i];
					if (inst != null)
					{
						GameObject.Destroy(inst);
						self.goInstCountCache[item.Key]--;
					}
					self.instPathCache.Remove(inst);
				}
			}
			self.instCache = new Dictionary<string, List<GameObject>>();

			if (includePooledGo)
			{
				Dictionary<string, bool> dictExcludepath = null;
				if (excludePathArray != null)
				{
					dictExcludepath = new Dictionary<string, bool>();
					for (int i = 0; i < excludePathArray.Count; i++)
					{
						dictExcludepath[excludePathArray[i]] = true;
					}
				}

				List<string> keys = self.goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if ((dictExcludepath == null || !dictExcludepath.ContainsKey(path))
					    && self.goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && self.CheckNeedUnload(path))
						{
							ResourcesComponent.Instance?.ReleaseAsset(pooledGo);
							self.goPool.Remove(path);
						}
					}
				}
			}
			Log.Info("GameObjectPool Cleanup Over");
		}
		/// <summary>
		/// <para>释放asset</para>
		/// <para>注意这里需要保证外面没有引用这些path的inst了，不然会出现材质丢失的问题</para>
		/// <para>不要轻易调用，除非你对内部的资源的生命周期有了清晰的了解</para>
		/// </summary>
		/// <param name="self"></param>
		/// <param name="includePooledGo">是否需要将预设也释放</param>
		/// <param name="patharray">需要释放的资源路径数组</param>
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
				foreach (var item in self.instCache)
				{
					if (dict_path.ContainsKey(item.Key))
					{
						for (int i = 0; i < item.Value.Count; i++)
						{
							var inst = item.Value[i];
							if (inst != null)
							{
								GameObject.Destroy(inst);
								self.goInstCountCache[item.Key]-- ;
							}
							self.instPathCache.Remove(inst);
						}
					}
				}
				for (int i = 0; i < patharray.Count; i++)
				{
					self.instCache.Remove(patharray[i]);
				}
			}
			
			if (includePooledGo)
			{
				List<string> keys = self.goPool.Keys.ToList();
				for (int i = keys.Count - 1; i >= 0; i--)
				{
					var path = keys[i];
					if (patharray != null && dict_path.ContainsKey(path) && self.goPool.TryOnlyGet(path, out var pooledGo))
					{
						if (pooledGo != null && self.CheckNeedUnload(path))
						{
							ResourcesComponent.Instance.ReleaseAsset(pooledGo);
							self.goPool.Remove(path);
						}
					}
				}
			}
		}
		/// <summary>
		/// 获取已经缓存的预制
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static GameObject GetCachedGoWithPath(this GameObjectPoolComponent self,string path)
		{
			if (self.goPool.TryOnlyGet(path, out var res))
			{
				return res;
			}
			return null;
		}
		
				
		#region 私有方法
		        
		/// <summary>
		/// 初始化inst
		/// </summary>
		/// <param name="self"></param>
		/// <param name="inst"></param>
		static void InitInst(this GameObjectPoolComponent self,GameObject inst)
		{
			if (inst != null)
			{
				inst.SetActive(true);
			}
		}
		/// <summary>
		/// 检测是否已经被缓存
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <returns></returns>
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

			if (self.instCache.ContainsKey(path) && self.instCache[path].Count > 0)
			{
				return true;
			}
			return self.goPool.ContainsKey(path);
		}

		/// <summary>
		/// 缓存并实例化GameObject
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="go"></param>
		/// <param name="inst_count"></param>
		static void CacheAndInstGameObject(this GameObjectPoolComponent self,string path, GameObject go, int inst_count)
		{
			self.goPool.Set(path, go);
			self.InitGoChildCount(path, go);
			if (inst_count > 0)
			{
				List<GameObject> cachedInst;
				if (!self.instCache.TryGetValue(path, out cachedInst))
					cachedInst = new List<GameObject>();
				for (int i = 0; i < inst_count; i++)
				{
					var inst = GameObject.Instantiate(go);
					inst.transform.SetParent(self.cacheTransRoot);
					inst.SetActive(false);
					cachedInst.Add(inst);
					self.instPathCache[inst] = path;
				}
				self.instCache[path] = cachedInst;
				if (!self.goInstCountCache.ContainsKey(path)) self.goInstCountCache[path] = 0;
				self.goInstCountCache[path] = self.goInstCountCache[path] + inst_count;
			}
		}
		/// <summary>
		/// 删除gameobject 所有从GameObjectPool中
		/// </summary>
		/// <param name="self"></param>
		/// <param name="inst"></param>
		static void DestroyGameObject(this GameObjectPoolComponent self,GameObject inst)
		{
			if (self.instPathCache.TryGetValue(inst, out string path))
			{
				if (self.goInstCountCache.TryGetValue(path, out int count))
				{
					if (count <= 0)
					{
						Log.Error("goInstCountCache[path] must > 0");
					}
					else
					{
						self.CheckRecycleInstIsDirty(path, inst, () =>
						{
							GameObject.Destroy(inst);
							self.goInstCountCache[path]--;
						});
						self.instPathCache.Remove(inst);
					}
				}
			}
			else
			{
				Log.Error("DestroyGameObject inst not found from instPathCache");
			}
		}
		/// <summary>
		/// 检查回收时是否污染
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="inst"></param>
		/// <param name="callback"></param>
		static void CheckRecycleInstIsDirty(this GameObjectPoolComponent self,string path, GameObject inst, Action callback)
		{
			if (!self.IsOpenCheck())
			{
				callback?.Invoke();
				return;
			}
			inst.SetActive(false);
			self.CheckAfter(path, inst).Coroutine();
			callback?.Invoke();
		}
		/// <summary>
		/// 延迟一段时间检查
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="inst"></param>
		/// <returns></returns>
		static async ETTask CheckAfter(this GameObjectPoolComponent self,string path, GameObject inst)
		{
			await TimerComponent.Instance.WaitAsync(2000);
			if (inst != null && inst.transform != null && self.CheckInstIsInPool(path, inst))
			{
				var go_child_count = self.goChildsCountPool[path];
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int instChildCount = self.RecursiveGetChildCount(inst.transform, "", ref childsCountMap);
				if (go_child_count != instChildCount)
				{
					Log.Error($"go_child_count({ go_child_count }) must equip inst_child_count({instChildCount}) path = {path} ");
					foreach (var item in childsCountMap)
					{
						var k = item.Key;
						var v = item.Value;
						var unfair = false;
						if (!self.detailGoChildsCount[path].ContainsKey(k))
							unfair = true;
						else if (self.detailGoChildsCount[path][k] != v)
							unfair = true;
						if (unfair)
							Log.Error($"not match path on checkrecycle = { k}, count = {v}");
					}
				}
			}
		}
		/// <summary>
		/// 检查inst是否在池子中
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="inst"></param>
		/// <returns></returns>
		static bool CheckInstIsInPool(this GameObjectPoolComponent self,string path, GameObject inst)
		{
			if (self.instCache.TryGetValue(path, out var inst_array))
			{
				for (int i = 0; i < inst_array.Count; i++)
				{
					if (inst_array[i] == inst) return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 获取GameObject的child数量
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		/// <param name="go"></param>
		static void InitGoChildCount(this GameObjectPoolComponent self,string path, GameObject go)
		{
			if (!self.IsOpenCheck()) return;
			if (!self.goChildsCountPool.ContainsKey(path))
			{
				Dictionary<string, int> childsCountMap = new Dictionary<string, int>();
				int total_child_count = self.RecursiveGetChildCount(go.transform, "", ref childsCountMap);
				self.goChildsCountPool[path] = total_child_count;
				self.detailGoChildsCount[path] = childsCountMap;
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		/// <param name="self"></param>
		/// <param name="path"></param>
		public static void ReleaseAsset(this GameObjectPoolComponent self,string path)
		{
			if (self.instCache.ContainsKey(path))
			{
				for (int i = self.instCache[path].Count - 1; i >= 0; i--)
				{
					self.instPathCache.Remove(self.instCache[path][i]);
					GameObject.Destroy(self.instCache[path][i]);
					self.instCache[path].RemoveAt(i);
				}
				self.instCache.Remove(path);
				self.goInstCountCache.Remove(path);
			}
			if (self.goPool.TryOnlyGet(path, out var pooledGo) && self.CheckNeedUnload(path))
			{
				ResourcesComponent.Instance.ReleaseAsset(pooledGo);
				self.goPool.Remove(path);
			}
		}
		/// <summary>
		/// 是否开启检查污染
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		static bool IsOpenCheck(this GameObjectPoolComponent self)
		{
			return Define.Debug;
		}
		/// <summary>
		/// 递归取子物体组件数量
		/// </summary>
		/// <param name="self"></param>
		/// <param name="trans"></param>
		/// <param name="path"></param>
		/// <param name="record"></param>
		/// <returns></returns>
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
		/// <param name="self"></param>
		/// <param name="path"></param>
		private static bool CheckNeedUnload(this GameObjectPoolComponent self,string path)
		{
			return !self.instPathCache.ContainsValue(path);
		}
		
		#endregion
		
    }
}
