using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace ET
{
    public class ViewDataEditor: EditorWindow
    {
        Vector2 scrollPos;
        Vector2 scrollPos2;
        TreeViewState m_TreeViewState = new TreeViewState ();
        EntityDataTreeView m_TreeView;

        [MenuItem("Tools/Entity/实时数据快照")]
        static void OpenViewEditor()
        {
            GetWindow<ViewDataEditor>().Show();
        }

        private EntityData Data;
        private EntityData ShowData;
        public void OnGUI()
        {
            if (!Application.isPlaying||CodeLoader.Instance==null||!CodeLoader.Instance.IsInit)
            {
                ShowNotification(new GUIContent("请在游戏启动后使用"));
                return;
            }
            
            
            
            if (GUILayout.Button("抓取数据快照", GUILayout.Width(100),GUILayout.Height(20)))
            {
                this.ShowData = null;
                Data = null;
                Data = CodeLoader.Instance.GetEntityData();
            }
            
            if (Data != null)
            {
                if (m_TreeView == null)
                {
                    m_TreeView = new EntityDataTreeView(m_TreeViewState,Data,ShowDataDetails);
                }
                else if(m_TreeView.Data!=Data)
                {
                    m_TreeView.Data = Data;
                    m_TreeView.Reload();
                }
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(ShowData != null?position.height-325:position.height));
                m_TreeView.OnGUI(new Rect(0,0, position.width, ShowData != null?position.height-325:position.height));
                EditorGUILayout.EndScrollView();
            }
            if (this.ShowData != null)
            {
                scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.Width(position.width), GUILayout.Height(300));
                GUILayout.Label("-");
                GUILayout.Label("Name:   " + this.ShowData.Type.Name);
                GUILayout.Label("Id:   " + this.ShowData.Id);
                GUILayout.Label("ChildCount:   " + this.ShowData.Childs.Count);
                GUILayout.Label("ComponentCount:   " + this.ShowData.Components.Count);
                for (int i = 0; i < this.ShowData.DataProperties.Count; i++)
                {
                    DrawProp(this.ShowData.DataProperties[i]);
                }
                EditorGUILayout.EndScrollView();
            }
            
        }

        public void ShowDataDetails(EntityData data)
        {
            this.ShowData = data;
        }

        public void DrawProp(EntityDataProperty property)
        {
            EditorGUI.indentLevel = property.indent;
            
            if (property.value is List<EntityDataProperty> list)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUI.indentLevel*20));
                GUILayout.Label($"[{property.type.Name}] {property.name}");
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < list.Count; i++)
                {
                    DrawProp(list[i]);
                }
                return;
            }

            if (property.value is Component mono)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUI.indentLevel*20));
                GUILayout.Label($"[{property.type.Name}] {property.name}");
                EditorGUILayout.ObjectField(mono, mono.GetType(), false);
                EditorGUILayout.EndHorizontal();
                return;
            }
            if (property.value is IList ilist)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUI.indentLevel*20));
                GUILayout.Label($"[{property.type.Name}] {property.name}");
                EditorGUILayout.EndHorizontal();
                int count = 0;
                foreach (var item in ilist)
                {
                    if (item != null)
                    {
                        if (count > 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width((EditorGUI.indentLevel + 1) * 20));
                            GUILayout.Label($"null * {count}");
                            EditorGUILayout.EndHorizontal();
                            count = 0;
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width((EditorGUI.indentLevel + 1) * 20));
                        GUILayout.Label($"[{item.GetType().Name}] {item}");
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        count++;
                    }
                }
                if (count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width((EditorGUI.indentLevel + 1) * 20));
                    GUILayout.Label($"null * {count}");
                    EditorGUILayout.EndHorizontal();
                    count = 0;
                }
                return;
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(EditorGUI.indentLevel*20));
            GUILayout.Label($"[{property.type.Name}] {property.name}:   {GetValue(property)}");
            EditorGUILayout.EndHorizontal();
        }

        public string GetValue(EntityDataProperty property)
        {
            try
            {
                if (property.type.IsArray)
                {
                    return JsonHelper.ToJson(property.value);
                }

                if (property.type.GenericTypeArguments.Length > 0)
                {
                    return JsonHelper.ToJson(property.value);
                }

                if (property.type.IsClass)
                {
                    return JsonHelper.ToJson(property.value);
                }
            }
            catch (Exception ex)
            {
                
            }
            return property.value?.ToString();
        }
    }
}