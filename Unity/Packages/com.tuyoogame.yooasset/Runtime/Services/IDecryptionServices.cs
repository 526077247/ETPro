﻿using System.IO;
using UnityEngine;

namespace YooAsset
{
    public struct DecryptFileInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件加载路径
        /// </summary>
        public string FileLoadPath;

        /// <summary>
        /// Unity引擎用于内容校验的CRC
        /// </summary>
        public uint FileLoadCRC;
    }
    public struct DecryptResult
    {
        /// <summary>
        /// 资源包对象
        /// </summary>
        public AssetBundle Result;

        /// <summary>
        /// 异步请求句柄
        /// </summary>
        public AssetBundleCreateRequest CreateRequest;

        /// <summary>
        /// 托管流对象
        /// 注意：流对象在资源包对象释放的时候会自动释放
        /// </summary>
        public Stream ManagedStream;
    }

    public interface IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// </summary>
        DecryptResult LoadAssetBundle(DecryptFileInfo fileInfo);

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// </summary>
        DecryptResult LoadAssetBundleAsync(DecryptFileInfo fileInfo);

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] ReadFileData(DecryptFileInfo fileInfo);

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string ReadFileText(DecryptFileInfo fileInfo);
    }
}
