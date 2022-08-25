
using UnityEditor;
using UnityEngine;



public class ImageMenu : Editor
{

    [MenuItem("Tools/ArtTools/搜索或批量替换Sprite", false, 503)]
    public static void ReplaceImage()
    {
        Rect _rect = new Rect(0, 0, 900, 600);
        ReplaceImage window = EditorWindow.GetWindowWithRect<ReplaceImage>(_rect, true, "搜索或批量替换Sprite");
        window.Show();
    }

    [MenuItem("Tools/ArtTools/查找未使用的图片", false, 503)]
    public static void CheckUnUseImage()
    {
        Rect _rect = new Rect(0, 0, 900, 600);
        CheckUnuseImage window = EditorWindow.GetWindowWithRect<CheckUnuseImage>(_rect, true, "查找未使用的图片");
        window.Show();
    }

    [MenuItem("Tools/ArtTools/检查丢失image", false, 504)]
    public static void CheckLossImage()
    {
        Rect _rect = new Rect(0, 0, 900, 600);
        CheckEmptyImage window = EditorWindow.GetWindowWithRect<CheckEmptyImage>(_rect, true, "检查预设丢失image");
        window.Show();
    }
}
