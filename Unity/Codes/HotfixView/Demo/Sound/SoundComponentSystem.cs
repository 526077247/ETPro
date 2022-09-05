using UnityEngine;
using System.Collections.Generic;
using System;

namespace ET
{
    [ObjectSystem]
    public class SoundComponentAwakeSystem: AwakeSystem<SoundComponent>
    {
        public override void Awake(SoundComponent self)
        {
            self._soundsRoot = new GameObject("SoundsRoot").transform;
            GameObject.DontDestroyOnLoad(self._soundsRoot);
            var temp = ResourcesComponent.Instance.Load<GameObject>("Audio/Common/BGMManager.prefab");
            var go = GameObject.Instantiate(temp);
            self.m_bgm = go.GetComponent<AudioSource>();
            self.m_bgm.transform.SetParent(self._soundsRoot);
            
            self._soundsClipClone = ResourcesComponent.Instance.Load<GameObject>("Audio/Common/Source.prefab");

            SoundComponent.Instance = self;
            for (int i = 0; i < self._initSoundsCount; i++)
            {
                var item = self.CreateClipSource();
                item.gameObject.SetActive(false);
                self._soundsPool.Add(item);
            }
            
            self.BGM = self.m_bgm.outputAudioMixerGroup.audioMixer;
            self.Sound = self._soundsClipClone.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
            
            //给个初始值覆盖
            self.SoundVolume = -1;
            self.MusicVolume = -1;
            self.SetMusicVolume(PlayerPrefs.GetInt(CacheKeys.MusicVolume, 5));
            self.SetSoundVolume(PlayerPrefs.GetInt(CacheKeys.SoundVolume, 5));
            ResourcesComponent.Instance.ReleaseAsset(temp);
        }
    }
    [ObjectSystem]
    public class SoundComponentDestroySystem: DestroySystem<SoundComponent>
    {
        public override void Destroy(SoundComponent self)
        {
            SoundComponent.Instance = null;
            for (int i = self._soundsPool.Count-1; i >=0 ; i--) {
                GameObject.Destroy(self._soundsPool[i]);
            }
            self._soundsPool = null;
        }
    }
    [FriendClass(typeof(SoundComponent))]
    public static class SoundComponentSystem
    {
        public static AudioSource CreateClipSource(this SoundComponent self)
        {
            if (self._soundsClipClone == null || self._soundsRoot == null)
            {
                return null;
            }

            var obj = GameObject.Instantiate(self._soundsClipClone);
            obj.transform.SetParent(self._soundsRoot, false);
            return obj.GetComponent<AudioSource>();
        }


        public static void SetSoundVolume(this SoundComponent self,int value)
        {
            if (self.SoundVolume != value)
            {
                self.SoundVolume = value;
                self.Sound.SetFloat("Sound", (int) self.ValueList[value]);
            }
        }
        
        public static void SetMusicVolume(this SoundComponent self,int value)
        {
            if (self.MusicVolume != value)
            {
                self.MusicVolume = value;
                self.BGM.SetFloat("BGM", (int) self.ValueList[value]);
            }
        }
        public static AudioSource GetClipSource(this SoundComponent self) {
            AudioSource clipSource = null;
            for (int i = 0; i <  self._soundsPool.Count; i++) {
                if ( self._soundsPool[i].gameObject.activeSelf == false) {
                    clipSource =  self._soundsPool[i];
                    break;
                }
            }
            if (clipSource == null) {
                clipSource = self.CreateClipSource();
                self._soundsPool.Add(clipSource);
            }
            clipSource.gameObject.SetActive(true);
            return clipSource;
        }

        static void Add(this SoundComponent self,string key, AudioClip value) {
            if (self._sounds[key] != null || value == null) return;
            self._sounds.Add(key, value);
        }

        static AudioClip Get(this SoundComponent self,string key) {
            if (self._sounds[key] == null) return null;
            return self._sounds[key] as AudioClip;
        }

        public static void PlayMusic(this SoundComponent self,string name,bool force = false)
        {
            self.CoPlayMusic(name,force).Coroutine();
        }

        private static async ETTask CoPlayMusic(this SoundComponent self,string name,bool force)
        {
            Log.Info("CoPlayMusic");
            if (!force&&self.CurMusic == name) return;
            self.m_bgm.loop = true;
            AudioClip ac = self.Get(name);
            if (ac == null)
            {
                ac = await ResourcesComponent.Instance.LoadAsync<AudioClip>(name);
                if (ac != null)
                {
                    self.Add(name, ac);
                    self.m_bgm.clip = ac;
                    self.m_bgm.Play();
                    self.CurMusic = name;
                }
                else {
                    Log.Info("ac is null");
                }
            }
            else
            {
                self.m_bgm.clip = ac;
                self.m_bgm.Play();
                self.CurMusic = name;
            }
        }

        public static void StopMusic(this SoundComponent self,string path = null)
        {
            if(path==null||path==self.CurMusic)
            {
                if (self.m_bgm.clip != null)
                {
                    self.m_bgm.Stop();
                    self.m_bgm.clip = null;
                    self.CurMusic = null;
                    GameUtility.ClearMemory();
                }
                
            }
        }

        public static void PlaySound(this SoundComponent self,string name) 
        {
            self.PoolPlay(name).Coroutine();
        }

        public static void StopAllSound(this SoundComponent self) 
        {
            for (int i = 0; i < self._soundsPool.Count; i++)
            {
                if (self._soundsPool[i].gameObject.activeSelf)
                {
                    self._soundsPool[i].Stop();
                    self._soundsPool[i].clip = null;
                    self._soundsPool[i].gameObject.SetActive(false);
                }
            }
        }

        private static async ETTask PoolPlay(this SoundComponent self,string name) {
            AudioClip clip = self.Get(name);
            if (clip == null)
            {
                clip =await ResourcesComponent.Instance.LoadAsync<AudioClip>(name);
                if (clip == null)
                {
                    // Debug.Log("clip is null");
                    return;
                }
                self.Add(name, clip);
            }
            AudioSource source = self.GetClipSource();
            source.clip = clip;
            source.Play();
            await TimerComponent.Instance.WaitAsync((long)(clip.length*1000)+100);
            source.gameObject.SetActive(false);

        }
    }
}