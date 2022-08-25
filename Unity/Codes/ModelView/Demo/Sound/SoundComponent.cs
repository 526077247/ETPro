using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class SoundComponent:Entity,IAwake,IDestroy
    {
        public static SoundComponent Instance;
        public List<AudioSource> _soundsPool = new List<AudioSource>();
        public int _initSoundsCount = 3;
        public Hashtable _sounds = new Hashtable();

        public int MusicVolume;
        public int SoundVolume;

        public string CurMusic;
        public ArrayList ValueList = new ArrayList(){-80,-30, -20, -10, -5, 0, 1, 2, 4, 6, 10};
    }
}