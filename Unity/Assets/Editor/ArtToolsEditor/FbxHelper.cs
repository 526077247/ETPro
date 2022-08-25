using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public class FbxHelperWindow : EditorWindow
{
    private List<FileInfo> m_fbxFileInfo = new List<FileInfo>();
    private List<string> m_paths = new List<string>();

    public static void ShowWindow()
    {

        FbxHelperWindow window = GetWindowWithRect<FbxHelperWindow>(new Rect(0, 0, 600, 600));
        window.titleContent = new GUIContent("动画工具");
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        GUILayout.Label("可以选中文件夹、FBX文件、anim文件");
        m_paths = getSelectedPaths();
        m_fbxFileInfo = FilterFbxFile(m_paths);
        if (m_fbxFileInfo.Count == 0)
        {
            GUILayout.Label("当前没有选中任何fbx文件");
        }
        else
        {
            int maxLen = 20;
            for (int i = 0; i < m_fbxFileInfo.Count; i++)
            {
                if(i < maxLen)
                {
                    GUILayout.Label("当前选中：" + m_fbxFileInfo[i].FullName);
                }
                else
                {
                    GUILayout.Label("后续fbx文件省略显示。。。");
                    break;
                }
            }
        }


        if (GUILayout.Button("从FBX导出anim并压缩"))
        {
            if (m_paths.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没找到fbx文件", "确定");
                return;
            }
            for (int i = 0; i < m_paths.Count; i++)
            {
                HandleFBX(m_paths[i]);
            }

            EditorUtility.DisplayDialog("提示", "导出结束", "确定");
        }
        if (GUILayout.Button("压缩anim文件"))
        {
            if (m_paths.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没选中", "确定");
                return;
            }
            for (int i = 0; i < m_paths.Count; i++)
            {
                HandleAnim(m_paths[i]);
            }
            EditorUtility.DisplayDialog("提示", "导出结束", "确定");
        }

        EditorGUILayout.EndVertical();
    }
    private void OnInspectorUpdate()
    {

    }
    void OnSelectionChange()
    {
        this.Repaint();
    }

    /// <summary>
    /// //获取当前选中目录
    /// </summary>


    List<string> getSelectedPaths()
    {
        List<string> seledtFile = new List<string>(); // 保存选中的文件、文件夹
        UnityEngine.Object[] arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
        for (int i = 0; i < arr.Length; i++)
        {
            seledtFile.Add(AssetDatabase.GetAssetPath(arr[i]));
        }
        return seledtFile;
    }

    /// <summary>
    /// // 根据选中内容，获取所有fbx文件
    /// </summary>
    private List<FileInfo> FilterFbxFile(List<string> selectFile)
    {
        List<FileInfo> fbxFileInfo = new List<FileInfo>();
        foreach (string fileItem in selectFile)
        {
            if (FileTools.IsExistDirectory(fileItem))
            {
                // 获取文件具体信息
                DirectoryInfo fbxDir = new DirectoryInfo(fileItem);
                if (fbxDir == null)
                {
                    continue;
                }
                //选中的是目录
                List<FileInfo> filesInfos = new List<FileInfo>(fbxDir.GetFiles("*.FBX", SearchOption.AllDirectories));
                fbxFileInfo.AddRange(filesInfos);
            }
            else if (FileTools.IsExistFile(fileItem))
            {
                if (fileItem.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    fbxFileInfo.Add(new FileInfo(fileItem));
                }
            }
            else
            {
                Debug.LogWarning(fileItem + ":本路径既不是目录也不是文件，请确认好！！！");
            }
        }
        return fbxFileInfo;
    }


    /// <summary>
    /// 从源目录的FBX文件中提取Animation到目标目录
    /// </summary>
    /// <param name="source_path"> 源路径</param>

    public static void HandleFBX(string source_path)
    {

        DirectoryInfo fbxDir = new DirectoryInfo(source_path);
        if (fbxDir == null)
        {
            EditorUtility.DisplayDialog("提示", "请选择正确的目录，当前选中:" + source_path, "确定");
            return;
        }

        List<FileInfo> filesInfos = new List<FileInfo>();


        if (File.GetAttributes(source_path).CompareTo(FileAttributes.Directory) == 0)
        {
            //选中的是目录
            filesInfos = new List<FileInfo>(fbxDir.GetFiles("*.FBX", SearchOption.AllDirectories));
        }
        else
        {
            if (source_path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            {
                filesInfos.Add(new FileInfo(source_path));
            }
        }
        ChangeFBXToAnimAndCompess(filesInfos);
    }
    public static void HandleAnim(string source_path)
    {
        DirectoryInfo fbxDir = new DirectoryInfo(source_path);
        if (fbxDir == null)
        {
            EditorUtility.DisplayDialog("提示", "当前路径不存在:" + source_path, "确定");
            return;
        }

        List<string> filesInfos = new List<string>();


        if (File.GetAttributes(source_path).CompareTo(FileAttributes.Directory) == 0)
        {
            //选中的是目录
            filesInfos = new List<string>(FileTools.GetFileNames(source_path, "*.anim", true));//// new List<FileInfo>(Fi.GetFiles("*.FBX", SearchOption.AllDirectories));
        }
        else
        {
            if (source_path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
            {
                filesInfos.Add(source_path);
            }
        }

        for (int i = 0; i < filesInfos.Count; i++)
        {

            if (filesInfos[i].EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
            {
                ReplaceAniClipAsset(filesInfos[i]);
            }
        }
    }

    static private void ChangeFBXToAnimAndCompess(List<FileInfo> filesInfos)
    {
        int iAniClipNum = 0;
        AnimationClip kAC = null;
        string strFileName, strExtention, strFbxAssetPath;
        int progress = 0;
        float total = filesInfos.Count;
        foreach (FileInfo fileInfo in filesInfos)
        {
            progress++;
            string parentDir = fileInfo.Directory.FullName;
            parentDir = parentDir.Substring(parentDir.IndexOf("Assets"));

            strFileName = fileInfo.Name;
            strExtention = fileInfo.Extension;
            strFbxAssetPath = fileInfo.FullName;

            strFbxAssetPath = strFbxAssetPath.Substring(strFbxAssetPath.IndexOf("Assets"));

            ModelImporter kMI = ModelImporter.GetAtPath(strFbxAssetPath) as ModelImporter;
            if (kMI == null)
            {
                Debug.LogError("null:" + strFbxAssetPath);
                continue;
            }
            //kMI.animationType = ModelImporterAnimationType.Legacy;

            //过滤
            if (fileInfo.Name.IndexOf("_skin", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            string targetDir = parentDir + "/" + "Animations";
            FileTools.CreateDirectory(targetDir);

            UnityEngine.Object[] arrObj = AssetDatabase.LoadAllAssetRepresentationsAtPath(strFbxAssetPath);
            foreach (UnityEngine.Object obj in arrObj)
            {
                kAC = obj as AnimationClip;
                if (kAC != null)
                {
                    string strClipAssetPath = targetDir + "/" + kAC.name + ".anim";
                    CreateAniClipAsset(kAC, strClipAssetPath);
                    iAniClipNum++;
                    EditorUtility.DisplayProgressBar("导出动画并压缩", strFbxAssetPath, progress / total);
                }
            }
        }
        EditorUtility.ClearProgressBar();
        if (iAniClipNum > 0)
        {
            //string strContext = String.Format("导出完成，共导出{0}个动画文件", iAniClipNum);
            //EditorUtility.DisplayDialog("导出动画文件", strContext, "确定");
        }
    }

    static private void CreateAniClipAsset(AnimationClip kSrcAniClip, string strClipAssetPath)
    {
        AnimationClip compessClip = GameObject.Instantiate(kSrcAniClip);
        compessClip.name = kSrcAniClip.name;

        compess(compessClip);

        AnimationClip clip = AssetDatabase.LoadAssetAtPath(strClipAssetPath, typeof(AnimationClip)) as AnimationClip;

        if (clip != null)
        {
            //已存在，直接替换
            EditorUtility.CopySerialized(compessClip, clip);
        }
        else
        {
            //新建
            AnimationClip kDstAniClip = new AnimationClip();
            EditorUtility.CopySerialized(compessClip, kDstAniClip);
            AssetDatabase.CreateAsset(kDstAniClip, strClipAssetPath);
        }
        Debug.Log("压缩：" + strClipAssetPath);
        Resources.UnloadUnusedAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    static private void ReplaceAniClipAsset(string strClipAssetPath)
    {
        Debug.Log("压缩：" + strClipAssetPath);
        AnimationClip kSrcAniClip = AssetDatabase.LoadAssetAtPath(strClipAssetPath, typeof(AnimationClip)) as AnimationClip;

        compess(kSrcAniClip);

        Resources.UnloadUnusedAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }

    public static void compess(AnimationClip clip)
    {
        optmizeAnimationScaleCurve(clip);
        optmizeAnimationFloat(clip);
    }

    /// <summary>
    /// 优化scale曲线
    /// </summary>
    static AnimationClip optmizeAnimationScaleCurve(AnimationClip clip)
    {
        EditorCurveBinding[] bs = AnimationUtility.GetCurveBindings(clip);
        foreach (EditorCurveBinding theCurveBinding in bs)
        {
            if (theCurveBinding.propertyName.IndexOf("scale", StringComparison.OrdinalIgnoreCase)<0)
            {
                continue;
            }

            bool needDelete = true;

            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, theCurveBinding);
            Keyframe[] keyFrames = curve.keys;

            for (int j = 0; j < keyFrames.Length; j++)
            {
                Keyframe key = keyFrames[j];

                float a = Mathf.Abs(key.value - 1);
                float b = Mathf.Abs(key.inTangent - 0);
                float c = Mathf.Abs(key.outTangent - 0);
                float delta = 0.001f;
                if (a > delta || b > delta || c > delta)
                {
                    needDelete = false;
                    break;
                }
            }

            if (needDelete)
            {
                AnimationUtility.SetEditorCurve(clip, theCurveBinding, null);
            }
        }
        return clip;
    }
    static AnimationClip optmizeAnimationFloat(AnimationClip clip)
    {
        if (clip == null)
            return clip;
        try
        {
            AnimationClipCurveData[] curves = null;
            curves = AnimationUtility.GetAllCurves(clip);
            Keyframe key;
            Keyframe[] keyFrames;
            for (int i = 0; i < curves.Length; ++i)
            {
                AnimationClipCurveData curveDate = curves[i];
                if (curveDate.curve == null || curveDate.curve.keys == null)
                {
                    continue;
                }
                keyFrames = curveDate.curve.keys;
                for (int j = 0; j < keyFrames.Length; j++)
                {
                    key = keyFrames[j];
                    key.value = float.Parse(key.value.ToString("f3"));
                    key.inTangent = float.Parse(key.inTangent.ToString("f3"));
                    key.outTangent = float.Parse(key.outTangent.ToString("f3"));
                    keyFrames[j] = key;
                }
                curveDate.curve.keys = keyFrames;
                clip.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
            }
            return clip;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return clip;
        }
    }

}
