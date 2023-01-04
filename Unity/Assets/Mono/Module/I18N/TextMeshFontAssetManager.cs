﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ET
{
    public class TextMeshFontAssetManager
    {
        public static TextMeshFontAssetManager Instance => new TextMeshFontAssetManager();
        private Dictionary<string, TMP_FontAsset> AddFontWithPathList = new Dictionary<string, TMP_FontAsset>();

#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern string __NT_GetSystemFonts();
#endif

        private void AddFontAsset(ScriptableObject fontAsset)
        {
            if (CheckFontAsset(fontAsset)) return;
            var def = TMP_Settings.defaultFontAsset;
            def.fallbackFontAssetTable.Add(fontAsset as TMP_FontAsset);
        }

        private void RemoveFontAsset(ScriptableObject fontAsset)
        {
            if (!CheckFontAsset(fontAsset)) return;
            var def = TMP_Settings.defaultFontAsset;
            def.fallbackFontAssetTable.Remove(fontAsset as TMP_FontAsset);
        }

        private bool CheckFontAsset(ScriptableObject fontAsset)
        {
            TMP_FontAsset font = fontAsset as TMP_FontAsset;
            var def = TMP_Settings.defaultFontAsset;
            return def.fallbackFontAssetTable.Contains(font);
        }

        public void AddWithOSFont(string[] tb)
        {
            Dictionary<string, string> fontPaths = new Dictionary<string, string>();
            IEnumerable tempPaths = null;
#if UNITY_IPHONE && !UNITY_EDITOR
            string jsonData = __NT_GetSystemFonts();
            if (!string.IsNullOrEmpty(jsonData))
            {
                //Debug.Log(jsonData);
                tempPaths = JsonHelper.FromJson<List<string>>(jsonData);
            }
#else
            tempPaths = Font.GetPathsToOSFonts();
#endif

            if (tempPaths == null)
                return;

            foreach (string path in tempPaths)
            {
                string key = Path.GetFileNameWithoutExtension(path);
                //Debug.Log(key);
                if (!fontPaths.ContainsKey(key))
                    fontPaths.Add(key, path);
            }

            for (int i = 0; i < tb.Length; i++)
            {
                string fontname = tb[i];
                if (fontPaths.ContainsKey(fontname))
                {
                    AddFontAssetByFontPath(fontPaths[fontname]);
                }
            }
        }

        //可以从网上下载字体或获取到本地自带字体
        private void AddFontAssetByFontPath(string fontPath)
        {
            if (AddFontWithPathList.ContainsKey(fontPath))
                return;

            Font font = new Font(fontPath);
            TMP_FontAsset tp_font = TMP_FontAsset.CreateFontAsset(font, 20, 2, GlyphRenderMode.SDFAA, 512, 512);
            AddFontAsset(tp_font);
            AddFontWithPathList.Add(fontPath, tp_font);
        }

        public void RemoveFontAssetByFontPath(string fontPath)
        {
            if (!AddFontWithPathList.ContainsKey(fontPath))
                return;

            TMP_FontAsset tp_font = AddFontWithPathList[fontPath];
            RemoveFontAsset(tp_font);
        }

        public int GetSystemLangeuage()
        {
            return (int) Application.systemLanguage;
        }
    }
}