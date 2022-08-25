using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Reflection;
using UnityEngine.Profiling;

public class FindReferences : EditorWindow
{
    static Dictionary<string, System.Collections.Generic.List<string>> refDic = new Dictionary<string, System.Collections.Generic.List<string>>();
    static bool showWin = true;
    /// <summary>
    /// 查询目标文件夹下资源的使用（.prefab/.unity/.mat/.asset）情况，标记出资源使用为0的
    /// </summary>
    static public void Find(bool canSeleted = true,bool _showWin = true)
    {

        showWin = _showWin;

        EditorSettings.serializationMode = SerializationMode.ForceText;

        //在这里定义查找文件的类型和文件的路径
        //string[] guids = Selection.assetGUIDs;
        string defaulPath = "Assets/AssetsPackage";
        if ((Selection.objects.Length > 0)&& canSeleted)
        {
            defaulPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
        }

        string path = Path.Combine(Application.dataPath, "AssetsPackage");
        if (canSeleted)
        {
            path = EditorUtility.OpenFolderPanel("选择要分析哪个文件夹资源", defaulPath, "");
        }
        FindByArtToolsWindow(path);
    }

    //可视化
    public static void FindByArtToolsWindow(string path)
    {
        string[] assetPathsFromFolder = Directory.GetFiles(path,"*.*", SearchOption.AllDirectories);
        FindAssetsByArtToolsWindow(assetPathsFromFolder);
    }

    public static void FindAssetsByArtToolsWindow(string [] assetsPath)
    {
        System.Collections.Generic.List<string> pathList = new System.Collections.Generic.List<string>();
        foreach (var tempPath in assetsPath)
        {
            if (!tempPath.EndsWith(".meta"))
            {
                pathList.Add( AssetDatabase.AssetPathToGUID(GetRelativeAssetsPath(tempPath)) );
            }
        }
        string[] guids = pathList.ToArray<string>();
        if (guids.Length < 1)
        {
            EditorUtility.DisplayDialog("提示", "请选择要查找的资源", "确定");
            return;
        }

        Dictionary<string, int> checkAssetPaths = new Dictionary<string, int>();

        foreach (var item in guids)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(item);
            if (!checkAssetPaths.ContainsKey(assetPath))
            {
                if (Directory.Exists(assetPath))
                {
                    string[] fileStrs = Directory.GetFiles(Path.GetFullPath(assetPath), "*.*", SearchOption.AllDirectories);
                    foreach(var fileItem in fileStrs)
                    {
                        if (!string.Equals(Path.GetExtension(fileItem), ".meta",System.StringComparison.OrdinalIgnoreCase))
                        {
                            checkAssetPaths.Add(fileItem, 1);
                        }
                    }
                }
                else
                {
                    if (!string.Equals(Path.GetExtension(assetPath), ".meta",System.StringComparison.OrdinalIgnoreCase))
                    {
                        checkAssetPaths.Add(assetPath, 1);
                    }
                }
            }
        }
        
        string[] allassetpaths = new string[checkAssetPaths.Count];
        int index = 0;
        foreach (var keyvalue in checkAssetPaths)
        {
            allassetpaths[index] = keyvalue.Key;
            index++;
        }

        int totalGuid = index;
        refDic.Clear();

        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower()) && !s.Contains("AddressableAssetsData") ).ToArray();
        
        MatchFile(allassetpaths, index, files, refDic);
    }


    [MenuItem("Tools/Find References", true)]
    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }

    static private void MatchFile(string[] allassetpaths, int index, string[] files, Dictionary<string, System.Collections.Generic.List<string>> refDic)
    {
        if (index > 0 && !string.IsNullOrEmpty(allassetpaths[index - 1]))
        {
            string guid = AssetDatabase.AssetPathToGUID(GetRelativeAssetsPath(allassetpaths[index - 1]));
            int startIndex = 0;
            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    if (!refDic.ContainsKey(guid))
                    {
                        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                        list.Add(file);
                        refDic.Add(guid, list);
                    }
                    else
                    {
                        refDic[guid].Add(file);
                    }
                }
                else
                {
                    if (!refDic.ContainsKey(guid))
                    {
                        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                        refDic.Add(guid, list);
                    }
                }

                startIndex++;
                
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    //Debug.Log("匹配结束");
                    if ((index - 1) == 0)
                    {
                        //Debug.Log("匹配结束");
                        //OutputUnuse(refDic);
                        FindCSFileReferences(allassetpaths,refDic);
                    }
                    else
                    {
                        MatchFile(allassetpaths, index - 1, files, refDic);
                    }
                }
            };
        }
    }
    

    //查找CS文件对资源的引用
    static private void FindCSFileReferences(string[] allassetpaths, Dictionary<string, System.Collections.Generic.List<string>> refDic)
    {
        string[] csFiles = Directory.GetFiles("Codes", "*.cs", SearchOption.AllDirectories)
            .Where(s => s.IndexOf("/editor/", System.StringComparison.OrdinalIgnoreCase) <0 ).ToArray();
        MatchFileContents(allassetpaths,allassetpaths.Length,csFiles,refDic,2);
    }



    ///output 0 输出  1  CSFile
    static private void MatchFileContents(string[] allassetpaths, int index, string[] files, Dictionary<string, System.Collections.Generic.List<string>> refDic,int output,bool isSpecial = false)
    {
        if (index > 0 && !string.IsNullOrEmpty(allassetpaths[index - 1]))
        {
            string guid = AssetDatabase.AssetPathToGUID(GetRelativeAssetsPath(allassetpaths[index - 1]));
            string fileName = Path.GetFileName(allassetpaths[index - 1]);
            int startIndex = 0;
            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
                
                bool bValid  = false; 
                string content = File.ReadAllText(file);

                if(isSpecial)
                {
                    string[] contentList = content.Split(new string[] { ",\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach(var contentItem in contentList)
                    {
                        //Debug.Log(contentItem);
                        bValid  = Regex.IsMatch(fileName, contentItem);
                        if(bValid){
                            break;
                        }
                    }
                }else{
                    bValid = Regex.IsMatch(content, fileName);
                }


                if (bValid)
                {
                    if (!refDic.ContainsKey(guid))
                    {
                        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                        list.Add(file);
                        refDic.Add(guid, list);
                    }
                    else
                    {
                        refDic[guid].Add(file);
                    }
                }
                else
                {
                    if (!refDic.ContainsKey(guid))
                    {
                        System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
                        refDic.Add(guid, list);
                    }
                }

                startIndex++;
                
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    //Debug.Log("匹配结束");
                    if ((index - 1) == 0)
                    {
                        //Debug.Log("匹配结束");
                        OutputUnuse(refDic);
                    }
                    else
                    {
                        MatchFileContents(allassetpaths, index - 1, files, refDic,output,isSpecial);
                    }
                }
            };
        }
    }





    static private void OutputUnuse(Dictionary<string, System.Collections.Generic.List<string>> refDic)
    {
        if (showWin)
        {
            FindReferences window = (FindReferences)EditorWindow.GetWindow(typeof(FindReferences));
            window.Show();
        }
        else
        {
            OutputToExcel();
        }
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
                if (item.Value.Count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    var assetPath = AssetDatabase.GUIDToAssetPath(item.Key);
                    Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                    GUILayout.Label(assetPath,GUILayout.Width(350));
                    EditorGUILayout.ObjectField("", assetObj, typeof(Object), true, GUILayout.Width(120));
                    GUILayout.Label("内存占用：" + EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(assetObj)), GUILayout.Width(150));

                    if (Rule01(assetPath))
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
                    Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                    GUILayout.Label(assetPath,GUILayout.Width(350));
                    EditorGUILayout.ObjectField("", assetObj, typeof(Object), true, GUILayout.Width(100));
                    showList[index] = EditorGUILayout.Toggle("", showList[index], GUILayout.Width(20));

                    GUILayout.Label("内存占用：" + EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(assetObj)), GUILayout.Width(150));

                    if (Rule01(assetPath))
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
                        Rect r = EditorGUILayout.BeginVertical("Button");
                        foreach (var fileName in item.Value)
                        {
                            assetPath = GetRelativeAssetsPath(fileName);
                            assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label(assetPath,GUILayout.Width(350));
                            EditorGUILayout.ObjectField("", assetObj, typeof(Object), true,GUILayout.Width(120));
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


    static public string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }


    /// <summary>
    /// 资源分析
    /// 分析Assets\AssetsPackage目录下的所有资源，输出excel
    /// </summary>
    static public void ResourceAnalysis()
    {
        Find(false,false);
        //OutputToExcel();
        //Object target = Selection.activeObject as Object;
        //System.Type type = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");
        //MethodInfo methodInfo = type.GetMethod("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
        //Debug.Log("内存占用：" + EditorUtility.FormatBytes(UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(target)));
        //Debug.Log("硬盘占用：" + EditorUtility.FormatBytes((int)methodInfo.Invoke(null, new object[] { target })));
    }
    

    static private void OutputToExcel()
    {
        //输出excel
        Debug.Log("=========================开始输出excel");

        //指定默认路径
        //string path = Application.dataPath + "/" + "UIPrefabExcel.xlsx";
        //自己选择一个路径
        System.DateTime time = System.DateTime.Now;
        string timeStr = time.Year.ToString("D4")  + "_" + time.Month.ToString("D2") + time.Day.ToString("D2") + "_" + time.Hour.ToString("D2") + time.Minute.ToString("D2");
        string path = EditorUtility.SaveFilePanel("Save Excel File", "", timeStr + ".xlsx", "xlsx");
        FileInfo newFile = new FileInfo(path);
        if (newFile.Exists)
        {
            newFile.Delete();
            newFile = new FileInfo(path);
        }

        //通过ExcelPackage打开文件
        using (ExcelPackage package = new ExcelPackage(newFile))
        {
            //添加sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("sheet1");
            //添加列名
            worksheet.Cells[1, 1].Value = "资源名字";
            worksheet.Cells[1, 2].Value = "被引用数量";
            worksheet.Cells[1, 3].Value = "内存占用";
            worksheet.Cells[1, 4].Value = "Atlas超过256*256";

            var index = 2;

            foreach (var item in refDic)
            {
                //if (item.Value.Count == 0)
                //{
                var assetPath = AssetDatabase.GUIDToAssetPath(item.Key);
                worksheet.Cells["A" + index].Value = assetPath;
                worksheet.Cells["B" + index].Value = item.Value.Count;
                Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                worksheet.Cells["C" + index].Value = EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(assetObj));
                worksheet.Cells["D" + index].Value = Rule01(assetPath)?"是":"否";
                index++;
                //}
            }

            //foreach (var item in refDic)
            //{
            //    if (item.Value.Count > 0)
            //    {
            //        var assetPath = AssetDatabase.GUIDToAssetPath(item.Key);
            //        worksheet.Cells["A"+ index].Value = assetPath;
            //        worksheet.Cells["B"+ index].Value = item.Value.Count;
            //        Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            //        worksheet.Cells["C" + index].Value = EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(assetObj));
            //        index++;
            //    }
            //}

            package.Save();
            Debug.Log("Create Success!");
        }
    }

    /// <summary>
    /// DiscreteImages 目录下面的图不能超过 256 * 256
    /// </summary>
    public static bool Rule01(string assetPath)
    {
        if (assetPath.Contains(AltasHelper.AtlasName))
        {
            Texture assetObj = (Texture)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture));
            if(assetObj == null)
            {
                return false;
            }
            if(assetObj.width > 256 || assetObj.height > 256)
            {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
        return false;
    }

}