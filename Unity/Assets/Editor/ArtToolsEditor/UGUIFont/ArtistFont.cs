using System.IO;
using UnityEditor;
using UnityEngine;

public class ArtistFont : MonoBehaviour
{

    public static void BatchCreateArtistFont()
    {
        string fntFilePath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(fntFilePath) || !fntFilePath.Contains(".fnt"))
        {
            Debug.LogError("请选择字体文件，后缀为.fnt的文件");
            return;
        }

        string fntName = Path.GetFileNameWithoutExtension(fntFilePath);
        string dirName = Path.GetDirectoryName(fntFilePath);

        Font CustomFont = new Font();
        AssetDatabase.CreateAsset(CustomFont, Path.Combine(dirName, fntName + ".fontsettings"));
        CustomFont = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(dirName, fntName + ".fontsettings"));

        TextAsset BMFontText = AssetDatabase.LoadAssetAtPath(fntFilePath, typeof(TextAsset)) as TextAsset;

        BMFont mbFont = new BMFont();
        BMFontReader.Load(mbFont, BMFontText.name, BMFontText.bytes);  // 借用NGUI封装的读取类
        CharacterInfo[] characterInfo = new CharacterInfo[mbFont.glyphs.Count];
        for (int i = 0; i < mbFont.glyphs.Count; i++)
        {
            BMGlyph bmInfo = mbFont.glyphs[i];
            CharacterInfo info = new CharacterInfo();
            info.index = bmInfo.index;
            info.uv.x = (float)bmInfo.x / (float)mbFont.texWidth;
            info.uv.y = 1 - (float)bmInfo.y / (float)mbFont.texHeight;
            info.uv.width = (float)bmInfo.width / (float)mbFont.texWidth;
            info.uv.height = -1f * (float)bmInfo.height / (float)mbFont.texHeight;
            info.vert.x = (float)bmInfo.offsetX;
            info.vert.y = (float)bmInfo.offsetY - (float)bmInfo.height / 2;
            info.vert.width = (float)bmInfo.width;
            info.vert.height = (float)bmInfo.height;
            info.width = (float)bmInfo.advance;
            characterInfo[i] = info;
        }
        CustomFont.characterInfo = characterInfo;

        string textureFilename = Path.Combine(dirName, mbFont.spriteName + ".png");
        Shader shader = Shader.Find("GUI/Custom Text Shader");
        Material mat = new Material(shader);
        Texture tex = AssetDatabase.LoadAssetAtPath(textureFilename, typeof(Texture)) as Texture;
        mat.SetTexture("_MainTex", tex);
        AssetDatabase.CreateAsset(mat, Path.Combine(dirName, fntName + ".mat"));
        CustomFont.material = mat;

        EditorUtility.SetDirty(CustomFont);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
