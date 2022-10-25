using UnityEditor.IMGUI.Controls;

namespace ET
{
    public class EntityTreeView:TreeView
    {
        public EntityTreeView(TreeViewState treeViewState,EntityView data)
                : base(treeViewState)
        {
            Data = data;
            Reload();
        }

        private int _id = 0;
        public EntityView Data;
        protected override TreeViewItem BuildRoot()
        {
            _id = 0;
            var root = new TreeViewItem {id = _id++, depth = -1, displayName ="Root"};
            if (Data != null)
            {
                var item = new TreeViewItem { id = _id++, displayName = Data.Name };
                root.AddChild(item);
                CreateTree(item, this.Data);
                SetupDepthsFromParentsAndChildren(root);
            }
            return root;
        }

        void CreateTree(TreeViewItem root,EntityView parent)
        {
            for (int i = 0; i < parent.Childs.Count;i++)
            {
                var data = parent.Childs[i];
                var item = new TreeViewItem { id = _id++, displayName ="[Child]"+  data.Name };
                root.AddChild(item);
                CreateTree(item, data);
            }
            for (int i = 0; i < parent.Components.Count;i++)
            {
                var data = parent.Components[i];
                var item = new TreeViewItem { id = _id++, displayName ="[Component]"+ data.Name };
                root.AddChild(item);
                CreateTree(item, data);
            }
        }
    }
}