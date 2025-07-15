using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SoundComponent:Entity,IAwake,IDestroy
    {
        public const int DEFAULTVALUE = 10;
        public const int INITSOUNDCOUNT = 3;

        public static SoundComponent Instance;

        public LinkedList<AudioSource> soundsPool = new LinkedList<AudioSource>();

        public Dictionary<long, SoundItem> sounds = new Dictionary<long, SoundItem>();

        public int MusicVolume { get;  set; }
        public int SoundVolume { get;  set; }

        public SoundItem curMusic;
        
        public Transform soundsRoot;

        public GameObject soundsClipClone;
    }
}