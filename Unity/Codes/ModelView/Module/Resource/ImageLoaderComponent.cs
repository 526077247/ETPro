using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System;

namespace ET
{
    public class SpriteValue
    {
        public Sprite asset;
        public int ref_count;
    }

    public class SpriteAtlasValue
    {
        public Dictionary<string, SpriteValue> subasset;
        public SpriteAtlas asset;
        public int ref_count;
    }
    [ComponentOf(typeof(Scene))]
    public class ImageLoaderComponent : Entity,IAwake,IDestroy
    {
        public readonly string ATLAS_KEY = "/Atlas/";
        public readonly string DYN_ATLAS_KEY="/DynamicAtlas/";
        public static ImageLoaderComponent Instance { get; set; }

        public LruCache<string, SpriteValue> m_cacheSingleSprite;

        public LruCache<string, SpriteAtlasValue> m_cacheSpriteAtlas;

        public Dictionary<string, DynamicAtlas> m_cacheDynamicAtlas;
        
        
        public static class SpriteType
        {
            public static int Sprite = 0;
            public static int SpriteAtlas = 1;
            public static int DynSpriteAtlas = 2;
        }
    }


}