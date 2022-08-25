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
                self.m_cacheSingleSprite = new LruCache<string, SpriteValue>();
                self.m_cacheSpriteAtlas = new LruCache<string, SpriteAtlasValue>();
                self.m_cacheDynamicAtlas = new Dictionary<string, DynamicAtlas>();
                self.InitSingleSpriteCache(self.m_cacheSingleSprite);
                self.InitSpriteAtlasCache(self.m_cacheSpriteAtlas);
                PreLoad().Coroutine();
            }
        }
        [ObjectSystem]
        public class ImageLoaderComponentDestroySystem : DestroySystem<ImageLoaderComponent>
        {
            public override void Destroy(ImageLoaderComponent self)
            {
                self.Clear();
                self.m_cacheDynamicAtlas = null;
                self.m_cacheSingleSprite = null;
                self.m_cacheSpriteAtlas = null;
                ImageLoaderComponent.Instance = null;
            }
        }

        static async ETTask PreLoad()
        {
            for (int i = 0; i < 2; i++)//看情况提前预加载，加载会卡顿
            {
                await TimerComponent.Instance.WaitAsync(1);
                var temp = DynamicAtlasPage.OnCreate(i, DynamicAtlasGroup.Size_2048, DynamicAtlasGroup.Size_2048,null);
                temp.Dispose();
            }
        }
        static void InitSpriteAtlasCache(this ImageLoaderComponent self,LruCache<string, SpriteAtlasValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteAtlasValue value) => {
                return value.ref_count == 0;
            });

            cache.SetPopCallback((key, value) => {
                var subasset = value.subasset;
                foreach (var item in subasset)
                {
                    UnityEngine.Object.Destroy(item.Value.asset);
                    item.Value.asset = null;
                    item.Value.ref_count = 0;
                }
                ResourcesComponent.Instance.ReleaseAsset(value.asset);
                value.asset = null;
                value.ref_count = 0;
            });
        }
        
        static void InitSingleSpriteCache(this ImageLoaderComponent self, LruCache<string, SpriteValue> cache)
        {
            cache.SetCheckCanPopCallback((string key, SpriteValue value) => {
                return value.ref_count == 0;
            });
            cache.SetPopCallback((key, value) => {
                ResourcesComponent.Instance.ReleaseAsset(value.asset);
                value.asset = null;
                value.ref_count = 0;
            });
        }
        //异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
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
        //异步加载图片 会自动识别图集：回调方式（image 和button已经封装 外部使用时候 谨慎使用）
        public static async ETTask<Sprite> LoadImageAsync(this ImageLoaderComponent self,string image_path, Action<Sprite> callback = null)
        {
            Sprite res = null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, image_path.GetHashCode());
                self.__GetSpriteLoadInfoByPath(image_path, out int asset_type, out string asset_address,
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

        //释放图片
        public static void ReleaseImage(this ImageLoaderComponent self,string image_path)
        {
            if (string.IsNullOrEmpty(image_path))
                return;
            self.__GetSpriteLoadInfoByPath(image_path, out int asset_type, out string asset_address, out string subasset_name);

            if (asset_type == ImageLoaderComponent.SpriteType.SpriteAtlas)
            {
                if (self.m_cacheSpriteAtlas.TryOnlyGet(image_path, out SpriteAtlasValue value))
                {
                    if (value.ref_count > 0)
                    {
                        var subasset = value.subasset;
                        if (subasset.ContainsKey(subasset_name))
                        {
                            subasset[subasset_name].ref_count = subasset[subasset_name].ref_count - 1;
                            if (subasset[subasset_name].ref_count <= 0)
                            {
                                GameObject.Destroy(subasset[subasset_name].asset);
                                subasset.Remove(subasset_name);
                            }
                            value.ref_count = value.ref_count - 1;
                        }
                    }
                }
            }
            else if (asset_type == ImageLoaderComponent.SpriteType.DynSpriteAtlas)
            {
                var index = asset_address.IndexOf(self.DYN_ATLAS_KEY);
                var path = asset_address.Substring(0, index);
                if (self.m_cacheDynamicAtlas.TryGetValue(path, out var value))
                {
                    value.RemoveTexture(image_path, true);
                }
            }
            else
            {
                if (self.m_cacheSingleSprite.TryOnlyGet(image_path, out SpriteValue value))
                {
                    if (value.ref_count > 0)
                    {
                        value.ref_count = value.ref_count - 1;
                    }
                }
            }

        }


        //异步加载图集： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        public static async ETTask<Sprite> LoadAtlasImageAsync(this ImageLoaderComponent self,string atlas_path, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, atlas_path.GetHashCode());
                res = await self.__LoadAtlasImageAsyncInternal(atlas_path, null, callback);
                callback?.Invoke(res);
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        
        //异步加载图片： 回调方式，按理除了预加载的时候其余时候是不需要关心图集的
        public static async ETTask<Sprite> LoadSingleImageAsync(this ImageLoaderComponent self,string atlas_path, Action<Sprite> callback = null)
        {
            Sprite res;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, atlas_path.GetHashCode());
                res = await self.__LoadSingleImageAsyncInternal(atlas_path, callback);
                callback?.Invoke(res);

            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return res;
        }
        #region private

        static async ETTask<Sprite> __LoadAtlasImageAsyncInternal(this ImageLoaderComponent self,string asset_address, string subasset_name, Action<Sprite> callback = null)
        {
            var cacheCls = self.m_cacheSpriteAtlas;
            if (cacheCls.TryGet(asset_address, out var value_c))
            {
                if (value_c.asset == null)
                {
                    cacheCls.Remove(asset_address);
                }
                else
                {
                    value_c.ref_count = value_c.ref_count + 1;
                    if (value_c.subasset.TryGetValue(subasset_name, out var result))
                    {
                        value_c.subasset[subasset_name].ref_count = value_c.subasset[subasset_name].ref_count + 1;
                        callback?.Invoke(result.asset);
                        return result.asset;
                    }
                    else
                    {
                        var sp = value_c.asset.GetSprite(subasset_name);
                        if (sp == null)
                        {
                            Log.Error("image not found:" + subasset_name);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.subasset == null)
                            value_c.subasset = new Dictionary<string, SpriteValue>();
                        value_c.subasset[subasset_name] = new SpriteValue { asset = sp, ref_count = 1 };
                        callback?.Invoke(sp);
                        return sp;
                    }
                }
            }
            var asset = await ResourcesComponent.Instance.LoadAsync<SpriteAtlas>(asset_address);
            if (asset != null)
            {
                if (cacheCls.TryGet(asset_address, out var value))
                {
                    value.ref_count = value.ref_count + 1;
                }
                else
                {
                    value = new SpriteAtlasValue() { asset = asset , ref_count = 1 };
                    cacheCls.Set(asset_address, value);
                }
                if (value.subasset.TryGetValue(subasset_name, out var result))
                {
                    value.subasset[subasset_name].ref_count = value.subasset[subasset_name].ref_count + 1;
                    callback?.Invoke(result.asset);
                    return result.asset;
                }
                else
                {
                    var sp = value.asset.GetSprite(subasset_name);
                    if (sp == null)
                    {
                        Log.Error("image not found:" + subasset_name);
                        callback?.Invoke(null);
                        return null;
                    }
                    if (value.subasset == null)
                        value.subasset = new Dictionary<string, SpriteValue>();
                    value.subasset[subasset_name] = new SpriteValue { asset = sp, ref_count = 1 };
                    callback?.Invoke(sp);
                    return sp;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        static async ETTask<Sprite> __LoadSingleImageAsyncInternal(this ImageLoaderComponent self,string asset_address, Action<Sprite> callback = null)
        {
            var cacheCls = self.m_cacheSingleSprite;
            if (cacheCls.TryGet(asset_address, out var value_c))
            {
                if (value_c.asset == null)
                {
                    cacheCls.Remove(asset_address);
                }
                else
                {
                    value_c.ref_count = value_c.ref_count + 1;
                    callback?.Invoke(value_c.asset);
                    return value_c.asset;
                }
            }
            var asset = await ResourcesComponent.Instance.LoadAsync<Sprite>(asset_address);
            if (asset != null)
            {
                if (cacheCls.TryGet(asset_address, out var value))
                {
                    value.ref_count = value.ref_count + 1;
                }
                else
                {
                    value = new SpriteValue() { asset = asset, ref_count = 1 };
                    cacheCls.Set(asset_address, value);
                    callback?.Invoke(value.asset);
                    return value.asset;
                }
            }
            callback?.Invoke(null);
            return null;
        }
        static void __GetSpriteLoadInfoByPath(this ImageLoaderComponent self,string image_path, out int asset_type, out string asset_address, out string subasset_name)
        {
            asset_address = image_path;
            subasset_name = "";
            var index = image_path.IndexOf(self.ATLAS_KEY);
            if (index < 0)
            {
                //没有找到/atlas/，则是散图
                index = image_path.IndexOf(self.DYN_ATLAS_KEY);
                if (index < 0)
                {
                    //是散图
                    asset_type = ImageLoaderComponent.SpriteType.Sprite;
                    return;
                }
                else
                {
                    //是动态图集
                    asset_type = ImageLoaderComponent.SpriteType.DynSpriteAtlas;
                    return;
                }
            }
            asset_type = ImageLoaderComponent.SpriteType.SpriteAtlas;
            var substr = image_path.Substring(index + self.ATLAS_KEY.Length);
            var subIndex = substr.IndexOf('/');
            string atlasPath;
            string spriteName;
            if (subIndex >= 0)
            {
                //有子目录
                var prefix = image_path.Substring(0, index+1);
                var name = substr.Substring(0, subIndex);
                atlasPath = string.Format("{0}{1}.spriteatlas", prefix, "Atlas_" + name);
                var dotIndex = substr.LastIndexOf(".");
                var lastSlashIndex = substr.LastIndexOf('/');
                spriteName = substr.Substring(lastSlashIndex+1, dotIndex - lastSlashIndex-1);
            }
            else
            {
                var prefix = image_path.Substring(0, index + 1);

                atlasPath = prefix + "Atlas.spriteatlas";


                var dotIndex = substr.LastIndexOf(".");

                spriteName = substr.Substring(0, dotIndex);
            }
            asset_address = atlasPath;
            subasset_name = spriteName;
        }

        static async ETTask<Sprite> __LoadSpriteImageAsyncInternal(this ImageLoaderComponent self,
             string asset_address, string subasset_name, Action<Sprite> callback)
        {
            LruCache<string, SpriteAtlasValue> cacheCls = self.m_cacheSpriteAtlas;
            var cached = false;
            if (cacheCls.TryGet(asset_address, out SpriteAtlasValue value_c))
            {
                if (value_c.asset == null)
                {
                    cacheCls.Remove(asset_address);
                }
                else
                {
                    cached = true;
                    Sprite result;
                    var subasset_list = value_c.subasset;
                    if (subasset_list.ContainsKey(subasset_name))
                    {
                        result = subasset_list[subasset_name].asset;
                        subasset_list[subasset_name].ref_count = subasset_list[subasset_name].ref_count + 1;
                        value_c.ref_count++;
                    }
                    else
                    {
                        result = value_c.asset.GetSprite(subasset_name);
                        if (result == null)
                        {
                            Log.Error("image not found:" + asset_address + "__" + subasset_name);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.subasset == null)
                            value_c.subasset = new Dictionary<string, SpriteValue>();
                        value_c.subasset[subasset_name] = new SpriteValue { asset = result, ref_count = 1 };
                        value_c.ref_count++;
                    }
                    callback?.Invoke(result);
                    return result;
                }
            }
            if (!cached)
            {
                var asset = await ResourcesComponent.Instance.LoadAsync<SpriteAtlas>(asset_address);
                if (asset != null)
                {
                    Sprite result;
                    var sa = asset;
                    if (cacheCls.TryGet(asset_address, out value_c))
                    {
                        var subasset_list = value_c.subasset;
                        if (subasset_list.ContainsKey(subasset_name))
                        {
                            result = subasset_list[subasset_name].asset;
                            subasset_list[subasset_name].ref_count = subasset_list[subasset_name].ref_count + 1;
                        }
                        else
                        {
                            result = value_c.asset.GetSprite(subasset_name);
                            if (result == null)
                            {
                                Log.Error("image not found:" + asset_address + "__" + subasset_name);
                                callback?.Invoke(null);
                                return null;
                            }
                            if (value_c.subasset == null)
                                value_c.subasset = new Dictionary<string, SpriteValue>();
                            value_c.subasset[subasset_name] = new SpriteValue { asset = result, ref_count = 1 };
                        }
                    }
                    else
                    {
                        value_c = new SpriteAtlasValue { asset = sa, subasset = new Dictionary<string, SpriteValue>(), ref_count = 1 };
                        result = value_c.asset.GetSprite(subasset_name);
                        if (result == null)
                        {
                            Log.Error("image not found:" + asset_address + "__" + subasset_name);
                            callback?.Invoke(null);
                            return null;
                        }
                        if (value_c.subasset == null)
                            value_c.subasset = new Dictionary<string, SpriteValue>();
                        value_c.subasset[subasset_name] = new SpriteValue { asset = result, ref_count = 1 };
                        cacheCls.Set(asset_address, value_c);
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
             string asset_address, Action<Sprite> callback)
        {
            Dictionary<string, DynamicAtlas> cacheCls = self.m_cacheDynamicAtlas;
            var index = asset_address.IndexOf(self.DYN_ATLAS_KEY);
            var path = asset_address.Substring(0, index);
            if (cacheCls.TryGetValue(path, out DynamicAtlas value_c))
            {
                Sprite result;
                if (value_c.TryGetSprite(asset_address,out result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                else
                {
                    var texture = await ResourcesComponent.Instance.LoadAsync<Texture>(asset_address);
                    if (texture == null)
                    {
                        Log.Error("image not found:" + asset_address);
                        callback?.Invoke(null);
                        return null;
                    }
                    value_c.SetTexture(asset_address,texture);
                    ResourcesComponent.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                    if (value_c.TryGetSprite(asset_address,out  result))
                    {
                        callback?.Invoke(result);
                        return result;
                    }
                    Log.Error("image not found:" + asset_address );
                    callback?.Invoke(null);
                    return null;
                }
            }
            else
            {
                Log.Info(self.Id +" "+ cacheCls.Count);
                Log.Info("CreateNewDynamicAtlas  ||"+path+"||");
                value_c = new DynamicAtlas(DynamicAtlasGroup.Size_2048);
                cacheCls.Add(path,value_c);
                Log.Info(self.Id +" "+ cacheCls.Count);
                var texture = await ResourcesComponent.Instance.LoadAsync<Texture>(asset_address);
                if (texture == null)
                {
                    Log.Error("image not found:" + asset_address );
                    callback?.Invoke(null);
                    return null;
                }
                value_c.SetTexture(asset_address,texture);
                ResourcesComponent.Instance.ReleaseAsset(texture);//动态图集拷贝过了，直接释放
                if (value_c.TryGetSprite(asset_address,out var result))
                {
                    callback?.Invoke(result);
                    return result;
                }
                Log.Error("image not found:" + asset_address);
                callback?.Invoke(null);
                return null;
            }
        }
        #endregion

        public static void Clear(this ImageLoaderComponent self)
        {
            self.m_cacheSpriteAtlas.ForEach((key, value) =>
            {
                if (value.subasset != null)
                    foreach (var item in value.subasset)
                    {
                        GameObject.Destroy(item.Value.asset);
                    }
                ResourcesComponent.Instance?.ReleaseAsset(value.asset);
                value.asset = null;
                value.subasset = null;
                value.ref_count = 0;
            });
            self.m_cacheSpriteAtlas.Clear();

            self.m_cacheSingleSprite.ForEach((key, value) =>
                {
                    ResourcesComponent.Instance?.ReleaseAsset(value.asset);
                    value.asset = null;
                    value.ref_count = 0;
                }
            );
            self.m_cacheSingleSprite.Clear();

            foreach (var item in self.m_cacheDynamicAtlas)
            {
                item.Value.Dispose();
            }
            self.m_cacheDynamicAtlas.Clear();
            Log.Info("ImageLoaderComponent Clear");
        }
    }
}