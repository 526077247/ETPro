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
            SoundComponent.Instance = self;
            for (int i = 0; i < self._initSoundsCount; i++)
            {
                var item = SoundManager.Instance.CreateClipSource();
                item.gameObject.SetActive(false);
                self._soundsPool.Add(item);
            }
            //给个初始值覆盖
            self.SoundVolume = -1;
            self.MusicVolume = -1;
            self.SetMusicVolume(PlayerPrefs.GetInt(CacheKeys.MusicVolume, 5));
            self.SetSoundVolume(PlayerPrefs.GetInt(CacheKeys.SoundVolume, 5));
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
        public static void SetSoundVolume(this SoundComponent self,int value)
        {
            if (self.SoundVolume != value)
            {
                self.SoundVolume = value;
                SoundManager.Instance.Sound.SetFloat("Sound", (int) self.ValueList[value]);
            }
        }
        
        public static void SetMusicVolume(this SoundComponent self,int value)
        {
            if (self.MusicVolume != value)
            {
                self.MusicVolume = value;
                SoundManager.Instance.BGM.SetFloat("BGM", (int) self.ValueList[value]);
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
                clipSource = SoundManager.Instance.CreateClipSource();
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
            SoundManager.Instance.m_bgm.loop = true;
            AudioClip ac = self.Get(name);
            if (ac == null)
            {
                ac = await ResourcesComponent.Instance.LoadAsync<AudioClip>(name);
                if (ac != null)
                {
                    self.Add(name, ac);
                    SoundManager.Instance.m_bgm.clip = ac;
                    SoundManager.Instance.m_bgm.Play();
                    self.CurMusic = name;
                }
                else {
                    Log.Info("ac is null");
                }
            }
            else
            {
                SoundManager.Instance.m_bgm.clip = ac;
                SoundManager.Instance.m_bgm.Play();
                self.CurMusic = name;
            }
        }

        public static void StopMusic(this SoundComponent self,string path = null)
        {
            if(path==null||path==self.CurMusic)
            {
                if (SoundManager.Instance.m_bgm.clip != null)
                {
                    SoundManager.Instance.m_bgm.Stop();
                    SoundManager.Instance.m_bgm.clip = null;
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
            }
            AudioSource source = self.GetClipSource();
            source.clip = clip;
            source.Play();
            await TimerComponent.Instance.WaitAsync((long)(clip.length*1000)+100);
            source.gameObject.SetActive(false);

        }
    }
}