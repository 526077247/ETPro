﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset
{
    internal static class ManifestTools
    {
#if UNITY_EDITOR
        /// <summary>
        /// 序列化（JSON文件）
        /// </summary>
        public static void SerializeToJson(string savePath, PackageManifest manifest)
        {
            string json = JsonUtility.ToJson(manifest, true);
            FileUtility.WriteAllText(savePath, json);
        }

        /// <summary>
        /// 序列化（二进制文件）
        /// </summary>
        public static void SerializeToBinary(string savePath, PackageManifest manifest)
        {
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                // 创建缓存器
                BufferWriter buffer = new BufferWriter(YooAssetSettings.ManifestFileMaxSize);

                // 写入文件标记
                buffer.WriteUInt32(YooAssetSettings.ManifestFileSign);

                // 写入文件版本
                buffer.WriteUTF8(manifest.FileVersion);

                // 写入文件头信息
                buffer.WriteBool(manifest.EnableAddressable);
                buffer.WriteBool(manifest.LocationToLower);
                buffer.WriteBool(manifest.IncludeAssetGUID);
                buffer.WriteInt32(manifest.OutputNameStyle);
                buffer.WriteInt32(manifest.BuildBundleType);
                buffer.WriteUTF8(manifest.BuildPipeline);
                buffer.WriteUTF8(manifest.PackageName);
                buffer.WriteUTF8(manifest.PackageVersion);
                buffer.WriteUTF8(manifest.PackageNote);

                // 写入资源列表
                buffer.WriteInt32(manifest.AssetList.Count);
                for (int i = 0; i < manifest.AssetList.Count; i++)
                {
                    var packageAsset = manifest.AssetList[i];
                    buffer.WriteUTF8(packageAsset.Address);
                    buffer.WriteUTF8(packageAsset.AssetPath);
                    buffer.WriteUTF8(packageAsset.AssetGUID);
                    buffer.WriteUTF8Array(packageAsset.AssetTags);
                    buffer.WriteInt32(packageAsset.BundleID);
                }

                // 写入资源包列表
                buffer.WriteInt32(manifest.BundleList.Count);
                for (int i = 0; i < manifest.BundleList.Count; i++)
                {
                    var packageBundle = manifest.BundleList[i];
                    buffer.WriteUTF8(packageBundle.BundleName);
                    buffer.WriteUInt32(packageBundle.UnityCRC);
                    buffer.WriteUTF8(packageBundle.FileHash);
                    buffer.WriteUTF8(packageBundle.FileCRC);
                    buffer.WriteInt64(packageBundle.FileSize);
                    buffer.WriteBool(packageBundle.Encrypted);
                    buffer.WriteUTF8Array(packageBundle.Tags);
                    buffer.WriteInt32Array(packageBundle.DependIDs);
                }

                // 写入文件流
                buffer.WriteToStream(fs);
                fs.Flush();
            }
        }

        /// <summary>
        /// 反序列化（JSON文件）
        /// </summary>
        public static PackageManifest DeserializeFromJson(string jsonContent)
        {
            return JsonUtility.FromJson<PackageManifest>(jsonContent);
        }

        /// <summary>
        /// 反序列化（二进制文件）
        /// </summary>
        public static PackageManifest DeserializeFromBinary(byte[] binaryData)
        {
            // 创建缓存器
            BufferReader buffer = new BufferReader(binaryData);

            // 读取文件标记
            uint fileSign = buffer.ReadUInt32();
            if (fileSign != YooAssetSettings.ManifestFileSign)
                throw new Exception("Invalid manifest file !");

            // 读取文件版本
            string fileVersion = buffer.ReadUTF8();
            if (fileVersion != YooAssetSettings.ManifestFileVersion)
                throw new Exception($"The manifest file version are not compatible : {fileVersion} != {YooAssetSettings.ManifestFileVersion}");

            PackageManifest manifest = new PackageManifest();
            {
                // 读取文件头信息
                manifest.FileVersion = fileVersion;
                manifest.EnableAddressable = buffer.ReadBool();
                manifest.LocationToLower = buffer.ReadBool();
                manifest.IncludeAssetGUID = buffer.ReadBool();
                manifest.OutputNameStyle = buffer.ReadInt32();
                manifest.BuildBundleType = buffer.ReadInt32();
                manifest.BuildPipeline = buffer.ReadUTF8();
                manifest.PackageName = buffer.ReadUTF8();
                manifest.PackageVersion = buffer.ReadUTF8();
                manifest.PackageNote = buffer.ReadUTF8();

                // 检测配置
                if (manifest.EnableAddressable && manifest.LocationToLower)
                    throw new Exception("Addressable not support location to lower !");

                // 读取资源列表
                int packageAssetCount = buffer.ReadInt32();
                CreateAssetCollection(manifest, packageAssetCount);
                for (int i = 0; i < packageAssetCount; i++)
                {
                    var packageAsset = new PackageAsset();
                    packageAsset.Address = buffer.ReadUTF8();
                    packageAsset.AssetPath = buffer.ReadUTF8();
                    packageAsset.AssetGUID = buffer.ReadUTF8();
                    packageAsset.AssetTags = buffer.ReadUTF8Array();
                    packageAsset.BundleID = buffer.ReadInt32();
                    FillAssetCollection(manifest, packageAsset);
                }

                // 读取资源包列表
                int packageBundleCount = buffer.ReadInt32();
                CreateBundleCollection(manifest, packageBundleCount);
                for (int i = 0; i < packageBundleCount; i++)
                {
                    var packageBundle = new PackageBundle();
                    packageBundle.BundleName = buffer.ReadUTF8();
                    packageBundle.UnityCRC = buffer.ReadUInt32();
                    packageBundle.FileHash = buffer.ReadUTF8();
                    packageBundle.FileCRC = buffer.ReadUTF8();
                    packageBundle.FileSize = buffer.ReadInt64();
                    packageBundle.Encrypted = buffer.ReadBool();
                    packageBundle.Tags = buffer.ReadUTF8Array();
                    packageBundle.DependIDs = buffer.ReadInt32Array();
                    FillBundleCollection(manifest, packageBundle);
                }
            }

            // 初始化资源清单
            InitManifest(manifest);
            return manifest;
        }
#endif

        #region 解析资源清单辅助方法
        public static void InitManifest(PackageManifest manifest)
        {
            // 填充资源包内包含的主资源列表
            foreach (var packageAsset in manifest.AssetList)
            {
                int bundleID = packageAsset.BundleID;
                if (bundleID >= 0 && bundleID < manifest.BundleList.Count)
                {
                    var packageBundle = manifest.BundleList[bundleID];
                    packageBundle.IncludeMainAssets.Add(packageAsset);
                }
                else
                {
                    throw new Exception($"Invalid bundle id : {bundleID} Asset path : {packageAsset.AssetPath}");
                }
            }
        }

        public static void CreateAssetCollection(PackageManifest manifest, int assetCount)
        {
            manifest.AssetList = new List<PackageAsset>(assetCount);
            manifest.AssetDic = new Dictionary<string, PackageAsset>(assetCount);

            if (manifest.EnableAddressable)
                manifest.AssetPathMapping1 = new Dictionary<string, string>(assetCount * 3);
            else
                manifest.AssetPathMapping1 = new Dictionary<string, string>(assetCount * 2);

            if (manifest.IncludeAssetGUID)
                manifest.AssetPathMapping2 = new Dictionary<string, string>(assetCount);
            else
                manifest.AssetPathMapping2 = new Dictionary<string, string>();
        }
        public static void FillAssetCollection(PackageManifest manifest, PackageAsset packageAsset)
        {
            // 添加到列表集合
            manifest.AssetList.Add(packageAsset);

            // 注意：我们不允许原始路径存在重名
            string assetPath = packageAsset.AssetPath;
            if (manifest.AssetDic.ContainsKey(assetPath))
                throw new System.Exception($"AssetPath have existed : {assetPath}");
            else
                manifest.AssetDic.Add(assetPath, packageAsset);

            // 填充AssetPathMapping1
            {
                string location = packageAsset.AssetPath;
                if (manifest.LocationToLower)
                    location = location.ToLower();

                // 添加原生路径的映射
                if (manifest.AssetPathMapping1.ContainsKey(location))
                    throw new System.Exception($"Location have existed : {location}");
                else
                    manifest.AssetPathMapping1.Add(location, packageAsset.AssetPath);

                // 添加无后缀名路径的映射
                string locationWithoutExtension = Path.ChangeExtension(location, null);
                if (ReferenceEquals(location, locationWithoutExtension) == false)
                {
                    if (manifest.AssetPathMapping1.ContainsKey(locationWithoutExtension))
                        YooLogger.Warning($"Location have existed : {locationWithoutExtension}");
                    else
                        manifest.AssetPathMapping1.Add(locationWithoutExtension, packageAsset.AssetPath);
                }
            }

            // 添加可寻址地址
            if (manifest.EnableAddressable)
            {
                string location = packageAsset.Address;
                if (string.IsNullOrEmpty(location) == false)
                {
                    if (manifest.AssetPathMapping1.ContainsKey(location))
                        throw new System.Exception($"Location have existed : {location}");
                    else
                        manifest.AssetPathMapping1.Add(location, packageAsset.AssetPath);
                }
            }

            // 填充AssetPathMapping2
            if (manifest.IncludeAssetGUID)
            {
                if (manifest.AssetPathMapping2.ContainsKey(packageAsset.AssetGUID))
                    throw new System.Exception($"AssetGUID have existed : {packageAsset.AssetGUID}");
                else
                    manifest.AssetPathMapping2.Add(packageAsset.AssetGUID, packageAsset.AssetPath);
            }
        }

        public static void CreateBundleCollection(PackageManifest manifest, int bundleCount)
        {
            manifest.BundleList = new List<PackageBundle>(bundleCount);
            manifest.BundleDic1 = new Dictionary<string, PackageBundle>(bundleCount);
            manifest.BundleDic2 = new Dictionary<string, PackageBundle>(bundleCount);
            manifest.BundleDic3 = new Dictionary<string, PackageBundle>(bundleCount);
        }
        public static void FillBundleCollection(PackageManifest manifest, PackageBundle packageBundle)
        {
            // 初始化资源包
            packageBundle.InitBundle(manifest);

            // 添加到列表集合
            manifest.BundleList.Add(packageBundle);

            manifest.BundleDic1.Add(packageBundle.BundleName, packageBundle);
            manifest.BundleDic2.Add(packageBundle.FileName, packageBundle);
            manifest.BundleDic3.Add(packageBundle.BundleGUID, packageBundle);
        }
        #endregion

        /// <summary>
        /// 注意：该类拷贝自编辑器
        /// </summary>
        private enum EFileNameStyle
        {
            /// <summary>
            /// 哈希值名称
            /// </summary>
            HashName = 0,

            /// <summary>
            /// 资源包名称（不推荐）
            /// </summary>
            BundleName = 1,

            /// <summary>
            /// 资源包名称 + 哈希值名称
            /// </summary>
            BundleName_HashName = 2,
        }

        /// <summary>
        /// 获取资源文件的后缀名
        /// </summary>
        public static string GetRemoteBundleFileExtension(string bundleName)
        {
            string fileExtension = Path.GetExtension(bundleName);
            return fileExtension;
        }

        /// <summary>
        /// 获取远端的资源文件名
        /// </summary>
        public static string GetRemoteBundleFileName(int nameStyle, string bundleName, string fileExtension, string fileHash)
        {
            if (nameStyle == (int)EFileNameStyle.HashName)
            {
                return StringUtility.Format("{0}{1}", fileHash, fileExtension);
            }
            else if (nameStyle == (int)EFileNameStyle.BundleName)
            {
                return bundleName;
            }
            else if (nameStyle == (int)EFileNameStyle.BundleName_HashName)
            {
                if (string.IsNullOrEmpty(fileExtension))
                {
                    return StringUtility.Format("{0}_{1}", bundleName, fileHash);
                }
                else
                {
                    string fileName = bundleName.Remove(bundleName.LastIndexOf('.'));
                    return StringUtility.Format("{0}_{1}{2}", fileName, fileHash, fileExtension);
                }
            }
            else
            {
                throw new NotImplementedException($"Invalid name style : {nameStyle}");
            }
        }
    }
}