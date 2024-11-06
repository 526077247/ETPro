using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
    public class ImageOnlineInfo
    {
        public ImageOnlineInfo(Texture2D texture2D,Sprite sprite,int refCount)
        {
            Texture2D = texture2D;
            Sprite = sprite;
            RefCount = refCount;
        }
        public Texture2D Texture2D;
        public Sprite Sprite;
        public int RefCount;
    }

    [ComponentOf(typeof(Scene))]
    public class ImageOnlineComponent:Entity,IAwake,IDestroy
    {
        public static ImageOnlineComponent Instance { get; set; }
        public Dictionary<string, ImageOnlineInfo> CacheOnlineSprite;

    }
}
