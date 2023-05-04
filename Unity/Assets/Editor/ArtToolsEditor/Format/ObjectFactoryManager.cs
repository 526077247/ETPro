using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public class ObjectFactoryManager
{
    static ObjectFactoryManager()
    {
        ObjectFactory.componentWasAdded += ComponentWasAdded;
    }

    private static void ComponentWasAdded(Component comp)
    {
        if (comp.GetType() == typeof(Text))
        {
            // var fontData = GlobalFontUtils.LoadGlobalFont();
            // Text txt = (Text)comp;
            // txt.raycastTarget = false;
            // if (fontData != null)
            // {
            //     txt.font = fontData.font;
            // }

            var obj = comp.gameObject;
            GameObject.DestroyImmediate(comp);
            obj.AddComponent<TextMeshProUGUI>();
        }
        else if (comp.GetType() == typeof(TextMeshProUGUI))
        {
            TextMeshProUGUI tmp = (TextMeshProUGUI) comp;
            tmp.raycastTarget = false;
        }
    }
}