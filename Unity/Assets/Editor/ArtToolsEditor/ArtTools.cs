using System.IO;
using UnityEditor;
using UnityEngine;

public class ArtTools
{
    //创建子目录
    public static void CreateArtSubFolder()
    {
        string[] ArtFolderNames = { "Animations", "Materials", "Models", "Textures", "Prefabs" };
        string[] UIFolderNames = { "Animations", "Atlas", "DiscreteImages", "Prefabs","DynamicAtlas" };
        string[] guids = Selection.assetGUIDs;
        for (int i = 0; i < guids.Length; i++)
        {
            string selectPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Debug.Log(selectPath);
            if (FileTools.IsExistDirectory(selectPath))
            {
                var names = ArtFolderNames;
                selectPath.Replace("\\", "/");
                if (selectPath.Contains("UI/") || selectPath.Contains("UIHall/") || selectPath.Contains("UIGames/"))
                {
                    names = UIFolderNames;
                }
                for (int j = 0; j < names.Length; j++)
                {
                    string folderPath = Path.Combine(selectPath, names[j]);
                    Debug.Log(folderPath);
                    FileTools.CreateDirectory(folderPath);
                }
            }
            else
            {
                Debug.Log(selectPath + " is not a directory");
            }

        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
