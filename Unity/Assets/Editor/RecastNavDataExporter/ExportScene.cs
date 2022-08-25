using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;

namespace ET
{
    internal class MapExport2Nav: Editor
    {
        private static readonly int CellLen = 10;
        [MenuItem("Tools/NavMesh/导入场景mesh")]
        static void ImportSceneMesh()
        {
            string path = EditorUtility.OpenFilePanel("选择场景json文件", "", "json");
            if (path.Length != 0)
            {
                var jsonStr = File.ReadAllText(path);
                var data = JsonHelper.FromJson<AssetsRoot>(jsonStr);
                EditorApplication.OpenScene("Assets/MapEditor/Map.unity");
                for (int i = 0; i < data.Scenes.Count; i++)
                {
                    var scene = data.Scenes[i];
                    GameObject sceneObj = GameObject.Find(scene.Name);
                    if(sceneObj!=null)
                        DestroyImmediate(sceneObj);
                    sceneObj = new GameObject(scene.Name);
                    for (int j = 0; j < scene.Objects.Count; j++)
                    {
                        var objInfo = scene.Objects[j];
                        GameObject obj = null;
                        string addressPath;
                        switch (objInfo.Type)
                        {
                            case "Prefab":
                                addressPath = "Assets/AssetsPackage/" + objInfo.PrefabPath;
                                var prefab = AssetDatabase.LoadAssetAtPath(addressPath, typeof (GameObject)) as GameObject;
                                if(prefab==null) continue;
                                obj = Instantiate(prefab,sceneObj.transform);
                                obj.name = objInfo.Name;
                                obj.transform.localPosition = objInfo.Transform.Position;
                                obj.transform.localRotation = objInfo.Transform.Rotation;
                                obj.transform.localScale = objInfo.Transform.Scale;
                                
                                break;
                            case "Terrain":
                                addressPath = "Assets/AssetsPackage/" + objInfo.Terrain.TerrainPath;
                                var terrainData =  AssetDatabase.LoadAssetAtPath(addressPath, typeof(TerrainData)) as TerrainData;
                                if(terrainData==null) continue;
                                obj = new GameObject(objInfo.Name);
                                obj.transform.parent = sceneObj.transform;
                                obj.transform.localPosition = objInfo.Transform.Position;
                                obj.transform.localRotation = objInfo.Transform.Rotation;
                                obj.transform.localScale = objInfo.Transform.Scale;
                                
                                obj.AddComponent<MeshFilter>();
                                obj.AddComponent<MeshRenderer>();
                                var mesh = obj.AddComponent<ExportMesh>();
                                mesh.terrainData = terrainData;
                                mesh.Generic();
                                break;
                            default:
                                UnityEngine.Debug.Log("未处理的类型：" + objInfo.Type);
                                break;
                        }
                        if (obj != null)
                        {
                            var mesh = obj.GetComponentsInChildren<MeshFilter>();
                            for (int k = 0; k < mesh.Length; k++)
                            {
                                // if (mesh[k].GetComponent<Spine.Unity.SkeletonAnimation>() != null) continue;
                                mesh[k].gameObject.tag = "NavMesh";
                                mesh[k].gameObject.isStatic = true;
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("Assets/导出场景到Proto数据")]
        static void ExportProto()
        {
            string path = EditorUtility.SaveFilePanel("Save Resource", "", "Map", "bytes");
            if (path.Length != 0)
            {
                if (File.Exists(path)) File.Delete(path);
                AssetsRoot root = GetData();
                using (MemoryStream stream = new MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(stream, root);
                    var bytes = stream.ToArray();
                    // 保存场景数据
                    File.WriteAllBytes(path,bytes);
                }
                // 刷新Project视图
                AssetDatabase.Refresh();
                Debug.Log("导出场景到Proto数据成功");
            }
        }

        [MenuItem("Assets/导出场景到Josn数据")]
        static void ExportJson()
        {
            string path = EditorUtility.SaveFilePanel("Save Resource", "", "Map", "json");
            if (path.Length != 0)
            {
                if (File.Exists(path)) File.Delete(path);
                AssetsRoot root = GetData();
                // 保存场景数据
                File.WriteAllText(path, JsonHelper.ToJson(root));
                // 刷新Project视图
                AssetDatabase.Refresh();
                Debug.Log("导出场景到Josn数据成功");
            }
        }

        static AssetsRoot GetData()
        {
            Object[] selectedAssetList = Selection.GetFiltered(typeof (Object), SelectionMode.DeepAssets);
            // 如果存在场景文件，删除
            
            AssetsRoot root = new AssetsRoot();
            root.Scenes = new List<AssetsScene>();
                
            //遍历所有的游戏对象
            foreach (Object selectObject in selectedAssetList)
            {
                // 场景名称
                string sceneName = selectObject.name;
                // 场景路径
                string scenePath = AssetDatabase.GetAssetPath(selectObject);
                // 打开这个关卡
                EditorApplication.OpenScene(scenePath);
                AssetsScene sceneRoot = new AssetsScene();
                root.Scenes.Add(sceneRoot);
                sceneRoot.Name = sceneName.Split('_')[0];
                sceneRoot.Objects = new List<AssetsObject>();
                sceneRoot.CellMapObjects = new Dictionary<long, List<int>>();

                sceneRoot.MapObjects = new List<AssetsScene.IntList>();
                sceneRoot.CellLen = CellLen;
                foreach (GameObject sceneObject in Object.FindObjectsOfType(typeof (GameObject)))
                {
                    // 如果对象是激活状态
                    if (sceneObject.transform.parent == null && sceneObject.activeSelf)
                    {
                        ChangeObj2Data(sceneObject, sceneRoot);
                    }
                }
                sceneRoot.CellIds= new List<long>(sceneRoot.CellMapObjects.Count);
                sceneRoot.MapObjects = new List<AssetsScene.IntList>(sceneRoot.CellMapObjects.Count);
                foreach (var item in sceneRoot.CellMapObjects)
                {
                    sceneRoot.CellIds.Add(item.Key);
                    sceneRoot.MapObjects.Add(new AssetsScene.IntList(){Value = item.Value});
                }
                // Debug.Log(sceneRoot.CellIds.Count);
                ShellSort(sceneRoot.CellIds, sceneRoot.MapObjects);
            }

            return root;
        }
        public static void ShellSort(List<long> a,List<AssetsScene.IntList> b)
        {
            int n = a.Count;
            int h = 1;
            while (h < n / 3)
                h = h * 3 + 1;
            long temp;
            AssetsScene.IntList temp2;
            while (h >= 1)
            {
                for (int i = h; i < n; i++)
                {
                    for (int j = i; j >= h && a[j]- a[j - h]<0; j -= h)
                    {
                        temp = a[j];
                        temp2 = b[j];
                        a[j] = a[j - h];
                        b[j] = b[j - h];
                        a[j - h] = temp;
                        b[j - h] = temp2;
                    }
                }
                h = h / 3;
            }

        }
        public static void ChangeObj2Data(GameObject sceneObject,AssetsScene root)
        {
            List<AssetsObject> scene = root.Objects;
            var terrain = sceneObject.GetComponent<TerrainCollider>();
            int x = (int)Math.Floor( sceneObject.transform.position.x / CellLen);
            int y = (int)Math.Floor( sceneObject.transform.position.z / CellLen);
            if (terrain != null)
            {
                if(terrain.terrainData==null) return;
                AssetsObject obj = new AssetsObject();
                scene.Add(obj);
                obj.Name = sceneObject.name;
                obj.Type = "Terrain";
                var t = sceneObject.GetComponent<Terrain>();
                string prefabObject = EditorUtility.GetAssetPath(terrain.terrainData);
                string materialObject = EditorUtility.GetAssetPath(t.materialTemplate);
                obj.Terrain = new AssetsTerrain()
                {
                    TerrainPath = prefabObject.Replace("Assets/AssetsPackage/", ""),
                    MaterialPath = materialObject.Replace("Assets/AssetsPackage/", ""),
                };
                obj.Size = terrain.bounds.size;
                AddTransformInfo(obj, sceneObject);
                for (int i = x ; i <= x + obj.Size.x/ CellLen; i++)
                {
                    for (int j = y ; j <= y + obj.Size.z/ CellLen; j++)
                    {
                        var id = AOIHelper.CreateCellId(i, j);
                        // Log.Info("i"+i+"j"+j+" "+sceneObject.Name);
                        if (!root.CellMapObjects.ContainsKey(id))
                        {
                            root.CellMapObjects.Add(id, new List<int>());
                        }

                        root.CellMapObjects[id].Add(scene.Count-1);
                    }
                }
            }
            // 判断是否是预设
            else if (PrefabUtility.GetPrefabType(sceneObject) == PrefabType.PrefabInstance)
            {
                // 获取引用预设对象
                Object prefabObject = EditorUtility.GetPrefabParent(sceneObject);
            
                if (prefabObject != null)
                {
                    AssetsObject obj = new AssetsObject();
                    scene.Add(obj);
                    obj.Name = sceneObject.name;
                    obj.Type = "Prefab";
                    obj.PrefabPath = AssetDatabase.GetAssetPath(prefabObject).Replace("Assets/AssetsPackage/", "");
                    var mesh = sceneObject.GetComponentInChildren<Renderer>();
                    if (mesh != null)
                        obj.Size = mesh.bounds.size;
                    else
                        obj.Size = terrain.transform.localScale;
                    AddTransformInfo(obj, sceneObject);
                    float radius = Mathf.Sqrt(obj.Size.x * obj.Size.x +
                        obj.Size.z * obj.Size.z) / 2;
                    int count = (int) Math.Ceiling(radius / CellLen); //环境多加一格
                    for (int i = x - count; i <= x + count; i++)
                    {
                        var xMin = i * CellLen;
                        for (int j = y - count; j <= y + count; j++)
                        {
                            var yMin = j * CellLen;
                            var res = AOIHelper.GetGridRelationshipWithOBB(sceneObject.transform.position,
                                sceneObject.transform.rotation,
                                obj.Size, CellLen, xMin, yMin, radius, radius * radius);
                            if (res >= 0)
                            {
                                var id = AOIHelper.CreateCellId(i, j);
                                // Log.Info("i"+i+"j"+j+" "+sceneObject.Name);
                                if (!root.CellMapObjects.ContainsKey(id))
                                {
                                    root.CellMapObjects.Add(id, new List<int>());
                                }

                                root.CellMapObjects[id].Add(scene.Count-1);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < sceneObject.transform.childCount; i++)
                {
                    ChangeObj2Data(sceneObject.transform.GetChild(i).gameObject, root);
                }
            }
        }

        public static void AddTransformInfo(AssetsObject obj,GameObject sceneObject)
        {
            AssetsTransform transform = new AssetsTransform();


            // 位置信息
            transform.Position = sceneObject.transform.position;

            // 旋转信息
            transform.Rotation = sceneObject.transform.rotation;

            // 缩放信息
            transform.Scale = sceneObject.transform.localScale;
            obj.Transform = transform;
        }
    }
}