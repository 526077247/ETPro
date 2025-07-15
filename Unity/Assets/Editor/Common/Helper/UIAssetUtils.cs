using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class UIAssetUtils
{
    private static string[] paths = { "UICommonGame", "UIGames", "UIHall", "UI" };//要查询的路径

    /// <summary> 
    /// 获取所有的Prefab路径
    /// </summary> 
    /// <param name="fullPath">全路径或相对unity的路径</param>    
    public static List<string> GetAllPrefabs(bool fullPath = true)
    {
        List<string> prefabPaths = new List<string>();
        string[] searchFullPaths = InitPaths();
        for (int i = 0; i < searchFullPaths.Length; i++)
        {
            var tempList = CollectFiles(searchFullPaths[i], "*.prefab");
            if (tempList != null)
            {
                if (fullPath)
                {
                    prefabPaths.AddRange(tempList);
                }
                else
                {
                    for (int j = 0; j < tempList.Length; j++)
                    {
                        prefabPaths.Add(ChangeFilePath(tempList[j]));
                    }
                }
                
            } 
        }

        return prefabPaths;
    }

    /// <summary> 
    /// 获取所有的Images路径
    /// </summary> 
    /// <param name="fullPath">全路径或相对unity的路径</param>    
    public static List<string> GetAllImages(bool fullPath = true)
    {
        List<string> imagePaths = new List<string>();
        string[] searchFullPaths = InitPaths();
        for (int i = 0; i < searchFullPaths.Length; i++)
        {
            List<string> curPathImages = new List<string>();
	        var tempList = CollectFiles(searchFullPaths[i], "*.png");
	        if(tempList==null) continue;
            curPathImages.AddRange(tempList);
            tempList = CollectFiles(searchFullPaths[i], "*.jpg");
            curPathImages.AddRange(tempList);
            if (curPathImages != null)
            {
                if (fullPath)
                {
                    imagePaths.AddRange(curPathImages);
                }
                else
                {
                    for (int j = 0; j < curPathImages.Count; j++)
                    {
                        imagePaths.Add(ChangeFilePath(curPathImages[j]));
                    }
                }

            }
        }

        return imagePaths;
    }

   

    public static string[] CollectFiles(string directory, string format)
	{
		if(!Directory.Exists(directory)) return null;
        string[] files = Directory.GetFiles(directory, format, SearchOption.AllDirectories);
        return files;
    }


    public static string ChangeFilePath(string path)
    {
        path = path.Replace("\\", "/");
        path = path.Replace(Application.dataPath + "/", "");
        path = "Assets/" + path;

        return path;
    }

    private static string[] InitPaths()
    {
        string[] fullPath = new string[paths.Length];
        for (int i = 0; i < fullPath.Length; i++)
        {
            fullPath[i] = Path.Combine(Application.dataPath, "AssetsPackage", paths[i]);
        }

        return fullPath;
    }
}
