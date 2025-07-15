using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ET
{
    
    internal class DataTreeViewItem : TreeViewItem
    {
        public EntityData Data;
        
    }
    public class EntityDataTreeView:TreeView
    {
        public EntityDataTreeView(TreeViewState treeViewState, EntityData data,Action<EntityData> callBack)
                : base(treeViewState)
        {
            Data = data;
            OnClick = callBack;
            getNewSelectionOverride = GetNewSelection;
            Reload();
        }

        private int _id = 0;
        public EntityData Data;
        public Action<EntityData> OnClick;
        protected override TreeViewItem BuildRoot()
        {
            _id = 0;
            var root = new TreeViewItem {id = _id++, depth = -1, displayName ="Root"};
            if (Data != null)
            {
                var item = new DataTreeViewItem { id = _id++, displayName = "[Scene]"+Data.Type.Name ,Data = Data};
                root.AddChild(item);
                CreateTree(item, this.Data);
                SetupDepthsFromParentsAndChildren(root);
            }
            return root;
        }

        void CreateTree(TreeViewItem root,EntityData parent)
        {
            for (int i = 0; i < parent.Childs.Count;i++)
            {
                var data = parent.Childs[i];
                var item = new DataTreeViewItem { id = _id++, displayName ="[Child]"+  data.Type.Name,Data = data };
                root.AddChild(item);
                CreateTree(item, data);
            }
            for (int i = 0; i < parent.Components.Count;i++)
            {
                var data = parent.Components[i];
                var item = new DataTreeViewItem { id = _id++, displayName ="[Component]"+ data.Type.Name,Data = data };
                root.AddChild(item);
                CreateTree(item, data);
            }
            
        }

        public List<int> GetNewSelection(TreeViewItem item, bool keepMultiSelection, bool useActionKeyAsShift)
        {
            DataTreeViewItem data = (DataTreeViewItem) item;
            this.state.selectedIDs.Clear();
            this.state.selectedIDs.Add(item.id);
            OnClick?.Invoke(data.Data);
            return this.state.selectedIDs;
        }
    }
}