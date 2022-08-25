using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * fd: 工具类，提供android系统直接读取asset目录下的方法接口
 */
public class AndroidAssetUtil
{
#if UNITY_ANDROID
    private const string ANDROID_ASSET_UTIL_JAVA_CLASS = "com.jing.unity.Unity2Android";

    private static string callRawApiReturnString(string apiName, params object[] args)
    {
        using (AndroidJavaObject cls = new AndroidJavaObject(ANDROID_ASSET_UTIL_JAVA_CLASS))
        {
            return cls.Call<string>(apiName, args);
        }
    }
    private static int callRawApiReturnInt(string apiName, params object[] args)
    {
        using (AndroidJavaObject cls = new AndroidJavaObject(ANDROID_ASSET_UTIL_JAVA_CLASS))
        {
            return cls.Call<int>(apiName, args);
        }
    }

    //读取asset文件
    public static string readAssetText(string fileName)
    {
        return callRawApiReturnString("readAssetText", fileName);
    }

#endif

}
