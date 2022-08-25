

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public class FileTools/* #begin profile */
#if EASY_PROFILE
: Neutrino
#else

#endif
/* #end profile */
{
    #region 检测指定目录是否存在
    /// <summary> 
    /// 检测指定目录是否存在 
    /// </summary> 
    /// <param name="directoryPath">目录的绝对路径</param>         
    public static bool IsExistDirectory(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }
    #endregion

    #region 检测指定文件是否存在
    /// <summary> 
    /// 检测指定文件是否存在,如果存在则返回true。 
    /// </summary> 
    /// <param name="filePath">文件的绝对路径</param>         
    public static bool IsExistFile(string filePath)
    {
        return File.Exists(filePath);
    }
    #endregion

    #region 创建一个目录
    /// <summary> 
    /// 创建一个目录 
    /// </summary> 
    /// <param name="directoryPath">目录的绝对路径</param> 
    public static void CreateDirectory(string directoryPath)
    {
#if UNITY_WEB
        return ;
#endif
        //如果目录不存在则创建该目录 
        if (!IsExistDirectory(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
    #endregion

    #region 创建一个文件
    /// <summary> 
    /// 创建一个文件。 
    /// </summary> 
    /// <param name="filePath">文件的绝对路径</param> 
    public static void CreateFile(string filePath)
    {
#if UNITY_WEB
        return ;
#else
        try
        {
            //如果文件不存在则创建该文件 
            if (!IsExistFile(filePath))
            {
                //创建一个FileInfo对象 
                FileInfo file = new FileInfo(filePath);
                //创建文件 
                FileStream fs = file.Create();
                //关闭文件流 
                fs.Close();
            }
        }
        catch (Exception ex)
        {

            throw ex;
        }
#endif
    }
    /// <summary> 
    /// 创建一个文件,并将字节流写入文件。 
    /// </summary> 
    /// <param name="filePath">文件的绝对路径</param> 
    /// <param name="buffer">二进制流数据</param> 
    public static void CreateFile(string filePath, byte[] buffer)
    {
#if UNITY_WEB
        return ;
#else
        try
        {
            //如果文件不存在则创建该文件 
            if (!IsExistFile(filePath))
            {
                //创建一个FileInfo对象 
                FileInfo file = new FileInfo(filePath);
                //创建文件 
                FileStream fs = file.Create();
                //写入二进制流 
                fs.Write(buffer, 0, buffer.Length);
                //关闭文件流 
                fs.Close();
            }
        }
        catch (Exception ex)
        {

            throw ex;
        }
#endif
    }

    /// <summary>
    /// 创建一个文件
    /// </summary>
    /// <param name="filePath">路径</param>
    /// <param name="s">内容</param>
    /// <param name="encode">编码</param>
    /// <returns></returns>
    public static bool CreateFile(string filePath, string s, string encode)
    {
#if UNITY_WEB
        return false;
#endif
        bool ret = true;
        //string path = ConfigurationManager.AppSettings["MakeContentPath"];
        Encoding code = Encoding.GetEncoding(encode);
        StreamWriter sw = null;
        try
        {
            sw = new StreamWriter(filePath, false, code);
            sw.Write(s);
            sw.Flush();
        }
        catch (Exception ex)
        {
            ret = false;
            throw ex;
        }
        finally
        {
            sw.Close();
        }
        return ret;
    }
    #endregion

    #region 获取指定目录中的文件列表
    /// <summary> 
    /// 获取指定目录中所有文件列表 
    /// </summary> 
    /// <param name="directoryPath">指定目录的绝对路径</param>         
    public static string[] GetFileNames(string directoryPath)
    {
#if UNITY_WEB
        return null;
#endif
        //如果目录不存在，则抛出异常 
        if (!IsExistDirectory(directoryPath))
        {
            throw new FileNotFoundException();
        }
        //获取文件列表 
        return Directory.GetFiles(directoryPath);
    }
    /// <summary> 
    /// 获取指定目录及子目录中所有文件列表 (  *.png|*.txt|*.xml )
    /// </summary> 
    /// <param name="directoryPath">指定目录的绝对路径</param> 
    /// <param name="searchPattern">模式字符串，"*"代表0或N个字符，"?"代表1个字符。 
    /// 范例："Log*.xml"表示搜索所有以Log开头的Xml文件。</param> 
    /// <param name="isSearchChild">是否搜索子目录</param> 
    public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
    {
#if UNITY_WEB
        return null;
#else
        int i;
        List<string> mList = new List<string>();
        string[] exts = searchPattern.Split('|');
        //如果目录不存在，则抛出异常 
        if (!IsExistDirectory(directoryPath))
        {
            //throw new FileNotFoundException();
            return mList.ToArray();
        }
        try
        {
            if (isSearchChild)
            {
                for (i = 0; i < exts.Length; i++)
                {
                    mList.AddRange(Directory.GetFiles(directoryPath, exts[i], SearchOption.AllDirectories));
                }
                //return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
            }
            else
            {
                for (i = 0; i < exts.Length; i++)
                {
                    mList.AddRange(Directory.GetFiles(directoryPath, exts[i], SearchOption.TopDirectoryOnly));
                }
                //return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly);
            }

            return mList.ToArray();
        }
        catch (IOException ex)
        {
            throw ex;
        }
#endif
    }
    #endregion


    #region 从文件的绝对路径中获取文件名( 包含扩展名 )
    /// <summary> 
    /// 从文件的绝对路径中获取文件名( 包含扩展名 ) 
    /// </summary> 
    /// <param name="filePath">文件的绝对路径</param>         
    public static string GetFileName(string filePath)
    {
#if UNITY_WEB
        return string.Empty;
#endif
        //获取文件的名称 
        FileInfo fi = new FileInfo(filePath);
        return fi.Name;
    }
    #endregion

    #region 从文件的绝对路径中获取文件名( 不包含扩展名 )
    /// <summary> 
    /// 从文件的绝对路径中获取文件名( 不包含扩展名 ) 
    /// </summary> 
    /// <param name="filePath">文件的绝对路径</param>         
    public static string GetFileNameNoExtension(string filePath)
    {
        filePath = filePath.Replace("\\", "/");
        string[] strs = filePath.Split('/');
        filePath = strs[strs.Length - 1];
        return filePath.Split('.')[0];

    }

    public static string GetFileExtension(string filePath)
    {
#if UNITY_WEB
        return string.Empty;
#endif
        //获取文件的名称 
        FileInfo fi = new FileInfo(filePath);
        string name = fi.Name;
        if (name.LastIndexOf('.') > -1)
        {
            name = name.Substring(name.LastIndexOf('.') + 1);
            return name.ToLower();
        }
        return string.Empty;
    }
    #endregion

    #region 从文件夹的绝对路径中获取文件夹名
    /// <summary> 
    /// 从文件夹的绝对路径中获取文件夹名
    /// </summary> 
    /// <param name="filePath">文件夹的绝对路径</param>         
    public static string GetDiretoryName(string filePath)
    {
#if UNITY_WEB
        return string.Empty;
#endif
        if (filePath.IndexOf(".") > -1)
        {
            var fileName = GetFileName(filePath);
            filePath = filePath.Replace(fileName, "");
        }
        //获取文件的名称 
        DirectoryInfo fi = new DirectoryInfo(filePath);
        return fi.Name;
    }


    /// <summary>
    /// 获得文件夹路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetDiretoryPath(string filePath)
    {
#if UNITY_WEB
        return string.Empty;
#endif
        if (Directory.Exists(filePath))
        {
            return filePath;
        }
        else
        {
            filePath = filePath.Replace("\\", "/");
            int len = filePath.LastIndexOf("/");
            if (len < 0)
            {
                return string.Empty;
            }
            return filePath.Substring(0, len);
        }
    }

    /// <summary>
    /// 获得文件夹目录
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetParentDiretoryPath(string filePath)
    {
#if UNITY_WEB
        return string.Empty;
#endif
        filePath = filePath.Replace("\\", "/");
        int len = filePath.LastIndexOf("/");
        if (len < 0)
        {
            return string.Empty;
        }
        return filePath.Substring(0, len);
    }

    #endregion
    #region 文件夹的相对路径的文件夹名
    /// <summary> 
    /// 文件夹的相对路径的文件夹名
    /// </summary> 
    /// <param name="filePath">文件夹的相对路径</param>         
    public static string GetRelativeDiretoryName(string relativePath)
    {
#if UNITY_WEB
        return string.Empty;
#endif
        relativePath = relativePath.Replace("\\", "/");
        string[] strs = relativePath.Split('/');
        return strs[strs.Length - 1];
    }
    #endregion

    public static void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }



}
