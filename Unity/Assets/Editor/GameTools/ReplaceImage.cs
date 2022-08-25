using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Linq;

public class ReplaceImage : EditorWindow
{
    Sprite a = null;
    Sprite b = null;
    bool isDone = false;
    private GameObject curOpenPrefab = null;
    private string curOpenPrefabKey = null;
    private string curSelectTextKey = null;
    private Vector2 scrollPosition = Vector2.zero;
    Dictionary<string, List<string>> record = new Dictionary<string, List<string>>();
    Dictionary<string, List<string>> problems = new Dictionary<string, List<string>>();
    List<string> res = new List<string>();

    private void ResetData()
    {
        curOpenPrefabKey = null;
        curSelectTextKey = null;
        record.Clear();
        problems.Clear();
        res.Clear();
        isDone = false;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("搜索图片:", GUILayout.Width(70));
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        a = EditorGUI.ObjectField(rect, (Object)a, typeof(Sprite), false) as Sprite;

        GUILayout.Label("", GUILayout.Width(20));
        GUILayout.Label("替换图片", GUILayout.Width(70));
        b = EditorGUILayout.ObjectField((Object)b, typeof(Sprite), false) as Sprite;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("搜索"))
        {
            if (a == null)
            {
                this.ShowNotification(new GUIContent("请检查信息是否为空！"));
                return;
            }

            this.ResetData();
            this.Search();
        }

        if (GUILayout.Button("开始"))
        {
            if (a == null || b == null )
            {
                this.ShowNotification(new GUIContent("请检查信息是否为空！"));
                return;
            }

            this.ResetData();
            this.Start();
        }

        if (isDone)
        {

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUI.backgroundColor = Color.white;
            foreach (var kv in record)
            {
                string title = kv.Key;
                DrawPrefabItem(title, kv.Value);
            }

            if (record.Count > 0 && problems.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("--------------------------------------------------------------------------------", GUILayout.Width(800));
                EditorGUILayout.EndHorizontal();
            }
            if (problems.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("不能判断的Prefab", GUILayout.Width(800));
                EditorGUILayout.EndHorizontal();
            }

            foreach (var kv in problems)
            {
                string title = kv.Key;
                DrawPrefabItem(title, kv.Value);
            }

            if (this.res.Count > 0)
            {

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(800));
                EditorGUILayout.EndHorizontal();
            }

            foreach (var path in res)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.TextField(path, GUILayout.Width(800));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }


        if ((Event.current.type == EventType.DragUpdated
                || Event.current.type == EventType.DragExited)
                && rect.Contains(Event.current.mousePosition))
        {

            if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            {
                
            }
        }
    }

    private void DrawPrefabItem(string title, List<string> list)
    {

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
            foreach (var v in list)
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
    private void Search()
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

            Image[] Images = prefab.GetComponentsInChildren<Image>(true);
            for (int k = 0; k < Images.Length; k++)
            {
                if (Images[k].sprite == a)
                {
                    AddRecord(cur, Images[k].transform);
                }
            }

            Button[] Buttons = prefab.GetComponentsInChildren<Button>(true);
            for (int k = 0; k < Buttons.Length; k++)
            {
                Button btn = Buttons[k];
                if (btn != null)
                {
                    if (btn.transition == Selectable.Transition.SpriteSwap)
                    {
                        int counter = 0;
                        SpriteState state = btn.spriteState;
                        if (state.highlightedSprite == a)
                        {
                            counter++;
                        }

                        if (state.pressedSprite == a)
                        {
                            counter++;
                        }

                        if (state.selectedSprite == a)
                        {
                            counter++;
                        }

                        if (state.disabledSprite == a)
                        {
                            counter++;
                        }

                        if (counter > 0)
                        {
                            AddRecord(cur, Buttons[k].transform);
                        }
                    }
                }
            }

            EditorUtility.DisplayProgressBar("Prefab进度", (i + 1) + "/" + listString.Count, (float)(i + 1) / listString.Count);
        }

        EditorUtility.ClearProgressBar();
        this.ShowNotification(new GUIContent("搜索完成！"));
        isDone = true;
    }
    private void Start()
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

            bool isChange = false;
            Image[] Images = prefab.GetComponentsInChildren<Image>(true);
            for (int k = 0; k < Images.Length; k++)
            {
                if (Images[k].sprite == a)
                {
                    //替換圖片，添加記錄
                    if (b != null)
                    {
                        if (Images[k].type == Image.Type.Filled || Images[k].type == Image.Type.Tiled)
                        {
                            AddProblem(cur, Images[k].transform);
                            continue;
                        }

                        if (b.border.w > 0 || b.border.x > 0 || b.border.y > 0 || b.border.z > 0)
                        {
                            Images[k].type = Image.Type.Sliced;
                        }
                        else
                        {
                            Images[k].type = Image.Type.Simple;
                        }
                        Images[k].sprite = b;
                        isChange = true;
                        AddRecord(cur, Images[k].transform);
                    }
                }
            }

            Button[] Buttons = prefab.GetComponentsInChildren<Button>(true);
            for (int k = 0; k < Buttons.Length; k++)
            {
                Button btn = Buttons[k];
                if (btn != null)
                {
                    if (btn.transition == Selectable.Transition.SpriteSwap)
                    {
                        int counter = 0;
                        SpriteState state = btn.spriteState;
                        if (state.highlightedSprite == a)
                        {
                            counter++;
                            state.highlightedSprite = b;
                        }

                        if (state.pressedSprite == a)
                        {
                            counter++;
                            state.pressedSprite = b;
                        }

                        if (state.selectedSprite == a)
                        {
                            counter++;
                            state.selectedSprite = b;
                        }

                        if (state.disabledSprite == a)
                        {
                            counter++;
                            state.disabledSprite = b;
                        }

                        if (counter > 0)
                        {
                            btn.spriteState = state;
                            isChange = true;
                            AddRecord(cur, Buttons[k].transform);
                        }
                    }
                }
            }

            if (isChange)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
            EditorUtility.DisplayProgressBar("Prefab进度", (i + 1) + "/" + listString.Count, (float)(i + 1) / listString.Count);
        }

        EditorUtility.ClearProgressBar();
        this.ShowNotification(new GUIContent("替换完成！"));
    }

    void AddRecord(string cur, Transform tran)
    {
        if (!record.ContainsKey(cur))
        {
            record.Add(cur, new List<string>());
        }

        record[cur].Add(GetProblemFullPath(tran));
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
