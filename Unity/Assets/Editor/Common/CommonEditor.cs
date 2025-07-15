using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class CommonEditor
    {
        [MenuItem("Tools/帮助/启动场景 #_b")]
        static void ChangeInitScene()
        {
            EditorApplication.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
        }
        
        [MenuItem("Tools/工具/创建子目录", false, 101)]
        [MenuItem("Assets/工具/创建子目录", false, 101)]
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
}
