using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ET
{
    [ObjectSystem]
    public class AOISceneViewComponentAwakeSystem: AwakeSystem<AOISceneViewComponent>
    {
        public override void Awake(AOISceneViewComponent self)
        {
            AOISceneViewComponent.Instance = self;
            self.DynamicSceneObjectMapCount = new Dictionary<int, int>();
            self.DynamicSceneObjectMapObj = new Dictionary<int, AOISceneViewComponent.DynamicSceneViewObj>();
            self.NameMapScene = new Dictionary<string, AssetsScene>();
            self.Init();
        }
        
        
    }
    [FriendClass(typeof(AOISceneViewComponent))]
    [FriendClass(typeof(SceneManagerComponent))]
    public static class AOISceneViewComponentSystem
    {
        public static void Init(this AOISceneViewComponent self)
        {
            #region 从Proto初始化场景物体信息
            string[] protoPaths = {"MapConfig/Map.bytes"};
            for (int i = 0; i < protoPaths.Length; i++)
            {
                var jsonPath = protoPaths[i];
                ResourcesComponent.Instance.LoadAsync<TextAsset>(jsonPath, (file) =>
                {
                    var bytes = file.bytes;
                    AssetsRoot root;
                    for (int k = 0; k < 3; k++)//有时候Protobuf解析大量数据失败，多试几次
                    {
                        try
                        {
                            root= ProtobufHelper.FromBytes(typeof(AssetsRoot),bytes,0,bytes.Length) as AssetsRoot;
                            for (int j = 0; j < root.Scenes.Count; j++)
                            {
                                self.NameMapScene.Add(root.Scenes[j].Name, root.Scenes[j]);
                                root.Scenes[j].CellMapObjects = new Dictionary<long, List<int>>(root.Scenes[j].CellIds.Count);
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }
                }).Coroutine();
            }
            
            #endregion
        }
        /// <summary>
        /// 切换到某个场景
        /// </summary>
        /// <param name="name"></param>
        public static async ETTask ChangeToScene(this AOISceneViewComponent self,string name=null,SceneLoadComponent slc = null)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (SceneManagerComponent.Instance.Busing) return;
            //打开loading界面
            await Game.EventSystem.PublishAsync(new UIEventType.LoadingBegin());
            float slid_value = 0;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            int flag = 1;
            AssetsScene scene=null;
            while(!self.NameMapScene.TryGetValue(name,out scene)&&flag<300)//30s还没加载出来数据大小就有问题了
            {
                await TimerComponent.Instance.WaitAsync(100);
                flag++;
                var pro = Mathf.Lerp(0, 0.1f, flag / 200f);
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = pro });
            }

            if (scene == null)
            {
                Log.Error(name+" == null");
                SceneManagerComponent.Instance.Busing = false;
                return;
            }
            SceneManagerComponent.Instance.Busing = true;
            self.LastGridX = null;
            self.LastGridY = null;
            Log.Info("InnerSwitchScene start open uiloading");
            slid_value = 0.1f;
            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
            if (self.CurMap==null||name != self.CurMap.Name)//需要重新加载场景物体
            {
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                if (SceneManagerComponent.Instance.CurrentScene != SceneNames.Map)
                {
                    CameraManagerComponent.Instance.SetCameraStackAtLoadingStart();
                }
                slid_value = 0.15f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                //等待资源管理器加载任务结束，否则很多Unity版本在切场景时会有异常，甚至在真机上crash
                Log.Info("InnerSwitchScene ProsessRunning Done ");
                while (ResourcesComponent.Instance.IsProsessRunning())
                {
                    await TimerComponent.Instance.WaitAsync(1);
                }
                
                //清理旧场景
                foreach (var item in self.DynamicSceneObjectMapObj)
                {
                    self.RecycleObj(item.Key,false);
                }
                self.DynamicSceneObjectMapObj.Clear();
                self.DynamicSceneObjectMapCount.Clear();
                slid_value = 0.2f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });

                if (SceneManagerComponent.Instance.CurrentScene != SceneNames.Map)
                {
                    //清理UI
                    Log.Info("InnerSwitchScene Clean UI");
                    await UIManagerComponent.Instance.DestroyWindowExceptNames(SceneManagerComponent.Instance.DestroyWindowExceptNames.ToArray());
                    
                    slid_value = 0.25f;
                    Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                    //清除ImageLoaderManager里的资源缓存 这里考虑到我们是单场景
                    Log.Info("InnerSwitchScene ImageLoaderManager Cleanup");
                    ImageLoaderComponent.Instance.Clear();
                    //清除预设以及其创建出来的gameobject, 这里不能清除loading的资源
                    Log.Info("InnerSwitchScene GameObjectPool Cleanup");
                    
                    GameObjectPoolComponent.Instance.Cleanup(true, SceneManagerComponent.Instance.ScenesChangeIgnoreClean);
                    slid_value = 0.3f;
                    Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                    //清除除loading外的资源缓存 
                    List<UnityEngine.Object> gos = new List<UnityEngine.Object>();
                    for (int i = 0; i < SceneManagerComponent.Instance.ScenesChangeIgnoreClean.Count; i++)
                    {
                        var path = SceneManagerComponent.Instance.ScenesChangeIgnoreClean[i];
                        var go = GameObjectPoolComponent.Instance.GetCachedGoWithPath(path);
                        if (go != null)
                        {
                            gos.Add(go);
                        }
                    }
                    Log.Info("InnerSwitchScene ResourcesManager ClearAssetsCache excludeAssetLen = " + gos.Count);
                    ResourcesComponent.Instance.ClearAssetsCache(gos.ToArray());
                    slid_value = 0.45f;
                    Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                    
                    await ResourcesComponent.Instance.LoadSceneAsync(SceneManagerComponent.Instance.GetSceneConfigByName(SceneNames.Loading).SceneAddress, false);
                    Log.Info("LoadSceneAsync Over");
                    slid_value = 0.5f;
                    Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                    //GC：交替重复2次，清干净一点
                    GC.Collect();
                    GC.Collect();

                    var res = Resources.UnloadUnusedAssets();
                    while (!res.isDone)
                    {
                        await TimerComponent.Instance.WaitAsync(1);
                    }
                    slid_value = 0.6f;
                    Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });

                    Log.Info("异步加载目标场景 Start");
                    var scene_config = SceneManagerComponent.Instance.GetSceneConfigByName(SceneNames.Map); //固定Map空场景
                    //异步加载目标场景
                    await ResourcesComponent.Instance.LoadSceneAsync(scene_config.SceneAddress, false);
                    SceneManagerComponent.Instance.CurrentScene = scene_config.Name;
                }
                slid_value = 0.7f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                //准备工作：预加载资源等
                if (slc != null)
                {
                    await slc.OnPrepare((progress) =>
                    {
                        Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value + 0.3f * progress });
                        if (progress > 1) Log.Error("scene load waht's the fuck!");
                    });
                }
                slid_value = 1f;
                Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = slid_value });
                CameraManagerComponent.Instance.SetCameraStackAtLoadingDone();
            }

            Game.EventSystem.Publish(new UIEventType.LoadingProgress { Progress = 1 });
            await TimerComponent.Instance.WaitAsync(1000);//等1帧
            self.CurMap = scene;
            SceneManagerComponent.Instance.Busing = false;
        }

        /// <summary>
        /// 改变格子
        /// 这里通过9宫格实现，如果需要可自己改为四叉树八叉树
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="viewLen"></param>
        public static async ETTask ChangePosition(this AOISceneViewComponent self,Scene zoneScene,Vector3 pos,int viewLen)
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.AOIView, self.GetHashCode());
                while (self.Busing)
                {
                    await TimerComponent.Instance.WaitAsync(1);
                }
                int x = (int)(pos.x / self.CellLen);
                int y = (int)(pos.z / self.CellLen);
                if (self.LastGridX != null)
                {
                    int count = 0;
                    count += Math.Abs(x - (int) self.LastGridX);
                    count += Math.Abs(y - (int) self.LastGridY);
                    if (count > 4) //太远了走loading
                    {
                        await self.ChangeToScene();
                    }
                }
                DictionaryComponent<long, int> temp = DictionaryComponent<long, int>.Create();
                for (int i = -viewLen; i <= viewLen; i++)
                {
                    for (int j = -viewLen; j <= viewLen; j++)
                    {
                        if (self.LastGridY != null)
                        {
                            var oldid = AOIHelper.CreateCellId((int) self.LastGridX + i, (int) self.LastGridY + j);
                            if (!temp.ContainsKey(oldid))
                            {
                                temp[oldid] = 0;
                            }

                            temp[oldid]--;
                        }

                        var newid = AOIHelper.CreateCellId(x + i, y + j);
                        if (!temp.ContainsKey(newid))
                        {
                            temp[newid] = 0;
                        }

                        temp[newid]++;


                    }
                }

                using (ListComponent<ETTask> tasks = ListComponent<ETTask>.Create())
                {
                    foreach (var item in temp)
                    {
                        if (item.Value == 0) continue;
                        var list = self.GetCellMapObjects(item.Key);
                        if (list==null) continue;
                        for (int i = 0; i < list.Count; i++)
                        {
                            var index = list[i];
                            if (item.Value > 0) //新增
                            {
                                if (self.DynamicSceneObjectMapCount.ContainsKey(index))
                                {
                                    self.DynamicSceneObjectMapCount[index]++;
                                }
                                else
                                {
                                    self.DynamicSceneObjectMapCount[index] = 1;
                                }

                                //需要显示
                                if (self.DynamicSceneObjectMapCount[index] > 0)
                                {
                                    AOISceneViewComponent.DynamicSceneViewObj viewObj;
                                    //已经有
                                    if (self.DynamicSceneObjectMapObj.ContainsKey(index))
                                    {
                                        viewObj = self.DynamicSceneObjectMapObj[index];
                                        if (viewObj.Obj == null) //之前有单没加载出来，IsLoading改为true，防止之前已经被改成false了
                                        {
                                            viewObj.IsLoading = true;
                                        }

                                        continue;
                                    }

                                    var obj = self.CurMap.Objects[index];
                                    // Log.Info("AOISceneView Load " + obj.PrefabPath);
                                    //没有
                                    self.DynamicSceneObjectMapObj[index] = new AOISceneViewComponent.DynamicSceneViewObj();
                                    viewObj = self.DynamicSceneObjectMapObj[index];
                                    viewObj.IsLoading = true;
                                    viewObj.Type = obj.Type;
                                    if (viewObj.Type == "Prefab")
                                    {
                                        tasks.Add(GameObjectPoolComponent.Instance.GetGameObjectTask(obj.PrefabPath, (view) =>
                                        {
                                            if (!viewObj.IsLoading) //加载出来后已经不需要的
                                            {
                                                GameObjectPoolComponent.Instance.RecycleGameObject(view);
                                                self.DynamicSceneObjectMapObj.Remove(index);
                                            }

                                            viewObj.Obj = view;
                                            viewObj.IsLoading = false;
                                            view.transform.position = obj.Transform.Position;
                                            view.transform.rotation = obj.Transform.Rotation;
                                            view.transform.localScale = obj.Transform.Scale;
                                            view.transform.parent = GlobalComponent.Instance.Scene;
                                        }));
                                    }
                                
                                    else if (viewObj.Type == "Terrain")
                                    {
                                        var view = new GameObject(obj.Name);
                                        view.layer = LayerMask.NameToLayer("Map");
                                        view.transform.position = obj.Transform.Position;
                                        view.transform.rotation = obj.Transform.Rotation;
                                        view.transform.localScale = obj.Transform.Scale;
                                        view.transform.parent = GlobalComponent.Instance.Scene;
                                        var terrain = view.AddComponent<Terrain>();
                                        var collider = view.AddComponent<TerrainCollider>();
                                        tasks.Add(ResourcesComponent.Instance.LoadTask<TerrainData>(obj.Terrain.TerrainPath, (data) =>
                                        {
                                            viewObj.IsLoading = false;
                                            collider.terrainData = data;
                                            terrain.terrainData = data;
                                        }));
                                        //todo: 材质球
                                        obj.Terrain.MaterialPath = "GameAssets/Map/Materials/TerrainLit.mat";
                                        tasks.Add(MaterialComponent.Instance
                                                .LoadMaterialTask(obj.Terrain.MaterialPath, (data) => { terrain.materialTemplate = data; }));
                                        viewObj.Obj = view;
                                    }
                                }
                            }
                            else //移除
                            {
                                if (self.DynamicSceneObjectMapCount.ContainsKey(index))
                                {
                                    self.DynamicSceneObjectMapCount[index]--;
                                }
                                else
                                {
                                    self.DynamicSceneObjectMapCount[index] = -1;
                                }

                                //不需要显示但有
                                if (self.DynamicSceneObjectMapCount[index] <= 0 && self.DynamicSceneObjectMapObj.ContainsKey(index))
                                {
                                    // Log.Info("AOISceneView Remove " + index);
                                    self.RecycleObj(index);
                                }
                            }
                        }
                    }

                    await ETTaskHelper.WaitAll(tasks);
                }
                
                temp.Dispose();
                self.LastGridX = x;
                self.LastGridY = y;
                //加载完成，关闭loading界面
                // await Game.EventSystem.PublishAsync(new UIEventType.LoadingFinish());
                zoneScene.GetComponent<ObjectWait>().Notify(new WaitType.Wait_LoadAOISceneFinish());
                self.Busing = false;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        public static void RecycleObj(this AOISceneViewComponent self,int index,bool remove=true)
        {
            var viewObj = self.DynamicSceneObjectMapObj[index];
            if (viewObj.Obj == null) //还在加载
            {
                viewObj.IsLoading = false;
            }
            else
            {
                viewObj.Obj.SetActive(false);
                if (viewObj.Type == "Prefab")
                    GameObjectPoolComponent.Instance.RecycleGameObject(viewObj.Obj);
                else if (viewObj.Type == "Terrain")
                {
                    var collider = viewObj.Obj.GetComponent<TerrainCollider>();
                    ResourcesComponent.Instance.ReleaseAsset(collider.terrainData);
                    GameObject.Destroy(viewObj.Obj);
                }

                if (remove)
                {
                    self.DynamicSceneObjectMapObj.Remove(index);
                }
            }

        }

        public static List<int> GetCellMapObjects(this AOISceneViewComponent self, long id)
        {
            if (!self.CurMap.CellMapObjects.TryGetValue(id, out var res))
            {
                var arr = self.CurMap.CellIds;
                int left = 0;
                int right = arr.Count - 1;

                while (left<=right)
                {
                    int mid = left + (right - left) / 2;
                    if (id < arr[mid])
                    {
                        right = mid - 1;
                    }
                    else if (id > arr[mid])
                    {
                        left = mid + 1;
                    }
                    else
                    {
                        self.CurMap.CellMapObjects[id] = self.CurMap.MapObjects[mid].Value;
                        //找到了直接返回
                        res = self.CurMap.CellMapObjects[id];
                        break;
                    }
                }
            }
            return res;
        }
    }
}