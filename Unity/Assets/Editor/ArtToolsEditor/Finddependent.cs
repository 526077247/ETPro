using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Finddependent : EditorWindow
{
    static Dictionary<string, string[]> refDic = new Dictionary<string, string[]>();
    Vector2 scrollPos;
    System.Collections.Generic.List<bool> showList = new System.Collections.Generic.List<bool>();
    //查找某个资源的依赖
    public static void FindAssetDependent()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        FindAssetDependentByArtToolsWindow(path);
    }


    //可视化使用
    public static void FindAssetDependentByArtToolsWindow(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("查找依赖", "请选中你要查找的资源", "确定");
            return;
        }
        string[] a = GetAssetDependencies(path);

        if (a == null)
        {
            EditorUtility.DisplayDialog("查找依赖", "你要查找的资源不存在", "确定");
            return;
        }

        refDic.Clear();

        refDic.Add(path, a);

        Finddependent window = (Finddependent)EditorWindow.GetWindow(typeof(Finddependent));
        window.Show();
    }


    public static void FindFolderDependent()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string defaulPath = "Assets/AssetsPackage";
        string path = EditorUtility.OpenFolderPanel("选择要分析哪个文件夹资源", defaulPath, "");
        FindFolderDependentByArtToolsWindow(path);
    }

    //可视化使用
    public static void FindFolderDependentByArtToolsWindow(string path)
    {
        string dstAssetPath = FindReferences.GetRelativeAssetsPath(path);
        refDic = FindAllFolderDependent(path,dstAssetPath);
        Finddependent window = (Finddependent)EditorWindow.GetWindow(typeof(Finddependent));
        window.Show();
    }


    public static Dictionary<string, string[]> FindAllFolderDependent(string path,string dstAssetPath)
    {
        string[] assetPathsFromFolder = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        string assetPath = "";
        Dictionary<string, string[]> tempRefDic = new Dictionary<string, string[]>();

        foreach (var tempPath in assetPathsFromFolder)
        {
            if (!tempPath.EndsWith(".meta"))
            {
                assetPath = FindReferences.GetRelativeAssetsPath(tempPath);
                string[] a = GetAssetDependencies(assetPath);
                List<string> filterPath = new List<string>();
               /* for (int j = 0; j < a.Length; j++)
                {
                    //过滤掉本身目录，过滤掉.shader .cs
                    if (a[j].Contains(dstAssetPath) || a[j].Contains(".cs") || a[j].Contains(".shader"))
                    {
                    }
                    else
                    {
                        filterPath.Add(a[j]);
                    }
                }
                if (filterPath.Count != 0)
                {
                    tempRefDic.Add(assetPath, filterPath.ToArray());
                }*/
                tempRefDic.Add(assetPath, a);
            }
        }

        return tempRefDic;
    }


    public static string[] GetAssetDependencies(string assetPath)
    {
        if (!File.Exists(assetPath))
            return null;
        string dstAssetPath = FindReferences.GetRelativeAssetsPath(assetPath);
        string[] dependecies = AssetDatabase.GetDependencies(assetPath);
        List<string> filterPath = new List<string>();
        for (int j = 0; j < dependecies.Length; j++)
        {
            if(dependecies[j].Contains("Assets/AssetsPackage") && !dependecies[j].Contains(dstAssetPath))
            {
                filterPath.Add(dependecies[j]);
            }
        }

        dependecies = filterPath.ToArray();
        return dependecies;
    }


    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
        int index = 0;


        string dir = "";
        Dictionary<string, int> dirDic = new Dictionary<string, int>();

        foreach (var keyvalue in refDic)
        {

            if (showList.Count <= index)
            {
                showList.Add(true);
            }

            EditorGUILayout.BeginHorizontal();
            Object assetObj = AssetDatabase.LoadAssetAtPath(keyvalue.Key, typeof(Object));
            GUILayout.Label(keyvalue.Key,GUILayout.Width(350));
            EditorGUILayout.ObjectField("" , assetObj, typeof(Object), true, GUILayout.Width(120));
            showList[index] = EditorGUILayout.Toggle("", showList[index], GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();

            if (showList[index])
            {
                Rect r = EditorGUILayout.BeginVertical("Button");
                foreach (var fileName in keyvalue.Value)
                {
                    assetObj = AssetDatabase.LoadAssetAtPath(fileName, typeof(Object));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(fileName,GUILayout.Width(350));
                    EditorGUILayout.ObjectField("", assetObj, typeof(Object), true,GUILayout.Width(120));
                    // EditorGUILayout.TextField("", fileName);
                    EditorGUILayout.EndHorizontal();

                    dir = Path.GetDirectoryName(fileName);
                    if (!dirDic.ContainsKey(dir))
                    {
                        dirDic.Add(dir, 1);
                    }
                }
                EditorGUILayout.EndVertical();

            }
            index++;
        }

        GUILayout.Label("引用的文件夹：");
        EditorGUILayout.BeginVertical();
        foreach (var item in dirDic)
        {
            Object dirObj = AssetDatabase.LoadAssetAtPath(item.Key, typeof(Object));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(item.Key,GUILayout.Width(350));
            EditorGUILayout.ObjectField("", dirObj, typeof(Object), true, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

    }

}
