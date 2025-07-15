using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
	public static class Define
	{
		public const byte KEY = 97;
		public const string BuildOutputDir = "./Temp/Bin/Debug";
		public const string DefaultName = "DefaultPackage";
		public static bool IsSH;
		
		public const string HotfixLoadDir = "Code/Hotfix/";
#if UNITY_ANDROID
        public const string AOTLoadDir = "Code/AOTAndroid/";
#elif UNITY_IOS
        public const string AOTLoadDir = "Code/AOTiOS/";
#elif UNITY_WEBGL
        public const string AOTLoadDir = "Code/AOTWebGL/";
#else //不同BuildTarget的裁剪AOT dll一般不复用。
		public const string AOTLoadDir = "Code/AOT/";
#endif
		public const string HotfixDir = "Assets/AssetsPackage/" + HotfixLoadDir;
		public const string AOTDir = "Assets/AssetsPackage/" + AOTLoadDir;
#if UNITY_EDITOR
		public static readonly bool Debug = true;
#else
        public static readonly bool Debug = false;
#endif
		private const int dWidth = 1366;
		private const int dHeight = 768;
		public static readonly float DesignScreenWidth =
				Screen.width > Screen.height ? Mathf.Max(dWidth, dHeight) : Mathf.Min(dWidth, dHeight);
		public static readonly float DesignScreenHeight =
				Screen.width > Screen.height ? Mathf.Min(dWidth, dHeight) : Mathf.Max(dWidth, dHeight);

#if UNITY_EDITOR && !ASYNC
		public static bool IsAsync = false;
#else
        public static bool IsAsync = true;
#endif
		
#if UNITY_EDITOR
		public static bool IsEditor = true;
#else
        public static bool IsEditor = false;
#endif
#if FORCE_UPDATE // 是否默认强更 该配置项会影响到有无网络对游戏更新流程的改变
		public static bool ForceUpdate = true; //默认强更，无网络或更新失败会只能选退出游戏或重试
#else
		public static bool ForceUpdate = false; //默认不强更，无网络或更新失败可以选跳过更新或重试
#endif
		public static bool Networked = false;
		
		public static readonly string[] RenameList = {"iOS"};
	}
}