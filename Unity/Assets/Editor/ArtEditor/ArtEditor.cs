using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class ArtEditor
    {
        [MenuItem("Tools/工具/UI/设置图片", false, 31)]
        public static void SettingPNG()
        {
            AtlasHelper.SettingPNG();
        }
        
        [MenuItem("Tools/工具/UI/生成图集", false, 32)]
        public static void ClearAllAtlasAndGenerate()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                AtlasHelper.GeneratingAtlas();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        
        [MenuItem("Tools/工具/TA/Fbx压缩工具", false, 54)]
        [MenuItem("Assets/工具/TA/Fbx压缩工具", false, 54)]
        public static void ShowFbxToolWindow()
        {
            FbxHelperWindow.ShowWindow();
        }
        
        [MenuItem("Tools/工具/TA/资源分析输出excel", false, 202)]
        public static void ResourceAnalysis()
        {
            ResourceCheckTool.ResourceAnalysis();
        }
        
        [MenuItem("Tools/工具/TA/资源可视化窗口", false, 208)]
        public static void OpenWindow()
        {
            ArtToolsWindow.OpenWindow();
        }

        [MenuItem("Tools/工具/UI/创建图片字体", false, 500)]
        [MenuItem("Assets/工具/UI/创建图片字体", false, 203)]
        public static void CreateArtFont()
        {
            ArtistFont.BatchCreateArtistFont();
        }
        
        [MenuItem("Tools/工具/UI/搜索或批量替换Sprite", false, 503)]
        public static void ReplaceImage()
        {
            Rect _rect = new Rect(0, 0, 900, 600);
            ReplaceImage window = EditorWindow.GetWindowWithRect<ReplaceImage>(_rect, true, "搜索或批量替换Sprite");
            window.Show();
        }

        [MenuItem("Tools/工具/UI/查找未使用的图片", false, 503)]
        public static void CheckUnUseImage()
        {
            Rect _rect = new Rect(0, 0, 900, 600);
            CheckUnuseImage window = EditorWindow.GetWindowWithRect<CheckUnuseImage>(_rect, true, "查找未使用的图片");
            window.Show();
        }

        [MenuItem("Tools/工具/UI/检查丢失image", false, 504)]
        public static void CheckLossImage()
        {
            Rect _rect = new Rect(0, 0, 900, 600);
            CheckEmptyImage window = EditorWindow.GetWindowWithRect<CheckEmptyImage>(_rect, true, "检查预设丢失image");
            window.Show();
        }
    }
}