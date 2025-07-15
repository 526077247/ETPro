using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 资源依赖数据库
    /// </summary>
    public class AssetDependencyDatabase
    {
        private const string FILE_VERSION = "1.0";
        private const string ATLAS_KEY = "/Atlas/";
        private class DependencyInfo
        {
            /// <summary>
            /// 此哈希函数会聚合了以下内容：源资源路径、源资源、元文件、目标平台以及导入器版本。
            /// 如果此哈希值发送变化，则说明导入资源可能已更改，因此应重新搜集依赖关系。
            /// </summary>
            public string DependHash;

            /// <summary>
            /// 直接依赖资源的GUID列表
            /// </summary>
            public List<string> DependGUIDs = new List<string>();
        }

        private string _databaseFilePath;
        private readonly Dictionary<string, DependencyInfo> _database = new Dictionary<string, DependencyInfo>(100000);

        /// <summary>
        /// 创建缓存数据库
        /// </summary>
        public void CreateDatabase(bool readCacheDatabaseFile, string databaseFilePath)
        {
            _databaseFilePath = databaseFilePath;
            _database.Clear();

            FileStream stream = null;
            BinaryReader reader = null;
            try
            {
                if (readCacheDatabaseFile && File.Exists(databaseFilePath))
                {
                    // 解析缓存文件
                    stream = File.OpenRead(databaseFilePath);
                    reader = new BinaryReader(stream);
                    string fileVersion = reader.ReadString();
                    if (fileVersion != FILE_VERSION)
                        throw new Exception("The database file version not match !");

                    var count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var assetPath = reader.ReadString();
                        var cacheInfo = new DependencyInfo
                        {
                            DependHash = reader.ReadString(),
                            DependGUIDs = ReadStringList(reader),
                        };
                        _database.Add(assetPath, cacheInfo);
                    }

                    // 移除无效资源
                    List<string> removeList = new List<string>(10000);
                    foreach (var cacheInfoPair in _database)
                    {
                        var assetPath = cacheInfoPair.Key;
                        var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
                        if (string.IsNullOrEmpty(assetGUID))
                        {
                            removeList.Add(assetPath);
                        }
                    }
                    foreach (var assetPath in removeList)
                    {
                        _database.Remove(assetPath);
                    }
                }
            }
            catch (Exception ex)
            {
                ClearDatabase(true);
                Debug.LogError($"Failed to load cache database : {ex.Message}");
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();
            }

            // 查找新增或变动资源
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssetPaths)
            {
                if (_database.TryGetValue(assetPath, out DependencyInfo cacheInfo))
                {
                    var dependHash = AssetDatabase.GetAssetDependencyHash(assetPath);
                    if (dependHash.ToString() != cacheInfo.DependHash)
                    {
                        _database[assetPath] = CreateDependencyInfo(assetPath);
                    }
                }
                else
                {
                    var newCacheInfo = CreateDependencyInfo(assetPath);
                    _database.Add(assetPath, newCacheInfo);
                }
            }
        }

        /// <summary>
        /// 保存缓存数据库
        /// </summary>
        public void SaveDatabase()
        {
            if (File.Exists(_databaseFilePath))
                File.Delete(_databaseFilePath);

            FileStream stream = null;
            BinaryWriter writer = null;
            try
            {
                stream = File.Create(_databaseFilePath);
                writer = new BinaryWriter(stream);
                writer.Write(FILE_VERSION);
                writer.Write(_database.Count);
                foreach (var assetPair in _database)
                {
                    string assetPath = assetPair.Key;
                    var assetInfo = assetPair.Value;
                    writer.Write(assetPath);
                    writer.Write(assetInfo.DependHash);
                    WriteStringList(writer, assetInfo.DependGUIDs);
                }
                writer.Flush();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save cache database : {ex.Message}");
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (stream != null)
                    stream.Close();
            }
        }

        /// <summary>
        /// 清理缓存数据库
        /// </summary>
        public void ClearDatabase(bool deleteDatabaseFile)
        {
            if (deleteDatabaseFile)
            {
                if (File.Exists(_databaseFilePath))
                    File.Delete(_databaseFilePath);
            }

            _database.Clear();
        }

        /// <summary>
        ///  获取资源的依赖列表
        /// </summary>
        public string[] GetDependencies(string assetPath, bool recursive)
        {
            // 注意：AssetDatabase.GetDependencies()方法返回结果里会踢出丢失文件！
            // 注意：AssetDatabase.GetDependencies()方法返回结果里会包含主资源路径！

            // 注意：机制上不允许存在未收录的资源
            if (_database.ContainsKey(assetPath) == false)
            {
                throw new Exception($"Fatal : can not found cache info : {assetPath}");
            }

            var result = new HashSet<string> { assetPath };
            if (!assetPath.EndsWith(".spriteatlas"))
            {
                CollectDependencies(assetPath, result, recursive);
            }
            // 注意：AssetDatabase.GetDependencies保持一致，将主资源添加到依赖列表最前面
            return result.ToArray();
        }
        private void CollectDependencies(string assetPath, HashSet<string> result, bool recursive)
        {
            if (_database.TryGetValue(assetPath, out var cacheInfo) == false)
            {
                throw new Exception($"Fatal : can not found cache info : {assetPath}");
            }

            foreach (var dependGUID in cacheInfo.DependGUIDs)
            {
                string dependAssetPath = AssetDatabase.GUIDToAssetPath(dependGUID);
                if (string.IsNullOrEmpty(dependAssetPath))
                    continue;

                // 如果是文件夹资源
                if (AssetDatabase.IsValidFolder(dependAssetPath))
                    continue;

                // 如果已经收集过
                if (result.Contains(dependAssetPath))
                    continue;
                //图集资源
                var index = dependAssetPath.IndexOf(ATLAS_KEY);
                if (index > 0)
                {
                    var prefix = dependAssetPath.Substring(0, index + 1);
                    var substr = dependAssetPath.Substring(index + ATLAS_KEY.Length);
                    var subIndex = substr.IndexOf('/');
                    string atlasPath;
                    if (subIndex >= 0)
                    {
                        //有子目录
                        var name = substr.Substring(0, subIndex);
                        atlasPath = string.Format("{0}{1}.spriteatlas", prefix, "Atlas_" + name);
                    }
                    else
                    {
                        atlasPath = prefix + "Atlas.spriteatlas";
                    }
                    if (!result.Contains(atlasPath))
                    {
                        result.Add(atlasPath);
                    }
                    continue;
                }
                result.Add(dependAssetPath);

                // 递归收集依赖
                if (recursive)
                    CollectDependencies(dependAssetPath, result, recursive);
            }
        }

        private List<string> ReadStringList(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var values = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                values.Add(reader.ReadString());
            }
            return values;
        }
        private void WriteStringList(BinaryWriter writer, List<string> values)
        {
            writer.Write(values.Count);
            foreach (var value in values)
            {
                writer.Write(value);
            }
        }
        private DependencyInfo CreateDependencyInfo(string assetPath)
        {
            var dependHash = AssetDatabase.GetAssetDependencyHash(assetPath);
            var dependAssetPaths = AssetDatabase.GetDependencies(assetPath, false);
            var dependGUIDs = new List<string>();
            foreach (var dependAssetPath in dependAssetPaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(dependAssetPath);
                if (string.IsNullOrEmpty(guid) == false)
                {
                    dependGUIDs.Add(guid);
                }
            }

            var cacheInfo = new DependencyInfo();
            cacheInfo.DependHash = dependHash.ToString();
            cacheInfo.DependGUIDs = dependGUIDs;
            return cacheInfo;
        }
    }
}