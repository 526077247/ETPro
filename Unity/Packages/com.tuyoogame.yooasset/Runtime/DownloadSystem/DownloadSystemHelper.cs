﻿using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 自定义下载器的请求委托
    /// </summary>
    public delegate UnityWebRequest UnityWebRequestDelegate(string url);

    internal class DownloadSystemHelper
    {
        public static UnityWebRequestDelegate UnityWebRequestCreater = null;
        public static UnityWebRequest NewUnityWebRequestGet(string requestURL)
        {
            UnityWebRequest webRequest;
            if (UnityWebRequestCreater != null)
                webRequest = UnityWebRequestCreater.Invoke(requestURL);
            else
                webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbGET);
            return webRequest;
        }

        /// <summary>
        /// 获取WWW加载本地资源的路径
        /// </summary>
        public static string ConvertToWWWPath(string path)
        {
            string url;

            // 获取对应平台的URL地址
#if UNITY_EDITOR
            url = StringUtility.Format("file:///{0}", path);
#elif UNITY_WEBGL
            url = path;
#elif UNITY_IPHONE
            url = StringUtility.Format("file://{0}", path);
#elif UNITY_ANDROID
            if (path.StartsWith("jar:file://"))
                url = path;
            else
                url = StringUtility.Format("jar:file://{0}", path);
#elif UNITY_OPENHARMONY
            if (UnityEngine.Application.streamingAssetsPath.StartsWith("jar:file://"))
            {
                if (path.StartsWith("jar:file://"))
                    url = path;
                else
                    url = StringUtility.Format("jar:file://{0}", path);
            }
            else
            {
                if (path.StartsWith("file://"))
                    url = path;
                else
                    url = StringUtility.Format("file://{0}", path);
            }
#elif UNITY_STANDALONE_OSX
            url = new System.Uri(path).ToString();
#elif UNITY_STANDALONE
            url = StringUtility.Format("file:///{0}", path);
#else
            throw new System.NotImplementedException();
#endif

            // For some special cases when users have special characters in their devices, url paths can not be identified correctly.
            return url.Replace("+", "%2B").Replace("#", "%23").Replace("?", "%3F");
        }

        /// <summary>
        /// 是否请求的本地文件
        /// </summary>
        public static bool IsRequestLocalFile(string url)
        {
            //TODO UNITY_STANDALONE_OSX平台目前无法确定
            if (url.StartsWith("file:"))
                return true;
            if (url.StartsWith("jar:file:"))
                return true;

            return false;
        }
    }
}