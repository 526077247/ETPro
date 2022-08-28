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
    public static partial class HybridCLR
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
            
            var localIl2cppDir = BuildConfig.LocalIl2CppDir;
            if (!Directory.Exists(localIl2cppDir))
            {
                Debug.LogError($"本地il2cpp目录:{localIl2cppDir} 不存在，未安装本地il2cpp。请手动执行一次 {BuildConfig.HybridCLRDataDir} 目录下的 init_local_il2cpp_data.bat 或者 init_local_il2cpp_data.sh 文件");
                return false;
            }
            Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", localIl2cppDir);
            return true;
        }

    }
}
