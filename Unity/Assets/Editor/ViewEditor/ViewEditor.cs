using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET
{
    public class ViewEditor: EditorWindow
    {
        Vector2 scrollPos;
        Vector2 scrollPos2;
        TreeViewState m_TreeViewState;
        EntityTreeView m_TreeView;
        
        private Dictionary<string, EntityView> AllEntity;
        private List<EntityView> ChildOfAllEntity;
        private List<EntityView> ComponentOfAllEntity;

        public EntityView ShowEntityView;
        void OnEnable ()
        {
            //检查是否已存在序列化视图状态（在程序集重新加载后
            // 仍然存在的状态）
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState ();
        }
        void Init()
        {
            if (AllEntity == null)
            {
                this.AllEntity = CodeLoader.Instance.GetAllEntitys();
                ChildOfAllEntity = new List<EntityView>();
                this.ComponentOfAllEntity = new List<EntityView>();
                foreach (var item in AllEntity)
                {
                    var entity = item.Value;
                    if(entity.ChildOfAll) ChildOfAllEntity.Add(entity);
                    if(entity.ComponentOfAll) this.ComponentOfAllEntity.Add(entity);
                    foreach (var childof in item.Value.ChildOf)
                    {
                        AllEntity[childof].Childs.Add(entity);
                    }
                    foreach (var childof in item.Value.ComponentOf)
                    {
                        AllEntity[childof].Components.Add(entity);
                    }
                }
            }
        }

        void Clear()
        {
            ShowEntityView = null;
            AllEntity = null;
        }
        
        
        [MenuItem("Tools/组件实体关系一览")]
        static void OpenViewEditor()
        {
            GetWindow<ViewEditor>().Show();
        }
        
        public void OnGUI()
        {
            if (!Application.isPlaying||CodeLoader.Instance==null||!CodeLoader.Instance.IsInit)
            {
                ShowNotification(new GUIContent("请在游戏启动后使用"));
                return;
            }
            
            
            
            if (GUILayout.Button("刷新", GUILayout.Width(100),GUILayout.Height(20)))
            {
                Clear();
                Init();
            }
            if (this.AllEntity != null)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(ShowEntityView != null?300:position.height));
                foreach (var item in this.AllEntity)
                {
                    DrawData(item.Value);
                }
                EditorGUILayout.EndScrollView();
            }
            if (ShowEntityView != null)
            {
                EditorGUILayout.Space();
                if (m_TreeView == null)
                {
                    m_TreeView = new EntityTreeView(m_TreeViewState,ShowEntityView);
                }
                else if(m_TreeView.Data!=ShowEntityView)
                {
                    m_TreeView.Data = ShowEntityView;
                    m_TreeView.Reload();
                }
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.Width(position.width), GUILayout.Height(position.height-320));
                m_TreeView.OnGUI(new Rect(0,0, position.width, position.height-320));
                EditorGUILayout.EndScrollView();
            }
        }
        
        private void DrawData(EntityView data)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(data.Name);
            if(GUILayout.Button("详情",GUILayout.Width(60)))
            {
                ShowEntityView = data;
            }
            EditorGUILayout.EndHorizontal();
        }
        
    }
}