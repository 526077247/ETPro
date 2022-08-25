﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ET
{
	public class PlatformTypeComparer : IEqualityComparer<PlatformType>
	{
		public static PlatformTypeComparer Instance = new PlatformTypeComparer();
		public bool Equals(PlatformType x, PlatformType y)
		{
			return x == y;          //x.Equals(y);  注意这里不要使用Equals方法，因为也会造成封箱操作
		}

		public int GetHashCode(PlatformType x)
		{
			return (int)x;
		}
	}
	public enum PlatformType:byte
	{
		None,
		Android,
		IOS,
		PC,
		MacOS,
	}
	
	public enum BuildType:byte
	{
		Development,
		Release,
	}

	public class BuildEditor : EditorWindow
	{
		private const string settingAsset = "Assets/Editor/BuildEditor/ETBuildSettings.asset";

		private PlatformType activePlatform;
		private PlatformType platformType;
		private bool clearFolder;
		private bool isBuildExe;
		// private bool isInject;
		private bool isContainAB;
		private BuildType buildType;
		private BuildOptions buildOptions;
		private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
		private ETBuildSettings buildSettings;

		private BuildConfig config;
		[MenuItem("Tools/打包工具")]
		public static void ShowWindow()
		{
			GetWindow(typeof (BuildEditor));
		}

        private void OnEnable()
        {
#if UNITY_ANDROID
			activePlatform = PlatformType.Android;
#elif UNITY_IOS
			activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
			activePlatform = PlatformType.PC;
#elif UNITY_STANDALONE_OSX
			activePlatform = PlatformType.MacOS;
#else
			activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;

			if (!File.Exists(settingAsset))
            {
				buildSettings = new ETBuildSettings();
				AssetDatabase.CreateAsset(buildSettings, settingAsset);
            }
			else
			{
				buildSettings = AssetDatabase.LoadAssetAtPath<ETBuildSettings>(settingAsset);

				clearFolder = buildSettings.clearFolder;
				isBuildExe = buildSettings.isBuildExe;
				// isInject = buildSettings.isInject;
				isContainAB = buildSettings.isContainAB;
				buildType = buildSettings.buildType;
				buildAssetBundleOptions = buildSettings.buildAssetBundleOptions;
			}
        }

        private void OnDisable()
        {
			SaveSettings();
        }

        private void OnGUI() 
		{
			if (this.config == null)
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				config = JsonHelper.FromJson<BuildConfig>(jstr);
			}
			EditorGUILayout.LabelField("cdn地址：" + this.config.RemoteCdnUrl);
			EditorGUILayout.LabelField("渠道标识：" + this.config.Channel);
			EditorGUILayout.LabelField("资源版本：" + this.config.Resver);
			if (GUILayout.Button("修改配置"))
			{
				System.Diagnostics.Process.Start("notepad.exe", "Assets/AssetsPackage/config.bytes");
			}
			if (GUILayout.Button("刷新配置"))
			{
				string jstr = File.ReadAllText("Assets/AssetsPackage/config.bytes");
				config = JsonHelper.FromJson<BuildConfig>(jstr);
			}
			EditorGUILayout.LabelField("");
			EditorGUILayout.LabelField("打包平台:");
			this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
            this.clearFolder = EditorGUILayout.Toggle("清理资源文件夹: ", clearFolder);
            this.isBuildExe = EditorGUILayout.Toggle("是否打包EXE(整包): ", this.isBuildExe);
            this.buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType: ", this.buildType);
			//EditorGUILayout.LabelField("BuildAssetBundleOptions(可多选):");
			//this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);

			switch (buildType)
			{
				case BuildType.Development:
					this.buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging;
					break;
				case BuildType.Release:
					this.buildOptions = BuildOptions.None;
					break;
			}

			GUILayout.Space(5);

			if (GUILayout.Button("开始打包"))
			{
				if (this.platformType == PlatformType.None)
				{
					ShowNotification(new GUIContent("请选择打包平台!"));
					return;
				}
				if (platformType != activePlatform)
                {
                    switch (EditorUtility.DisplayDialogComplex("警告!", $"当前目标平台为{activePlatform}, 如果切换到{platformType}, 可能需要较长加载时间", "切换", "取消", "不切换"))
                    {
						case 0:
							activePlatform = platformType;
							break;
						case 1:
							return;
                        case 2:
							platformType = activePlatform;
							break;
                    }
                }

				if(!HybridCLR.HybridCLR.Setup())return;
				BuildHelper.Build(this.platformType, this.buildOptions, this.isBuildExe,this.clearFolder);
			}

			GUILayout.Space(5);
		}

		private void SaveSettings()
		{
			buildSettings.clearFolder = clearFolder;
			buildSettings.isBuildExe = isBuildExe;
			buildSettings.isContainAB = isContainAB;
			buildSettings.buildType = buildType;
			buildSettings.buildAssetBundleOptions = buildAssetBundleOptions;

			EditorUtility.SetDirty(buildSettings);
			AssetDatabase.SaveAssets();
		}
	}
}
