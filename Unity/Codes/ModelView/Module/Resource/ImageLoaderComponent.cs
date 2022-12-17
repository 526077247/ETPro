using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System;

namespace ET
{
    public class SpriteValue
    {
        public Sprite Asset;
        public int RefCount;
    }

    public class SpriteAtlasValue
    {
        public Dictionary<string, SpriteValue> SubAsset;
        public SpriteAtlas Asset;
        public int RefCount;
    }
    [ComponentOf(typeof(Scene))]
    public class ImageLoaderComponent : Entity,IAwake,IDestroy
    {
        public readonly string ATLAS_KEY = "/Atlas/";
        public readonly string DYN_ATLAS_KEY="/DynamicAtlas/";
        public static ImageLoaderComponent Instance { get; set; }

        public LruCache<string, SpriteValue> cacheSingleSprite;

        public LruCache<string, SpriteAtlasValue> cacheSpriteAtlas;

        public Dictionary<string, DynamicAtlas> cacheDynamicAtlas;
        
        
        public static class SpriteType
        {
            public static int Sprite = 0;
            public static int SpriteAtlas = 1;
            public static int DynSpriteAtlas = 2;
        }
    }


}