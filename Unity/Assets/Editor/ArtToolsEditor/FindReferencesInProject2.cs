using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class FindReferencesInProject2 : EditorWindow
{
    //public const string MenuItemName = "Tools/ArtTools/查找资源映射";
    private const string MetaExtension = ".meta";
    static Dictionary<string, System.Collections.Generic.List<string>> refDic = new Dictionary<string, System.Collections.Generic.List<string>>();
    
    //[MenuItem(MenuItemName, false, 209)]
    public static List<string> FindByRG(string searchStr,string searchPath,bool showWin=true,bool clearData = true)
    {
        bool isMacOS = Application.platform == RuntimePlatform.OSXEditor;
        int totalWaitMilliseconds = isMacOS ? 2 * 1000 : 300 * 1000;
        int cpuCount = Environment.ProcessorCount;
        //string appDataPath = Path.Combine(Application.dataPath, AddressableTools.Assets_Package );

        //var selectedObject = Selection.activeObject;
        //string selectedAssetPath = AssetDatabase.GetAssetPath(selectedObject);
        //string selectedAssetGUID = AssetDatabase.AssetPathToGUID(selectedAssetPath);
       // string selectedAssetMetaPath = selectedAssetPath + MetaExtension;




        var references = new List<string>();
        var output = new System.Text.StringBuilder();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var psi = new ProcessStartInfo();
        psi.WindowStyle = ProcessWindowStyle.Minimized;

        if (isMacOS)
        {
            psi.FileName = "/usr/bin/mdfind";
            psi.Arguments = string.Format("-onlyin {0} {1}", searchPath, searchStr);
        }
        else
        {
            psi.FileName = Path.Combine(Environment.CurrentDirectory, @"Assets\Editor\ArtTools\rg.exe");
            psi.Arguments = string.Format("--case-sensitive --follow --files-with-matches --no-text --fixed-strings " +
                                          "--ignore-file Assets/Editor/ArtTools/ignore.txt " +
                                          "--threads {0} --regexp {1} -- {2}",
                cpuCount, searchStr, searchPath);
        }

        psi.UseShellExecute = false;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        var process = new Process();
        process.StartInfo = psi;

        process.OutputDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            string relativePath = e.Data.Replace(searchPath, "Assets").Replace("\\", "/");

            // skip the meta file of whatever we have selected
            if (relativePath.EndsWith(MetaExtension))
                return;

            references.Add(relativePath);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            output.AppendLine("Error: " + e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        while (!process.HasExited)
        {
            if (stopwatch.ElapsedMilliseconds < totalWaitMilliseconds)
            {
                float progress = (float)((double)stopwatch.ElapsedMilliseconds / totalWaitMilliseconds);
                string info = string.Format("Finding {0}/{1}s {2:P2}", stopwatch.ElapsedMilliseconds / 1000,
                    totalWaitMilliseconds / 1000, progress);
                bool canceled = EditorUtility.DisplayCancelableProgressBar("Find References in Project", info, progress);

                if (canceled)
                {
                    process.Kill();
                    break;
                }

                Thread.Sleep(100);
            }
            else
            {
                process.Kill();
                break;
            }
        }
/*
        foreach (string file in references)
        {
            string guid = AssetDatabase.AssetPathToGUID(file);
            output.AppendLine(string.Format("{0} {1}", guid, file));

            string assetPath = file;
            if (file.EndsWith(MetaExtension))
            {
                assetPath = file.Substring(0, file.Length - MetaExtension.Length);
            }

            UnityEngine.Debug.Log(string.Format("{0}\n{1}", file, guid), AssetDatabase.LoadMainAssetAtPath(assetPath));
        }
*/
        EditorUtility.ClearProgressBar();
        stopwatch.Stop();


/*

        string content = string.Format(
            "{0} {1} found for object: \"{2}\" path: \"{3}\" guid: \"{4}\" total time: {5}s\n\n{6}",
            references.Count, references.Count > 2 ? "references" : "reference", selectedObject.name, selectedAssetPath,
            selectedAssetGUID, stopwatch.ElapsedMilliseconds / 1000d, output);
        UnityEngine.Debug.LogWarning(content, selectedObject);
*/
        if(clearData)
        {
            refDic.Clear();
        }

        refDic.Add(searchStr,references);

        if(showWin)
        {
            FindReferencesInProject2 window = (FindReferencesInProject2)EditorWindow.GetWindow(typeof(FindReferencesInProject2));
            window.Show();
        }
        return references;
    }

    public static void FindAssetsByRG(string path)
    {
        string[] assetPathsFromFolder = Directory.GetFiles(path,"*.*", SearchOption.AllDirectories);

        System.Collections.Generic.List<string> pathList = new System.Collections.Generic.List<string>();
        foreach (var tempPath in assetPathsFromFolder)
        {
            if (!tempPath.EndsWith(".meta"))
            {
                pathList.Add( AssetDatabase.AssetPathToGUID( FindReferences.GetRelativeAssetsPath(tempPath)) );
            }
        }

        if (pathList.Count < 1)
        {
            EditorUtility.DisplayDialog("提示", "请选择要查找的资源", "确定");
            return;
        }

        //string appDataPath = Path.Combine(Application.dataPath, AddressableTools.Assets_Package );
        //for(int i=0;i<pathList.Count;i++)
        //{
        //    if(i == (pathList.Count - 1))
        //    {
        //        FindReferencesInProject2.FindByRG(pathList[i],appDataPath,true,false);
        //    }
        //    else if(i == 0)
        //    {
        //        FindReferencesInProject2.FindByRG(pathList[i],appDataPath,false,true);
        //    }
        //    else
        //    {
        //        FindReferencesInProject2.FindByRG(pathList[i],appDataPath,false,false);
        //    }
        //}
    }


    bool showNoUser = true;
    bool showUser = true;
    System.Collections.Generic.List<bool> showList = new System.Collections.Generic.List<bool>();
    Vector2 scrollPos;
    void OnGUI()
    {

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

        showNoUser = EditorGUILayout.Toggle("无引用的资源：", showNoUser);
        if (showNoUser)
        {
            Rect r = EditorGUILayout.BeginVertical("Button");
            foreach (var item in refDic)
            {
                //UnityEngine.Debug.Log(item.Value.Count + "===========" + item.Key);
                if (item.Value.Count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    var assetPath = AssetDatabase.GUIDToAssetPath(item.Key);
                    UnityEngine.Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                    GUILayout.Label(assetPath,GUILayout.Width(350));
                    EditorGUILayout.ObjectField("", assetObj, typeof(UnityEngine.Object), true, GUILayout.Width(120));
                    GUILayout.Label("内存占用：" + EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(assetObj)), GUILayout.Width(150));

                    if (FindReferences.Rule01(assetPath))
                    {
                        GUIStyle fontStyle = new GUIStyle();
                        fontStyle.normal.textColor = new Color(1, 0, 0);   //设置字体颜色  
                        fontStyle.fixedWidth = 100;
                        GUILayout.Label("bigImage", fontStyle);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            
        }


        showUser = EditorGUILayout.Toggle("有引用的资源：", showUser);
        if (showUser)
        {
            var index = 0;
            foreach (var item in refDic)
            {
                if (item.Value.Count > 0)
                {
                    if(showList.Count <= index)
                    {
                        showList.Add(true);
                    }

                    //有引用的资源
                    EditorGUILayout.BeginHorizontal();
                    var assetPath = AssetDatabase.GUIDToAssetPath(item.Key);
                    UnityEngine.Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                    GUILayout.Label(assetPath,GUILayout.Width(350));
                    EditorGUILayout.ObjectField("", assetObj, typeof(UnityEngine.Object), true, GUILayout.Width(100));
                    showList[index] = EditorGUILayout.Toggle("", showList[index], GUILayout.Width(20));

                    GUILayout.Label("内存占用：" + EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(assetObj)), GUILayout.Width(150));

                    if (FindReferences.Rule01(assetPath))
                    {
                        GUIStyle fontStyle = new GUIStyle();
                        fontStyle.normal.textColor = new Color(1, 0, 0);   //设置字体颜色  
                        fontStyle.fixedWidth = 100;
                        GUILayout.Label("bigImage", fontStyle);
                    }

                    //GUILayout.Label("被下面" + item.Value.Count + "个资源引用:");
                    EditorGUILayout.EndHorizontal();

                    if (showList[index])
                    {
                        foreach (var fileName in item.Value)
                        {
                            assetPath = FindReferences.GetRelativeAssetsPath(fileName);
                            assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label(assetPath,GUILayout.Width(350));
                            EditorGUILayout.ObjectField("", assetObj, typeof(UnityEngine.Object), true,GUILayout.Width(120));
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    index++;
                }
            }
        }

        EditorGUILayout.EndScrollView();
        
    }




/*
    [MenuItem(MenuItemName, true)]
    private static bool FindValidate()
    {
        var obj = Selection.activeObject;
        if (obj != null && AssetDatabase.Contains(obj))
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return !AssetDatabase.IsValidFolder(path);
        }

        return false;
    }
  */  
}