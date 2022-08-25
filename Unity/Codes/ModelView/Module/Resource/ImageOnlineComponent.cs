using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ET
{
    public class ImageOnlineInfo
    {
        public int ref_count;
        public Sprite sprite;
    }

    [ComponentOf(typeof(Scene))]
    public class ImageOnlineComponent:Entity,IAwake,IDestroy
    {
        public static ImageOnlineComponent Instance { get; set; }
        public Dictionary<string, ImageOnlineInfo> m_cacheOnlineSprite;
        public Dictionary<string,Queue<Action<Sprite>>> callback_queue;
        
    }
}
