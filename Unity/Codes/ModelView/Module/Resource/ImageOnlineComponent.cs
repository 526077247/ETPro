using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
    public class ImageOnlineInfo
    {
        public int RefCount;
        public Sprite Sprite;
    }

    [ComponentOf(typeof(Scene))]
    public class ImageOnlineComponent:Entity,IAwake,IDestroy
    {
        public static ImageOnlineComponent Instance { get; set; }
        public Dictionary<string, ImageOnlineInfo> CacheOnlineSprite;
        public Dictionary<string,Queue<Action<Sprite>>> CallbackQueue;
        
    }
}
