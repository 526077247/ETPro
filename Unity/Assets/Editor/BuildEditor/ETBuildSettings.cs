using UnityEditor;
using UnityEngine;

namespace ET
{
    public class ETBuildSettings : ScriptableObject
    {
        public bool clearFolder = false;
        public bool isBuildExe = true;
        public bool isContainAB = false;
        public bool buildHotfixAssembliesAOT = true;
        public bool buildResourceAll = false;
        public BuildType buildType = BuildType.Release;
        public BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
    }
}
