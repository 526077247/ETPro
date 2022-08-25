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

        /*
            [MenuItem("Tools/ArtTools/查找无引用资源", false, 201)]
            [MenuItem("Assets/ArtTools/查找无引用资源", false, 201)]
            public static void FindNotUse()
            {
                FindReferences.Find();
            }
        */
        [MenuItem("Tools/ArtTools/资源分析输出excel", false, 202)]
        public static void ResourceAnalysis()
        {
            FindReferences01.ResourceAnalysis();
        }

        /*  集成到  Tools/ArtTools/可视化窗口
            [MenuItem("Tools/ArtTools/查找asset依赖的资源", false, 203)]
            [MenuItem("Assets/ArtTools/查找asset依赖的资源", false, 203)]
            public static void FindAssetDependent()
            {
                Finddependent.FindAssetDependent();
            }


            [MenuItem("Tools/ArtTools/查找文件夹依赖的文件夹", false, 204)]
            public static void FindFolderDependent()
            {
                Finddependent.FindFolderDependent();
            }
        */

        /*
            [MenuItem("Tools/ArtTools/OpenPlaySize", false, 205)]
            public static void OpenPlaySize()
            {
                FileCapacity.OpenPlaySize();
            }

            [MenuItem("Tools/ArtTools/ClosePlaySize", false, 206)]
            public static void ClosePlaySize()
            {
                FileCapacity.ClosePlaySize();
            }
        */
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
