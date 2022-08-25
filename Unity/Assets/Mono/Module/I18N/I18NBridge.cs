using System;
using System.Collections.Generic;
using ET;
public class I18NBridge
{

    public static I18NBridge Instance { get; private set; } = new I18NBridge();

    public Dictionary<string, string> i18nTextKeyDic;
    public event Action OnLanguageChangeEvt;
    public void OnLanguageChange()
    {
        OnLanguageChangeEvt?.Invoke();
    }
    /// <summary>
    /// 通过key获取多语言文本
    /// </summary>
    /// <param name="key">key</param>
    /// <returns></returns>
    public string GetText(string key)
    {
        if (i18nTextKeyDic.ContainsKey(key))
        {
            return i18nTextKeyDic[key];
        }
        Log.Error("多语言未配置："+key);
        return key;
    }

}

