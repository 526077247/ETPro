using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace ET
{
    public class ArtToolsEditor
    {
        [MenuItem("Tools/帮助/启动场景 #_b")]
        static void ChangeInitScene()
        {
            EditorApplication.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
        }
        
        [MenuItem("Tools/ArtTools/创建子目录", false, 101)]
        [MenuItem("Assets/ArtTools/创建子目录", false, 101)]
        public static void CreateArtSubFolder()
        {
            ArtTools.CreateArtSubFolder();
        }

        [MenuItem("Tools/ArtTools/Fbx压缩工具", false, 54)]
        [MenuItem("Assets/ArtTools/Fbx压缩工具", false, 54)]
        public static void ShowFbxToolWindow()
        {
            FbxHelperWindow.ShowWindow();
        }
        
        [MenuItem("Tools/ArtTools/资源分析输出excel", false, 202)]
        public static void ResourceAnalysis()
        {
            FindReferences01.ResourceAnalysis();
        }
        
        [MenuItem("Tools/ArtTools/资源可视化窗口", false, 208)]
        public static void OpenWindow()
        {
            ArtToolsWindow.OpenWindow();
        }

        [MenuItem("Tools/ArtTools/创建图片字体", false, 500)]
        [MenuItem("Assets/ArtTools/创建图片字体", false, 203)]
        public static void CreateArtFont()
        {
            ArtistFont.BatchCreateArtistFont();
        }
    }
}
