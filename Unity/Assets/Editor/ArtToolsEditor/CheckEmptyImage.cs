using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheckEmptyImage : EditorWindow
{
    Dictionary<string, List<string>> problems = new Dictionary<string, List<string>>();
    private Vector2 scrollPosition = Vector2.zero;
    private bool isDone = false;
    private GameObject curOpenPrefab = null;
    private string curOpenPrefabKey = null;
    private string curSelectTextKey = null;

    private void OnGUI()
    {

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("开始"))
        {
            isDone = false;
            problems.Clear();
            this.curOpenPrefab = null;
            this.curOpenPrefabKey = null;
            this.curSelectTextKey = null;
            this.Start();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);


        if (isDone)
        {

            GUI.backgroundColor = Color.white;
            foreach (var kv in problems)
            {

                //GUILayout.Label("------------------------------------------------------------------------------------------------------------", GUILayout.Width(800));
                string title = kv.Key;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Prefab:", GUILayout.Width(60));
                GUILayout.Label(title, GUILayout.Width(600));
                if (GUILayout.Button("打开"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(title);

                    EditorGUIUtility.PingObject(prefab);
                    AssetDatabase.OpenAsset(prefab);
                    this.curOpenPrefab = Selection.activeGameObject; //打开时选择的是打开的prefab的根目录，所以这样直接取了
                    this.curOpenPrefabKey = title;
                    this.curSelectTextKey = null;

                }
                EditorGUILayout.EndHorizontal();

                if (this.curOpenPrefabKey == title)
                {
                    foreach (var v in kv.Value)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(120));
                        GUILayout.TextField(v, GUILayout.Width(580));
                        if (this.curSelectTextKey == v)
                        {
                            GUILayout.Label("☚", GUILayout.Width(20));//辨别当前选中
                        }

                        if (GUILayout.Button("选择") && this.curOpenPrefab != null)
                        {
                            Transform se = this.curOpenPrefab.transform.Find(v);
                            EditorGUIUtility.PingObject(se.gameObject);
                            Selection.activeGameObject = se.gameObject;
                            curSelectTextKey = v;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

        }

        EditorGUILayout.EndScrollView();
    }

    public void Start()
    {
        List<string> listString = UIAssetUtils.GetAllPrefabs(false);

        for (int i = 0; i < listString.Count; i++)
        {
            string cur = listString[i];

            AssetImporter tmpAssetImporter = AssetImporter.GetAtPath(cur);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(tmpAssetImporter.assetPath);

            if (prefab == null)
            {
                Debug.LogError("空的预设 ： " + tmpAssetImporter.assetPath);
                continue;
            }

            Image[] images = prefab.GetComponentsInChildren<Image>(true);

            for (int k = 0; k < images.Length; k++)
            {
                SerializedObject so = new SerializedObject(images[k]);
                SerializedProperty iterator = so.GetIterator();
                //获取所有属性
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        //引用对象是null 并且 引用ID不是0 说明丢失了引用
                        if (iterator.objectReferenceValue == null && iterator.objectReferenceInstanceIDValue != 0)
                        {
                            AddProblem(cur, images[k].transform);
                        }

                    }
                }
            }

            EditorUtility.DisplayProgressBar("进度", (i + 1) + "/" + listString.Count, (float)(i + 1) / listString.Count);

        }

        EditorUtility.ClearProgressBar();
        isDone = true;
        this.ShowNotification(new GUIContent("查找完成！"));
    }

    void AddProblem(string cur, Transform tran)
    {
        if (!problems.ContainsKey(cur))
        {
            problems.Add(cur, new List<string>());
        }

        problems[cur].Add(GetProblemFullPath(tran));
    }

    string GetProblemFullPath(Transform tran)
    {
        string path = tran.name;

        while (tran.parent != null)
        {
            if (tran.parent.parent == null) break; //不要根

            path = tran.parent.name + "/" + path;
            tran = tran.parent;
        }

        return path;
    }
}
