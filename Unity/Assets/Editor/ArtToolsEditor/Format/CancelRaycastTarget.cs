using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Object = System.Object;

public class CancelRaycastTarget
{
    private static System.Type type = null;
    public static System.Type GetMenuOptionsClass()
    {
        if (type == null)
        {
            type = System.Type.GetType("UnityEditor.UI.MenuOptions,UnityEditor.UI");
        }

        return type;
    }

    private static Dictionary<string, MethodInfo> dict = new Dictionary<string, MethodInfo>();
    public static GameObject Invoke(string funcName, MenuCommand menuCommand)
    {
        if (string.IsNullOrEmpty(funcName))
        {
            return null;
        }

        if (!dict.ContainsKey(funcName))
        {
            System.Type type = GetMenuOptionsClass();
            System.Reflection.MethodInfo func = type.GetMethod(funcName);

            if (func != null)
            {
                dict.Add(funcName, func);
            }
            else
            {
                Debug.LogError("找不到方法: " + funcName);
                return null;
            }
        }

        dict[funcName].Invoke(null, new System.Object[] { menuCommand });
        return Selection.activeGameObject;
    }

    //重写Create->UI->Text事件
    [MenuItem("GameObject/UI/Text", false, 2080)]
    static void CreatText(MenuCommand menuCommand)
    {
        //新建TextMeshProUGUI对象  
        GameObject go = new GameObject("textmesh", typeof(TextMeshProUGUI));
        //将raycastTarget置为false  
        go.GetComponent<TextMeshProUGUI>().raycastTarget = false;
        //设置其父物体  
        go.transform.SetParent(Selection.activeTransform);
    }

    [MenuItem("GameObject/UI/Button", false, 2081)]
    public static void CreateButton(MenuCommand menuCommand)
    {
        ChangeUIText("AddButton", menuCommand);
    }

    [MenuItem("GameObject/UI/Toggle", false, 2033)]
    public static void CreateToggle(MenuCommand menuCommand)
    {
        ChangeUIText("AddToggle", menuCommand);
    }

    [MenuItem("GameObject/UI/Dropdown", false, 2082)]
    public static void CreateDropdown(MenuCommand menuCommand)
    {
        ChangeUIText("AddDropdown", menuCommand);
    }

    [MenuItem("GameObject/UI/Input Field", false, 2083)]
    public static void CreateInputField(MenuCommand menuCommand)
    {
        ChangeUIText("AddInputField", menuCommand);
    }

    //这里是菜单添加时修改，另一个地方是inspector addcomponent修改，在 ObjectFactoryManager类
    public static void ChangeUIText(string funcName, MenuCommand menuCommand)
    {
        GameObject go = Invoke(funcName, menuCommand);
        if (go != null)
        {
            // var fontData = GlobalFontUtils.LoadGlobalFont();
            // Text[] texts = go.GetComponentsInChildren<Text>(true);
            // foreach (var txt in texts)
            // {
            //     txt.raycastTarget = false;
            //     if (fontData != null)
            //     {
            //         txt.font = fontData.font;
            //     }
            // }
        }
    }

    // 自动取消RatcastTarget
    [MenuItem("GameObject/UI/Image")]
    static void CreatImage()
    {
        GameObject go = new GameObject("image", typeof(Image));
        go.GetComponent<Image>().raycastTarget = false;
        go.transform.SetParent(Selection.activeTransform);
    }

    //重写Create->UI->Raw Image事件  
    [MenuItem("GameObject/UI/Raw Image")]
    static void CreatRawImage()
    {
        //新建Text对象  
        GameObject go = new GameObject("RawImage", typeof(RawImage));
        //将raycastTarget置为false  
        go.GetComponent<RawImage>().raycastTarget = false;
        //设置其父物体  
        go.transform.SetParent(Selection.activeTransform);
    }

    //重写Create->UI->TextMeshPro事件  
    [MenuItem("GameObject/UI/Text - TextMeshPro")]
    static void CreatTextMeshPro()
    {
        //新建TextMeshProUGUI对象  
        GameObject go = new GameObject("textmesh", typeof(TextMeshProUGUI));
        //将raycastTarget置为false  
        go.GetComponent<TextMeshProUGUI>().raycastTarget = false;
        //设置其父物体  
        go.transform.SetParent(Selection.activeTransform);
    }
}