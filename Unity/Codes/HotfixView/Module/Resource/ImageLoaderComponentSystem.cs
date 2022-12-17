using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
namespace ET
{
    [FriendClass(typeof(ImageLoaderComponent))]
    public static class ImageLoaderComponentSystem
    {
        [ObjectSystem]
        public class ImageLoaderComponentAwakeSystem : AwakeSystem<ImageLoaderComponent>
        {
            public override void Awake(ImageLoaderComponent self)
            {
                ImageLoaderComponent.Instance = self;
                self.cacheSingleSprite = new LruCache<string, SpriteValue>();
                self.cacheSpriteAtlas = new LruCache<string, SpriteAtlasValue>();
                self.cacheDynamicAtlas = new Dictionary<string, DynamicAtlas>();
                self.__InitSingleSpriteCache(self.cacheSingleSprite);
                self.__InitSpriteAtlasCache(self.cacheSpriteAtlas);
                __PreLoad().Coroutine();
            }
        }
        [ObjectSystem]
        public class ImageLoaderComponentDestroySystem : DestroySystem<ImageLoaderComponent>
        {
            public override void Destroy(ImageLoaderComponent self)
            {
                self.Clear();
                self.cacheDynamicAtlas = null;
                self.cacheSingleSprite = null;
                self.cacheSpriteAtlas = null;
                ImageLoaderComponent.Instance = null;
            }
        }

        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="image_path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static ETTask LoadImageTask(this ImageLoaderComponent self, string image_path, Action<Sprite> callback = null)
        {
            ETTask task = ETTask.Create();
            self.LoadImageAsync(image_path, (data) =>
            {
                callback?.Invoke(data);
                task.SetResult();
            }).Coroutine();
            return task;
        }
        /// <summary>
        /// 异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        /// </summary>
        /// <param name="self"></param>
        /// <param name="imagePath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async ETTask<Sprite> LoadImageAsync(this ImageLoaderComponent self,string imagePath, Action<Sprite> callback = null)
        {
            Sprite res = null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, imagePath.GetHashCode());
                self.__GetSpriteLoadInfoByPath(imagePath, out int asset_type, out string asset_address,
                    out string subasset_name);
                if (asset_type == ImageLoaderComponent.SpriteType.Sprite)
                {
                    res = await self.__LoadSingleImageAsyncInternal( asset_address,callback);
                }
                else if (asset_type == ImageLoaderComponent.SpriteType.DynSpriteAtlas)
                {
                    res = await self.__LoadDynSpriteImageAsyncInternal(asset_address, callback);
                }
                else
                {
                    res = await self.__LoadSpriteImageAsyncInternal(asset_address, subasset_name, callback);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }

        /// <summary>
        /// 释放图片
        /// </summary>
        /// <param name="self"></param>
        /// <param name="imagePath"></param>
        public static void ReleaseImage(this ImageLoaderComponent self,string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;
            self.__GetSpriteLoadInfoByPath(imagePath, out int assetType, out string assetAddress, out string subassetName);

            if (assetType == ImageLoaderComponent.SpriteType.SpriteAtlas)
            {
                if (self.cacheSpriteAtlas.TryOnlyGet(imagePath, out SpriteAtlasValue value))
                {
                    if (value.RefCount > 0)
                    {
                        var subasset = value.SubAsset;
                        if (subasset.ContainsKey(subassetName))
                        {
                            subasset[subassetName].RefCount = subasset[subassetName].RefCount - 1;
                            if (subasset[subassetName].RefCount <= 0)
                            {
                                GameObject.Destroy(subasset[subassetName].Asset);
                                subasset.Remove(subassetName);
                            }
                            value.RefCount = value.RefCount - 1;
                        }
                    }
                }
            }
            else if (assetType == ImageLoaderComponent.SpriteType.DynSpriteAtlas)
            {
                var index = assetAddress.IndexOf(self.DYN_ATLAS_KEY);
                var path = assetAddress.Substring(0, index);
                if (self.cacheDynamicAtlas.TryGetValue(path, out var value))
                {
                    value.RemoveTexture(imagePath, true);
                }
            }
            else
            {
                if (self.cacheSingleSprite.TryOnlyGet(imagePath, out SpriteValue value))
                {
                    if (value.RefCount > 0)
                    {
                        value.RefCount = value.RefCount - 1;
                    }
                }
            }

        }


        /// <summary>
        /// 异步加载图集： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        /// </summary>
        /// <param name="self"></param>
        /// <param name="atlasPath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async ETTask<Sprite> LoadAtlasImageAsync(this ImageLoaderComponent self,string atlasPath, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, atlasPath.GetHashCode());
                res = await self.__LoadAtlasImageAsyncInternal(atlasPath, null, callback);
                callback?.Invoke(res);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        
        /// <summary>
        /// 异步加载图片： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        /// </summary>
        /// <param name="self"></param>
        /// <param name="atlasPath"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async ETTask<Sprite> LoadSingleImageAsync(this ImageLoaderComponent self,string atlasPath, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, atlasPath.GetHashCode());
                res = await self.__LoadSingleImageAsyncInternal(atlasPath, callback);
                callback?.Invoke(res);

            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }


        public static void Clear(this ImageLoaderComponent self)
        {
            foreach (var kv in self.cacheSpriteAtlas)
            {
                var value = kv.Value;
                if (value.SubAsset != null)
                    foreach (var item in value.SubAsset)
                    {
                        GameObject.Destroy(item.Value.Asset);
                    }
                ResourcesComponent.Instance?.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.SubAsset = null;
                value.RefCount = 0;
            }
            self.cacheSpriteAtlas.Clear();

            foreach (var kv in self.cacheSingleSprite)
            {
                var value = kv.Value;
                ResourcesComponent.Instance?.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.RefCount = 0;
            }
            self.cacheSingleSprite.Clear();

            foreach (var item in self.cacheDynamicAtlas)
            {
                item.Value.Dispose();
            }
            self.cacheDynamicAtlas.Clear();
            Log.Info("ImageLoaderComponent Clear");
        }
        
        #region 私有方法

        static async ETTask __PreLoad()
        {
            for (int i = 0; i < 2; i++)//看情况提前预加载，加载会卡顿
            {
                await Game.WaitFrameFinish();
                var temp = DynamicAtlasPage.OnCreate(i, DynamicAtlasDefine.Size_2048, DynamicAtlasDefine.Size_2048,null);
                temp.Dispose();
            }
        }
        static void __InitSpriteAtlasCache(this ImageLoaderComponent self,LruCache<string, SpriteAtlasValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteAtlasValue value) => {
                return value.RefCount == 0;
            });

            cache.SetPopCallback((key, value) => {
                var subasset = value.SubAsset;
                foreach (var item in subasset)
                {
                    UnityEngine.Object.Destroy(item.Value.Asset);
                    item.Value.Asset = null;
                    item.Value.RefCount = 0;
                }
                ResourcesComponent.Instance.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.RefCount = 0;
            });
        }
        
        static void __InitSingleSpriteCache(this ImageLoaderComponent self, LruCache<string, SpriteValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteValue value) => {
                return value.RefCount == 0;
            });
            cache.SetPopCallback((key, value) => {
                ResourcesComponent.Instance.ReleaseAsset(value.Asset);
                value.Asset = null;
                value.RefCount = 0;
            });
        }
        static async ETTask<Sprite> __LoadAtlasImageAsyncInternal(this ImageLoaderComponent self,string assetAddress, string subassetName, Action<Sprite> callback = null)
        {
            var cacheCls = self.cacheSpriteAtlas;
            if (cacheCls.TryGet(assetAddress, out var value_c))
            {
                if (value_c.Asset == null)
                {
                    cacheCls.Remove(assetAddress);
                }
                else
                {
                    value_c.RefCount = value_c.RefCount + 1;
                    if (value_c.SubAsset.TryGetValue(subassetName, out var result))
                    {
                        value_c.SubAsset[subassetName].RefCount = value_c.SubAsset[subassetName].RefCount + 1;
                        callback?.Invoke(result.Asset);
                        return result.Asset;
                    }
                    else
                    {
                        var sp = value_c.Asset.GetSprite(subassetName);
                        if (sp == null)
                        {
                            Log.Error("image not found:" + subassetName);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.SubAsset == null)
                            value_c.SubAsset = new Dictionary<string, SpriteValue>();
                        value_c.SubAsset[subassetName] = new SpriteValue { Asset = sp, RefCount = 1 };
                        callback?.Invoke(sp);
                        return sp;
                    }
                }
            }
            var asset = await ResourcesComponent.Instance.LoadAsync<SpriteAtlas>(assetAddress);
            if (asset != null)
            {
                if (cacheCls.TryGet(assetAddress, out var value))
                {
                    value.RefCount = value.RefCount + 1;
                }
                else
                {
                    value = new SpriteAtlasValue() { Asset = asset , RefCount = 1 };
                    cacheCls.Set(assetAddress, value);
                }
                if (value.SubAsset.TryGetValue(subassetName, out var result))
                {
                    value.SubAsset[subassetName].RefCount = value.SubAsset[subassetName].RefCount + 1;
                    callback?.Invoke(result.Asset);
                    return result.Asset;
                }
                else
                {
                    var sp = value.Asset.GetSprite(subassetName);
                    if (sp == null)
                    {
                        Log.Error("image not found:" + subassetName);
                        callback?.Invoke(null);
                        return null;
                    }
                    if (value.SubAsset == null)
                        value.SubAsset = new Dictionary<string, SpriteValue>();
                    value.SubAsset[subassetName] = new SpriteValue { Asset = sp, RefCount = 1 };
                    callback?.Invoke(sp);
                    return sp;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        static async ETTask<Sprite> __LoadSingleImageAsyncInternal(this ImageLoaderComponent self,string assetAddress, Action<Sprite> callback = null)
        {
            var cacheCls = self.cacheSingleSprite;
            if (cacheCls.TryGet(assetAddress, out var value_c))
            {
                if (value_c.Asset == null)
                {
                    cacheCls.Remove(assetAddress);
                }
                else
                {
                    value_c.RefCount = value_c.RefCount + 1;
                    callback?.Invoke(value_c.Asset);
                    return value_c.Asset;
                }
            }
            var asset = await ResourcesComponent.Instance.LoadAsync<Sprite>(assetAddress);
            if (asset != null)
            {
                if (cacheCls.TryGet(assetAddress, out var value))
                {
                    value.RefCount = value.RefCount + 1;
                }
                else
                {
                    value = new SpriteValue() { Asset = asset, RefCount = 1 };
                    cacheCls.Set(assetAddress, value);
                    callback?.Invoke(value.Asset);
                    return value.Asset;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        static void __GetSpriteLoadInfoByPath(this ImageLoaderComponent self,string imagePath, out int assetType, out string assetAddress, out string subasset_name)
        {
            assetAddress = imagePath;
            subasset_name = "";
            var index = imagePath.IndexOf(self.ATLAS_KEY);
            if (index < 0)
            {
                //没有找到/atlas/，则是散图
                index = imagePath.IndexOf(self.DYN_ATLAS_KEY);
                if (index < 0)
                {
                    //是散图
                    assetType = ImageLoaderComponent.SpriteType.Sprite;
                    return;
                }
                else
                {
                    //是动态图集
                    assetType = ImageLoaderComponent.SpriteType.DynSpriteAtlas;
                    return;
                }
            }
            assetType = ImageLoaderComponent.SpriteType.SpriteAtlas;
            var substr = imagePath.Substring(index + self.ATLAS_KEY.Length);
            var subIndex = substr.IndexOf('/');
            string atlasPath;
            string spriteName;
            if (subIndex >= 0)
            {
                //有子目录
                var prefix = imagePath.Substring(0, index+1);
                var name = substr.Substring(0, subIndex);
                atlasPath = string.Format("{0}{1}.spriteatlas", prefix, "Atlas_" + name);
                var dotIndex = substr.LastIndexOf(".");
                var lastSlashIndex = substr.LastIndexOf('/');
                spriteName = substr.Substring(lastSlashIndex+1, dotIndex - lastSlashIndex-1);
            }
            else
            {
                var prefix = imagePath.Substring(0, index + 1);

                atlasPath = prefix + "Atlas.spriteatlas";


                var dotIndex = substr.LastIndexOf(".");

                spriteName = substr.Substring(0, dotIndex);
            }
            assetAddress = atlasPath;
            subasset_name = spriteName;
        }

        static async ETTask<Sprite> __LoadSpriteImageAsyncInternal(this ImageLoaderComponent self,
             string assetAddress, string subassetName, Action<Sprite> callback)
        {
            LruCache<string, SpriteAtlasValue> cacheCls = self.cacheSpriteAtlas;
            var cached = false;
            if (cacheCls.TryGet(assetAddress, out SpriteAtlasValue value_c))
            {
                if (value_c.Asset == null)
                {
                    cacheCls.Remove(assetAddress);
                }
                else
                {
                    cached = true;
                    Sprite result;
                    var subasset_list = value_c.SubAsset;
                    if (subasset_list.ContainsKey(subassetName))
                    {
                        result = subasset_list[subassetName].Asset;
                        subasset_list[subassetName].RefCount = subasset_list[subassetName].RefCount + 1;
                        value_c.RefCount++;
                    }
                    else
                    {
                        result = value_c.Asset.GetSprite(subassetName);
                        if (result == null)
                        {
                            Log.Error("image not found:" + assetAddress + "__" + subassetName);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.SubAsset == null)
                            value_c.SubAsset = new Dictionary<string, SpriteValue>();
                        value_c.SubAsset[subassetName] = new SpriteValue { Asset = result, RefCount = 1 };
                        value_c.RefCount++;
                    }
                    callback?.Invoke(result);
                    return result;
                }
            }
            if (!cached)
            {
                var asset = await ResourcesComponent.Instance.LoadAsync<SpriteAtlas>(assetAddress);
                if (asset != null)
                {
                    Sprite result;
                    var sa = asset;
                    if (cacheCls.TryGet(assetAddress, out value_c))
                    {
                        var subasset_list = value_c.SubAsset;
                        if (subasset_list.ContainsKey(subassetName))
                        {
                            result = subasset_list[subassetName].Asset;
                            subasset_list[subassetName].RefCount = subasset_list[subassetName].RefCount + 1;
                        }
                        else
                        {
                            result = value_c.Asset.GetSprite(subassetName);
                            if (result == null)
                            {
                                Log.Error("image not found:" + assetAddress + "__" + subassetName);
                                callback?.Invoke(null);
                                return null;
                            }
                            if (value_c.SubAsset == null)
                                value_c.SubAsset = new Dictionary<string, SpriteValue>();
                            value_c.SubAsset[subassetName] = new SpriteValue { Asset = result, RefCount = 1 };
                        }
                    }
                    else
                    {
                        value_c = new SpriteAtlasValue { Asset = sa, SubAsset = new Dictionary<string, SpriteValue>(), RefCount = 1 };
                        result = value_c.Asset.GetSprite(subassetName);
                        if (result == null)
                        {
                            Log.Error("image not found:" + assetAddress + "__" + subassetName);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.SubAsset == null)
                            value_c.SubAsset = new Dictionary<string, SpriteValue>();
                        value_c.SubAsset[subassetName] = new SpriteValue { Asset = result, RefCount = 1 };
                        cacheCls.Set(assetAddress, value_c);
                    }
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    callback?.Invoke(null);
                    return null;
                }

            }
            callback?.Invoke(null);
            return null;

        }

        static async ETTask<Sprite> __LoadDynSpriteImageAsyncInternal(this ImageLoaderComponent self,
             string assetAddress, Action<Sprite> callback)
        {
            Dictionary<string, DynamicAtlas> cacheCls = self.cacheDynamicAtlas;
            var index = assetAddress.IndexOf(self.DYN_ATLAS_KEY);
            var path = assetAddress.Substring(0, index);
            if (cacheCls.TryGetValue(path, out DynamicAtlas value_c))
            {
                Sprite result;
                if (value_c.TryGetSprite(assetAddress,out result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    var texture = await ResourcesComponent.Instance.LoadAsync<Texture>(assetAddress);
                    if (texture == null)
                    {
                        Log.Error("image not found:" + assetAddress);
                        callback?.Invoke(null);
                        return null;
                    }
                    value_c.SetTexture(assetAddress,texture);
                    ResourcesComponent.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                    if (value_c.TryGetSprite(assetAddress,out  result))
                    {
                        callback?.Invoke(result);
                        return result;
                    }
                    Log.Error("image not found:" + assetAddress );
                    callback?.Invoke(null);
                    return null;
                }
            }
            else
            {
                Log.Info(self.Id +" "+ cacheCls.Count);
                Log.Info("CreateNewDynamicAtlas  ||"+path+"||");
                value_c = new DynamicAtlas(DynamicAtlasDefine.Size_2048);
                cacheCls.Add(path,value_c);
                Log.Info(self.Id +" "+ cacheCls.Count);
                var texture = await ResourcesComponent.Instance.LoadAsync<Texture>(assetAddress);
                if (texture == null)
                {
                    Log.Error("image not found:" + assetAddress );
                    callback?.Invoke(null);
                    return null;
                }
                value_c.SetTexture(assetAddress,texture);
                ResourcesComponent.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                if (value_c.TryGetSprite(assetAddress,out var result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                Log.Error("image not found:" + assetAddress);
                callback?.Invoke(null);
                return null;
            }
        }
        #endregion
    }
}