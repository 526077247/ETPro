using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveAsPrefab : Editor
{
    [MenuItem("Tools/Save SelectObjs As Prefab")]
    public static void OnClickSaveAsPrefab()
    {
        string save_path = "Assets/Temp";
        if (!Directory.Exists(save_path))
            Directory.CreateDirectory(save_path);

        Undo.IncrementCurrentGroup();
        int group_index = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Save SelectObjs As Prefab");
        foreach(var item in Selection.gameObjects)
        {
            string new_file_path = save_path + item.name + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(item, new_file_path);
            GameObject new_obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Undo.RegisterCreatedObjectUndo(new_obj, "create group object");
            Undo.SetTransformParent(new_obj.transform, item.transform.parent, "move item to group");

            new_obj.transform.localPosition = item.transform.localPosition;
            new_obj.transform.localRotation = item.transform.localRotation;
            new_obj.transform.localScale = item.transform.lossyScale;

            Undo.DestroyObjectImmediate(item);
        }
        Undo.CollapseUndoOperations(group_index);
    }
    
}
