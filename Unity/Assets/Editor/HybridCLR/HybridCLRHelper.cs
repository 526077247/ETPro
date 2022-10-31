using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ET;
using UnityEditor;
using UnityEngine;
namespace HybridCLR
{
    public static class HybridCLRHelper
    {
        public static bool IsWolong;
        public static bool Setup()
        {
            EditorApplication.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
            var init = UnityEngine.Object.FindObjectOfType<Init>();
            IsWolong = init.CodeMode == CodeMode.Wolong;
            if (!IsWolong)
            {
                Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", "");
                return true;
            }
            // unity允许使用UNITY_IL2CPP_PATH环境变量指定il2cpp的位置，因此我们不再直接修改安装位置的il2cpp，
            // 而是在本地目录
            
            var localIl2cppDir = HybridCLR.Editor.SettingsUtil.LocalIl2CppDir;
            if (!Directory.Exists(localIl2cppDir))
            {
                Debug.LogError($"本地il2cpp目录:{localIl2cppDir} 不存在，未安装本地il2cpp。请手动执行一次 {Editor.SettingsUtil.HybridCLRDataDir} 目录下的 init_local_il2cpp_data.bat 或者 init_local_il2cpp_data.sh 文件");
                return false;
            }
            Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", localIl2cppDir);
            return true;
        }

        /// <summary>
        /// 按照必要的顺序，执行所有生成操作，适合打包前操作
        /// </summary>
        [MenuItem("HybridCLR/Generate/All", priority = 200)]
        public static void GenerateAll()
        {
            #region 防裁剪
            FileHelper.CopyDirectory("Codes", "Assets/Codes/Temp");
            AssetDatabase.Refresh();
            #endregion
            
            HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
            
            #region 防裁剪
            Directory.Delete("Assets/Codes/Temp",true);
            File.Delete("Assets/Codes/Temp.meta");
            AssetDatabase.Refresh();
            #endregion
        }
    }
}
