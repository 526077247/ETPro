using System;
using System.Collections.Generic;
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
                self.CallbackQueue = new Dictionary<string, Queue<Action<Sprite>>>();
            }
        }
        
        [ObjectSystem]
        public class ImageOnlineComponentDestroySystem : DestroySystem<ImageOnlineComponent>
        {
            public override void Destroy(ImageOnlineComponent self)
            {
                ImageOnlineComponent.Instance = null;
            }
        }

        /// <summary>
        ///  获取线上图片精灵
        /// </summary>
        /// <param name="image_path">图片地址</param>
        /// <param name="callback">完成回调</param>
        /// <param name="reload">是否重新下载</param>
        public static async ETTask<Sprite> GetOnlineImageSprite(this ImageOnlineComponent self,string image_path,bool reload = false, Action<Sprite> callback=null)
        {
            if(!reload&& self.CacheOnlineSprite.TryGetValue(image_path,out var value))
            {
                value.RefCount++;
                callback?.Invoke(value.Sprite);
            }
            else if(self.CallbackQueue.TryGetValue(image_path,out var queue)&& queue!=null)
            {
                queue.Enqueue(callback);
            }
            else
            {
                self.CallbackQueue[image_path] = new Queue<Action<Sprite>>();
                self.CallbackQueue[image_path].Enqueue(callback);
                return await self.LoadImageOnline(image_path, 3, !reload);//没有找到就去下载
            }
            return null;
        }
        /// <summary>
        /// 加载图片失败递归重试
        /// </summary>
        /// <param name="image_path"></param>
        /// <param name="retryCount"></param>
        /// <param name="islocal"></param>
        /// <returns></returns>
        static async ETTask<Sprite> LoadImageOnline(this ImageOnlineComponent self,string image_path,int retryCount = 3, bool islocal = true)
        {
            if (retryCount <= 0) return null;
            retryCount--;
            Sprite res;
            if (islocal)//先从本地取
            {
                res = await self.HttpGetImage(image_path, null, null, true);
                if (res != null)
                {
                    self.CacheOnlineSprite[image_path] = new ImageOnlineInfo
                    {
                        Sprite = res,
                        RefCount = 1
                    };
                    self.CallBackAll(image_path, res);
                    return res;
                }
                Log.Debug("online_image_info path: " + image_path + " || msg:get img from local fail ");
            }
            // 从网上取
            res = await self.HttpGetImage(image_path);
            if (res != null)
            {
                self.CacheOnlineSprite[image_path] = new ImageOnlineInfo
                {
                    Sprite = res,
                    RefCount = 1
                };
                self.CallBackAll(image_path,res);
                return res;
            }
            else
            {
                return await self.LoadImageOnline(image_path, retryCount, false);// 失败重试
            }
        }

        static void CallBackAll(this ImageOnlineComponent self,string image_path, Sprite res)
        {
            if (self.CallbackQueue.TryGetValue(image_path, out var queue))
            {
                while (queue.Count > 0)
                {
                    queue.Dequeue()?.Invoke(res);
                }
                self.CallbackQueue.Remove(image_path);
            }
        }
        //释放
        public static void ReleaseOnlineSprite(this ImageOnlineComponent self,string image_path)
        {
            if (string.IsNullOrEmpty(image_path)) return;
            if(self.CacheOnlineSprite.TryGetValue(image_path, out var value))
            {
                value.RefCount--;
                if (value.RefCount <= 0)
                {
                    self.CacheOnlineSprite.Remove(image_path);
                }
            }
        }

        public static async ETTask<Sprite> HttpGetImage(this ImageOnlineComponent self,string url, Dictionary<string, string> headers = null, Dictionary<string, string> extparams = null, bool islocal = false)
        {
            if (headers == null) headers = new Dictionary<string, string>();
            var asyncOp = HttpManager.Instance.HttpGetImageOnline(url, islocal, headers);
            while (!asyncOp.isDone)
            {
                await Game.WaitFrameFinish();
            }
            Sprite res = null;
            if (asyncOp.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(asyncOp);
                res = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                if (!islocal)
                {
                    self.SaveImageToLocal(url, texture);
                }
                if (texture != null)
                    GameObject.Destroy(texture);
            }
            asyncOp.Dispose();
            return res;
        }

        public static void SaveImageToLocal(this ImageOnlineComponent self,string url,Texture2D texture)
        {
            GameUtility.SafeWriteAllBytes(HttpManager.Instance.LocalImage(url), texture.EncodeToPNG());
        }

    }
}