//art tools 可视化
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Reflection;
using UnityEngine.Profiling;
using System;
using Object = UnityEngine.Object;
public class ArtToolsWindow : EditorWindow
{

    private static bool showIllegalImage = false;
    private static Data data;
    private static Data selectData;
    private static bool showWin = false;
    private static string searchWord = "";

    [InitializeOnLoadMethod]
    private static void InitializeOnLoadMethod()
    {
        initFileData();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        initFileData();
    }


    //打开Art Tools window
    public static void OpenWindow()
    {
        showWin = true;
        //ArtToolsWindow window = (ArtToolsWindow)EditorWindow.GetWindow(typeof(ArtToolsWindow));
        //window.Show();
        initFileData();
    }

    void OnDestroy()
    {
        showWin = false;
    }

    Vector2 scrollPos;

    void OnGUI()
    {
        
        GUILayout.Label("提示1:标红说明图片大小超过256*256");
        GUILayout.Label("提示2:第一个是资源在硬盘大小");
        GUILayout.Label("提示3:第二个是资源在内存大小");
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();


        
        searchWord = EditorGUILayout.TextField("", searchWord, GUILayout.Width(200));
        if (GUILayout.Button("搜索", GUILayout.Width(100)))
        {
            searchData();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("刷新", GUILayout.Width(100)))
        {
            initFileData();
        }


        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 100));
        
        showFileView();

        EditorGUILayout.EndScrollView();
    }


//========================================================================================
        private static EditorApplication.CallbackFunction _updateDelegate;
		public delegate long ThreadRun(ThreadPars par);
		private const int ThreadCount = 4;
        public class ThreadPars
		{
            public List<Data> ChildDataList = new List<Data>();
            public List<string> DataPathList = new List<string>();
		}

        private static long ThreadFind(ThreadPars par)
		{
			long ret = 0;
            if (par != null)
			{
                for(int i=0;i<par.ChildDataList.Count;i++)
                {
                    ret += LoadFilesByThread(par.ChildDataList[i], par.DataPathList[i] ,1);
                }
            }
			return ret;
		}


   private static long LoadFilesByThread (Data data,string currentPath, int indent=0)
	{
        if(currentPath.EndsWith(".meta"))
        {
            return 0;
        }

		//GUIContent content = GetGUIContent (currentPath);
        long dataSize = 0;
		//if (content != null) {
			data.indent = indent;
			//data.content = content;
			data.assetPath = currentPath;
            //data.isIllegalImage = ValidateImage(currentPath);
            //data.fileMemorySize = MemorySize(currentPath);
		//}
 
		foreach (var path in Directory.GetFiles(currentPath)) {
            if(path.EndsWith(".meta"))
            {
                continue;
            }

			//content = GetGUIContent (path);
			//if (content != null) {
				Data child = new Data ();
				child.indent = indent + 1;
				//child.content = content;
				child.assetPath = path;
               // child.isIllegalImage = ValidateImage(path);
                //child.fileMemorySize = MemorySize(path);
                //FileInfo fi = new FileInfo(path);
                //child.fileSize = fi.Length;
                //dataSize += fi.Length;
                child.fileSize = FileSize(path);
                dataSize += child.fileSize;
				data.childs.Add (child);
			//}
		}
 
 
		foreach (var path in Directory.GetDirectories(currentPath)) {
			Data childDir = new Data ();
			data.childs.Add (childDir);
			dataSize += LoadFilesByThread (childDir,path, indent + 1);
		}

        data.fileSize = dataSize;
        return dataSize;
	}






    private static long LoadFilesThread (Data data,string currentPath, int indent=0)
	{
		GUIContent content = GetGUIContent (currentPath);
        long dataSize = 0;
		if (content != null) {
			data.indent = indent;
			data.content = content;
			data.assetPath = currentPath;
            data.isIllegalImage = ValidateImage(currentPath);
            data.fileMemorySize = MemorySize(currentPath);
		}
 
		foreach (var path in Directory.GetFiles(currentPath)) {
			content = GetGUIContent (path);
			if (content != null) {
				Data child = new Data ();
				child.indent = indent + 1;
				child.content = content;
				child.assetPath = path;
                child.isIllegalImage = ValidateImage(path);
                child.fileMemorySize = MemorySize(path);
                //FileInfo fi = new FileInfo(path);
                //child.fileSize = fi.Length;
                //dataSize += fi.Length;
                child.fileSize = FileSize(path);
                dataSize += child.fileSize;
				data.childs.Add (child);
			}
		}

        ThreadPars[] threadParses = new ThreadPars[ThreadCount];
		for (int index = 0; index < ThreadCount; index++)
		{
			threadParses[index] = new ThreadPars();
		}

        var i = 0;
		foreach (var path in Directory.GetDirectories(currentPath)) {
			Data childDir = new Data ();
			data.childs.Add (childDir);

            int index = i % ThreadCount;
			threadParses[index].ChildDataList.Add(childDir);
            threadParses[index].DataPathList.Add(path);
            i++;
			//dataSize += LoadFilesThread (childDir,path, indent + 1);
		}

 
		ThreadRun[] tRun = new ThreadRun[ThreadCount];
		int finishedState = ThreadCount;
		IAsyncResult[] results = new IAsyncResult[ThreadCount];





        _updateDelegate = delegate
				{
					var finishedCount = 0;
					for (int j = 0; j < ThreadCount; j++)
					{
						if (results[j].IsCompleted) ++finishedCount;
					}
					
					EditorUtility.DisplayProgressBar("匹配资源中",string.Format("进度：{0}",finishedCount),(float)finishedCount/ThreadCount);
 
					if (finishedCount >= finishedState)
					{
						 
						for (int j = 0; j < ThreadCount; j++)
						{
							dataSize += tRun[j].EndInvoke(results[j]);
                            
						}
						EditorUtility.ClearProgressBar();
						EditorApplication.update -= _updateDelegate;


                        ArtToolsWindow window = (ArtToolsWindow)EditorWindow.GetWindow(typeof(ArtToolsWindow));
                        window.Show();
                        data.fileSize = dataSize;
                        //LogData();
					}
				};
 
				for (int j = 0; j < ThreadCount; j++)
				{
					tRun[j] = ThreadFind;
					results[j] = tRun[j].BeginInvoke(threadParses[j], null, null);
				}
 
				EditorApplication.update += _updateDelegate;


        data.fileSize = dataSize;
        return dataSize;
	}



//==========================================================================================


    //初始化
    private static void initFileData()
    {
        if(!showWin)
        {
            return;
        }
        string path = Path.Combine("Assets", "AssetsPackage");
        data = new Data ();
		LoadFilesThread (data,path);
    }


    private static void RunThread()
    {
        //int cpuCount = Environment.ProcessorCount;
/*
ParameterizedThreadStart threadStart=new ParameterizedThreadStart(Calculate)
Thread thread=new Thread(threadStart) 
thread.Start(0.9);//参数是0.9
public void Calculate(object arg)//arg参数是0.9
{
double Diameter=double(arg);
Console.Write("The Area Of Circle with a Diameter of {0} is {1}"Diameter,Diameter*Math.PI);
}
*/

    }


    //显示 
    private static void showFileView()
    {
        if(data == null)
        {
            return;
        }
		GUI.enabled = true;
		EditorGUIUtility.SetIconSize (Vector2.one * 16);
		DrawData (data);
    }


    private static void LogData()
    {
        LogDrawData (data);
    }


    private static void LogDrawData(Data data)
    {
        Debug.Log( data.indent + "===" + data.assetPath);
        for (int i = 0; i < data.childs.Count; i++) {
            LogDrawData(data.childs[i]);
        }
    }

    private static void DrawData(Data data)
	{
        if (!data.isSearch)
        {
            return;
        }

        if (!data.isCreateContent)
        {
            data.isCreateContent = true;
            data.content = GetGUIContent (data.assetPath);
            data.isIllegalImage = ValidateImage(data.assetPath);
            data.fileMemorySize = MemorySize(data.assetPath);
        }

		if (data.content != null) {
			EditorGUI.indentLevel = data.indent;
			DrawGUIData (data);
 
		}
		for (int i = 0; i < data.childs.Count; i++) {
			Data child = data.childs[i];

            if(!child.isCreateContent &&!string.IsNullOrEmpty(child.assetPath))
            {
                child.isCreateContent = true;
                child.content = GetGUIContent (child.assetPath);
                child.isIllegalImage = ValidateImage(child.assetPath);
                child.fileMemorySize = MemorySize(child.assetPath);
            }

			if (child.content != null) {
				EditorGUI.indentLevel = child.indent;
				if(child.childs.Count>0 && child.isExpand)
                {
					DrawData (child);
                }
				else
                {
				    DrawGUIData (child);
                }
			}
		}
	}
 
 
	private static void DrawGUIData (Data data)
	{
        if(!data.isSearch)
        {
            return;
        }
EditorGUILayout.BeginHorizontal();


        if(!data.isCreateContent)
        {
            data.isCreateContent = true;
            data.content = GetGUIContent (data.assetPath);
            data.isIllegalImage = ValidateImage(data.assetPath);
            data.fileMemorySize = MemorySize(data.assetPath);
        }
        
        GUIStyle style = "Label";
		Rect rt = GUILayoutUtility.GetRect (data.content, style);
		if (data.isSelected) {
			EditorGUI.DrawRect (rt, Color.gray);
		}

        if(data.isIllegalImage)
        {
            EditorGUI.DrawRect (rt, Color.red);
        }
 
		rt.x += (16 * EditorGUI.indentLevel);
		if (GUI.Button (rt, data.content, style)) {
			if (selectData != null) {
				selectData.isSelected = false;
			}
			data.isSelected = true;
			selectData = data;
            data.isExpand = !data.isExpand;
		}



        GUILayout.Label( EditorUtility.FormatBytes( data.fileSize ),GUILayout.Width(60) );
        GUILayout.Label( EditorUtility.FormatBytes( data.fileMemorySize ),GUILayout.Width(60) );
        
        if(GUILayout.Button("引用",GUILayout.Width(60)))
        {
            FindSelfReferences(data.assetPath);
        }


        if(GUILayout.Button("被引用",GUILayout.Width(60)))
        {
            FindReferencesByAssets(data.assetPath);
        }

EditorGUILayout.EndHorizontal();
	}


    //查找引用了哪些资源
    private static void FindSelfReferences(string assetPath)
    {
        /*
        Dictionary<string, string[]> tempRefDic = new Dictionary<string,string[]>();
        if(Directory.Exists(assetPath))
        {
            string dstAssetPath = FindReferences.GetRelativeAssetsPath(assetPath);
            tempRefDic = Finddependent.FindAllFolderDependent(assetPath,dstAssetPath);
        }
        else
        {
            string[] a = Finddependent.GetAssetDependencies(assetPath);
            tempRefDic.Add(assetPath,a);
        }
        foreach(var keyvalue in tempRefDic)
        {
            Debug.Log("==================" + keyvalue.Key + "====================");
            foreach (var fileName in keyvalue.Value)
            {
                Debug.Log("==================" + fileName + "====================");
            }
        }*/

        if(Directory.Exists(assetPath))
        {
            Finddependent.FindFolderDependentByArtToolsWindow(assetPath);
        }
        else
        {
            Finddependent.FindAssetDependentByArtToolsWindow(assetPath);
        }
    }


    //查找本资源被哪些资源引用
    private static void FindReferencesByAssets(string assetPath)
    {
       /* if(Directory.Exists(assetPath))
        {
            FindReferencesInProject2.FindAssetsByRG(assetPath);
        }
        else
        {
            string appDataPath = Path.Combine(Application.dataPath, AddressableTools.Assets_Package );
            string selectedAssetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            FindReferencesInProject2.FindByRG(selectedAssetGUID,appDataPath);
        }
*/
        
  /*      
        if(Directory.Exists(assetPath))
        {
            FindReferences.FindByArtToolsWindow(assetPath);
        }
        else
        {
            string[] sArray = {assetPath};
            FindReferences.FindAssetsByArtToolsWindow(sArray);
        }
     */
        FindReferences01.FindThread(assetPath);

    }


    private static long LoadFiles (Data data,string currentPath, int indent=0)
	{
		GUIContent content = GetGUIContent (currentPath);
        long dataSize = 0;
		if (content != null) {
			data.indent = indent;
			data.content = content;
			data.assetPath = currentPath;
            data.isIllegalImage = ValidateImage(currentPath);
            data.fileMemorySize = MemorySize(currentPath);
		}
 
		foreach (var path in Directory.GetFiles(currentPath)) {
			content = GetGUIContent (path);
			if (content != null) {
				Data child = new Data ();
				child.indent = indent + 1;
				child.content = content;
				child.assetPath = path;
                child.isIllegalImage = ValidateImage(path);
                child.fileMemorySize = MemorySize(path);
                //FileInfo fi = new FileInfo(path);
                //child.fileSize = fi.Length;
                //dataSize += fi.Length;
                child.fileSize = FileSize(path);
                dataSize += child.fileSize;
				data.childs.Add (child);
			}
		}
 
 
		foreach (var path in Directory.GetDirectories(currentPath)) {
			Data childDir = new Data ();
			data.childs.Add (childDir);
			dataSize += LoadFiles (childDir,path, indent + 1);
		}

        data.fileSize = dataSize;
        return dataSize;
	}

    //硬盘占用
    private static long FileSize(string path)
    {
        //Texture target = (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
        //if(target == null)//不是图片
        //{
            FileInfo fi = new FileInfo(path);
            return fi.Length;
        //}
       // var type = System.Reflection.Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TextureUtil");//  Types.GetType ("UnityEditor.TextureUtil", "UnityEditor.dll");
       // MethodInfo methodInfo = type.GetMethod ("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
       // return (int)methodInfo.Invoke(null,new object[]{target});
    }

    private static long MemorySize(string path)
    {
        Object assetObj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        return Profiler.GetRuntimeMemorySizeLong(assetObj);
    }

    private static bool ValidateImage(string path)
    {
        if (path.Contains(AltasHelper.AtlasName))
        {
            Texture assetObj = (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
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
    }

	private static GUIContent GetGUIContent (string path)
	{
		Object asset = AssetDatabase.LoadAssetAtPath (path, typeof(Object));  
		if (asset) {
			return new GUIContent (asset.name,	AssetDatabase.GetCachedIcon (path));
		}
		return null;
	}

    private static void searchData()
    {
        searchDataPath(data, searchWord);
    }

    private static bool searchDataPath(Data searchData,string _searchWord)
    {
        bool isSearch = false;

        if (_searchWord == null || _searchWord == "")
        {
            isSearch = true;
        }
        else
        {
            isSearch = searchData.assetPath.Contains(_searchWord);
        }

        foreach(var item in searchData.childs)
        {
            bool hasSearch = searchDataPath(item, _searchWord);
            if(hasSearch)
            {
                isSearch = hasSearch;
            }
        }
        searchData.isSearch = isSearch;
        return isSearch;
    }

    //文件夹数据
    public class Data
	{
		public bool isSelected = false;
		public int indent =0;
		public GUIContent content;
		public string assetPath;
		public System.Collections.Generic.List<Data> childs = new System.Collections.Generic.List<Data> ();
        public long fileSize = 0;  //文件在硬盘大小
        public long fileMemorySize = 0;//文件在内存大小
        public bool isIllegalImage = false;
        public bool isExpand = false; 
        public bool isCreateContent = false;
        public bool isSearch = true;
	}


}