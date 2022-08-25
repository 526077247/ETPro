using UnityEngine;
using UnityEditor;
using System.IO;
namespace ET
{
    [CustomEditor(typeof(Readme))]
    [InitializeOnLoad]
    public class ReadmeEditor : Editor
	{
		static string kShowedReadmeSessionStateName = "ReadmeEditor.showedReadme";
		static float kSpace = 16f;

		static readonly string WorkPath = "Assets/Scripts/Readme";

		static ReadmeEditor()
		{
			EditorApplication.delayCall += SelectReadmeAutomatically;
		}

		static void SelectReadmeAutomatically()
		{
			if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
			{
				var readme = SelectReadme();
				SessionState.SetBool(kShowedReadmeSessionStateName, true);

				if (readme && !readme.loadedLayout)
				{
					LoadLayout();
					readme.loadedLayout = true;
				}
			}
		}

		static void LoadLayout()
		{
			//var assembly = typeof(EditorApplication).Assembly;
			//var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
			//var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
			//method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "TutorialInfo/Layout.wlt"), false });
		}

		[MenuItem("Assets/Create/ScriptObject/Readme", false)]
		static void CreateReadme()
		{
			var asset = ScriptableObject.CreateInstance<Readme>();
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}

			asset.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(WorkPath, "Icons/icon.jpg"));
			asset.title = "使用说明";
			asset.sections = new Readme.Section[2];

			Readme.Section s1 = new Readme.Section();
			s1.heading = "ETPro";

			Readme.Section s2 = new Readme.Section();
			s2.heading = "QA";
			s2.text = ">框架对应的引擎版本是多少？\n2020.3.33f1\n\n" +
					">和ET的区别?\n1.使用YooAssets实现了资源热更新，包括多渠道安装包分包配置功能\n"+
					"2.使用了基于YooAssets的资源管理系统替换了原有系统，包括对动态图集、Unity内置SpriteAtlas图集功能的支持\n"+
					"3.使用组件模式的UI框架替换掉了原有部分，包括红点系统、多语言系统、引导系统等，易于集成第三方插件\n"+
					"4.使用HybridCLR替换了ILRuntime，对C#语法支持更完备，BUG更少\n"+
					"5.提供一个简单可扩展的对话框架\n"+
					"6.替换AOI框架，支持OBB、球形触发器和射线检测，并且双端使用AOI\n"+
					"7.提供一个简单可扩展的战斗框架，并且双端可单独使用\n"+
					"8.提供一个简单的Ghost系统，服务端无缝世界跨逻辑地图战斗\n\n"+
					">如何快速启动游戏？\n" +
					"1.菜单Assets->打开C#项目\n" +
					"2.打开根目录下的Client-Server.sln，然后右键全部生成\n" +
					"3.菜单Tools->ServerTools，依次点击StartFileServer启动文件服务器，StartServer(Wartcher)启动服务器\n" +
					"4.Shift+B切换到启动场景Init\n\n" +
					">...";

			asset.sections[0] = s1;
			asset.sections[1] = s2;

			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/Readme.asset");
			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}

		[MenuItem("Tools/帮助/简介")]
		static Readme SelectReadme()
		{
			var ids = AssetDatabase.FindAssets("Readme t:Readme");
			if (ids.Length == 1)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

				Selection.objects = new UnityEngine.Object[] { readmeObject };

				return (Readme)readmeObject;
			}
			else
			{
				Debug.Log("Couldn't find a readme");
				return null;
			}
		}

		[MenuItem("Tools/帮助/启动场景 #_b")]
		static void ChangeInitScene()
		{
			EditorApplication.OpenScene("Assets/AssetsPackage/Scenes/InitScene/Init.unity");
		}

		protected override void OnHeaderGUI()
		{
			var readme = (Readme)target;
			Init();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				GUILayout.Label(readme.title, TitleStyle);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			var readme = (Readme)target;
			Init();

			foreach (var section in readme.sections)
			{
				if (!string.IsNullOrEmpty(section.heading))
				{
					GUILayout.Label(section.heading, HeadingStyle);
				}
				if (!string.IsNullOrEmpty(section.text))
				{
					GUILayout.Label(section.text, BodyStyle);
				}
				if (!string.IsNullOrEmpty(section.linkText))
				{
					if (LinkLabel(new GUIContent(section.linkText)))
					{
						Application.OpenURL(section.url);
					}
				}
				GUILayout.Space(kSpace);
			}
		}


		bool m_Initialized;

		GUIStyle LinkStyle { get { return m_LinkStyle; } }
		[SerializeField] GUIStyle m_LinkStyle;

		GUIStyle TitleStyle { get { return m_TitleStyle; } }
		[SerializeField] GUIStyle m_TitleStyle;

		GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
		[SerializeField] GUIStyle m_HeadingStyle;

		GUIStyle BodyStyle { get { return m_BodyStyle; } }
		[SerializeField] GUIStyle m_BodyStyle;

		void Init()
		{
			if (m_Initialized)
				return;
			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;

			m_LinkStyle = new GUIStyle(m_BodyStyle);
			m_LinkStyle.wordWrap = false;
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_LinkStyle.stretchWidth = false;

			m_Initialized = true;
		}

		bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkStyle);
		}
	}
}