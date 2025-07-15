using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
namespace ET
{
    [FriendClass(typeof(ImageOnlineComponent))]
    public static class ImageOnlineComponentSystem
    {
        [ObjectSystem]
        public class ImageOnlineComponentAwakeSystem : AwakeSystem<ImageOnlineComponent>
        {
            public override void Awake(ImageOnlineComponent self)
            {
                ImageOnlineComponent.Instance = self;
                self.CacheOnlineSprite = new Dictionary<string, ImageOnlineInfo>();
            }
        }
        
        [ObjectSystem]
        public class ImageOnlineComponentDestroySystem : DestroySystem<ImageOnlineComponent>
        {
            public override void Destroy(ImageOnlineComponent self)
            {
                self.Clear();
                self.CacheOnlineSprite = null;
                ImageOnlineComponent.Instance = null;
            }
        }
        public static void Clear(this ImageOnlineComponent self)
        {
            foreach (var item in self.CacheOnlineSprite)
            {
                GameObject.Destroy(item.Value.Texture2D);
            }
            self.CacheOnlineSprite.Clear();
            Log.Info("ImageOnlineComponent Clear");
        }
        /// <summary>
        /// 加载图片失败递归重试
        /// </summary>
        /// <param name="url"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        public static async ETTask<Sprite> GetOnlineSprite(this ImageOnlineComponent self, string url,int tryCount = 3)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());
                if (self.CacheOnlineSprite.TryGetValue(url, out var data))
                {
                    data.RefCount++;
                    if (data.Sprite == null)
                    {
                        data.Sprite = Sprite.Create(data.Texture2D, new Rect(0, 0, data.Texture2D.width, data.Texture2D.height),
                            new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                    }
                    return data.Sprite;
                }

                var texture = await HttpManager.Instance.HttpGetImageOnline(url, true);
                if (texture != null) //本地已经存在
                {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                    self.CacheOnlineSprite.Add(url, new ImageOnlineInfo(texture, sprite, 1));
                    return sprite;
                }
                else
                {
                    for (int i = 0; i < tryCount; i++)
                    {
                        texture = await HttpManager.Instance.HttpGetImageOnline(url, false);
                        if (texture != null) break;
                    }

                    if (texture != null)
                    {
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f), 100f, 0U, SpriteMeshType.FullRect);
                        var bytes = texture.EncodeToPNG();
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            File.WriteAllBytes(HttpManager.Instance.LocalFile(url), bytes);
                        });
                        self.CacheOnlineSprite.Add(url, new ImageOnlineInfo(texture, sprite, 1));
                        return sprite;
                    }
                    else
                    {
                        Log.Error("网络无资源 " + url);
                    }
                }
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return null;
        }
        public static async ETTask<Texture2D> GetOnlineTexture(this ImageOnlineComponent self,string url,int tryCount = 3)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                        await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());
                if (self.CacheOnlineSprite.TryGetValue(url, out var data))
                {
                    data.RefCount++;
                    return data.Texture2D;
                }

                var texture = await HttpManager.Instance.HttpGetImageOnline(url, true);
                if (texture != null) //本地已经存在
                {
                    self.CacheOnlineSprite.Add(url, new ImageOnlineInfo(texture, null, 1));
                    return texture;
                }
                else
                {
                    for (int i = 0; i < tryCount; i++)
                    {
                        texture = await HttpManager.Instance.HttpGetImageOnline(url, false);
                        if (texture != null) break;
                    }

                    if (texture != null)
                    {
                        var bytes = texture.EncodeToPNG();
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            File.WriteAllBytes(HttpManager.Instance.LocalFile(url), bytes);
                        });
                        // GameObject.Destroy(texture);
                        self.CacheOnlineSprite.Add(url, new ImageOnlineInfo(texture, null, 1));
                        return texture;
                    }
                    else
                    {
                        Log.Error("网络无资源 " + url);
                    }
                }
            }
            finally
            {
                coroutineLock?.Dispose();
            }
            return null;
        }
        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="self"></param>
        /// <param name="url"></param>
        /// <param name="clear"></param>
        public static void ReleaseOnlineImage(this ImageOnlineComponent self,string url,bool clear = true)
        {
            if (self.CacheOnlineSprite.TryGetValue(url,out var data))
            {
                data.RefCount--;
                if (clear && data.RefCount <= 0)
                {
                    if (data.Sprite != null)
                    {
                        GameObject.Destroy(data.Sprite);
                    }
                    GameObject.Destroy(data.Texture2D);
                    self.CacheOnlineSprite.Remove(url);
                }
                if (self.CacheOnlineSprite.Count > 10)
                {
                    using (ListComponent<string> temp = ListComponent<string>.Create())
                    {
                        foreach (var item in self.CacheOnlineSprite)
                        {
                            if (item.Value.RefCount == 0)
                            {
                                temp.Add(item.Key);
                            }
                        }

                        for (int i = 0; i < temp.Count; i++)
                        {
                            var img = self.CacheOnlineSprite[temp[i]];
                            if (img.Sprite != null)
                            {
                                GameObject.Destroy(img.Sprite);
                            }
                            GameObject.Destroy(img.Texture2D);
                            self.CacheOnlineSprite.Remove(temp[i]);
                        }
                    } 
                    
                }
            }
        }
    }
}