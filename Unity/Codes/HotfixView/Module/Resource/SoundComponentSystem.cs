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
            InitAsync(self).Coroutine();
        }
        
        private async ETTask InitAsync(SoundComponent self)
        {
            self.soundsRoot = new GameObject("SoundsRoot").transform;
            GameObject.DontDestroyOnLoad(self.soundsRoot);
            self.soundsClipClone =
                    await ResourcesComponent.Instance.LoadAsync<GameObject>("Audio/Common/Source.prefab", isPersistent: true);

            for (int i = 0; i < SoundComponent.INITSOUNDCOUNT; i++)
            {
                var item = self.CreateClipSource();
                self.soundsPool.AddLast(item);
            }
            
            //给个初始值覆盖
            self.SoundVolume = -1;
            self.MusicVolume = -1;
            self.SetMusicVolume(PlayerPrefs.GetInt(CacheKeys.MusicVolume, SoundComponent.DEFAULTVALUE));
            self.SetSoundVolume(PlayerPrefs.GetInt(CacheKeys.SoundVolume, SoundComponent.DEFAULTVALUE));
        }
    }

    [ObjectSystem]
    public class SoundComponentDestroySystem: DestroySystem<SoundComponent>
    {
        public override void Destroy(SoundComponent self)
        {
            SoundComponent.Instance = null;
            self.StopMusic();
            self.StopAllSound();
            foreach (var item in  self.sounds)
            {
                item.Value.Dispose();
            }

            self.sounds = null;
            foreach (var item in  self.soundsPool)
            {
                GameObject.Destroy(item);
            }

            self.soundsPool = null;
            ResourcesComponent.Instance?.ReleaseAsset( self.soundsClipClone);
            self.soundsClipClone = null;
            if ( self.soundsRoot != null)
            {
                GameObject.Destroy( self.soundsRoot.gameObject);
                self.soundsRoot = null;
            }
        }
    }
    [FriendClass(typeof(SoundComponent))]
    [FriendClass(typeof(SoundItem))]
    public static class SoundComponentSystem
    {
        #region Setting

        public static void SetSoundVolume(this SoundComponent self, int value)
        {
            if (self.SoundVolume != value)
            {
                self.SoundVolume = value;
                foreach (var item in self.sounds)
                {
                    if (item.Value?.AudioSource != null && item.Value!=self.curMusic)
                    {
                        item.Value.AudioSource.volume = self.SoundVolume / 10f;
                    }
                }
            }
        }

        public static void SetMusicVolume(this SoundComponent self, int value)
        {
            if (self.MusicVolume != value)
            {
                self.MusicVolume = value;
                if (self.curMusic?.AudioSource != null)
                {
                    self.curMusic.AudioSource.volume = self.MusicVolume / 10f;
                }
            }
        }

        #endregion

        #region Music

        public static long PlayMusic(this SoundComponent self, string path, ETCancellationToken token = null)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            AudioSource source = self.GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return 0;
            }
            source.loop = true;
            source.volume = self.MusicVolume / 10f;
            SoundItem res = self.AddChild<SoundItem,string,bool,AudioSource,ETCancellationToken>(path, false, source, token);
            self.PoolPlay(res, res.Token).Coroutine();
            self.curMusic?.Dispose();
            self.curMusic = res;
            return res.Id;
        }

        public static void PauseMusic(this SoundComponent self, bool pause)
        {
            if (self.curMusic == null) return;
            if (pause)
                self.curMusic.AudioSource.Pause();
            else
                self.curMusic.AudioSource.UnPause();
        }

        public static void StopMusic(this SoundComponent self, long id = 0)
        {
            if (self.curMusic == null) return;
            if (id == 0 || id == self.curMusic.Id)
            {
                if (self.curMusic.Clip != null)
                {
                    self.curMusic.Dispose();
                    self.curMusic = null;
                    self.ClearMemory();
                }

            }
        }

        #endregion

        #region Sound

        public static long PlaySound(this SoundComponent self, string path, ETCancellationToken token = null, bool isLoop = false)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            AudioSource source = self.GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return 0;
            }
            source.loop = isLoop;
            source.volume = self.SoundVolume / 10f;
            SoundItem res = self.AddChild<SoundItem,string,bool,AudioSource,ETCancellationToken>(path, false, source, token);
            self.PoolPlay(res, res.Token).Coroutine();
            return res.Id;
        }

        public static async ETTask PlaySoundAsync(this SoundComponent self, string path, ETCancellationToken token = null)
        {
            if (string.IsNullOrEmpty(path)) return;
            AudioSource source =  self.GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return;
            }

            source.loop = false;
            source.volume =  self.SoundVolume / 10f;
            SoundItem res = self.AddChild<SoundItem,string,bool,AudioSource,ETCancellationToken>(path, false, source, token);
            await self.PoolPlay(res, res.Token);
        }
        public static long PlayHttpAudio(this SoundComponent self, string url, bool loop = false, ETCancellationToken cancel = null)
        {
            if (string.IsNullOrEmpty(url)) return 0;
            AudioSource source = self.GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return 0;
            }
            source.loop = loop;
            source.volume = self.SoundVolume / 10f;
            SoundItem res = self.AddChild<SoundItem,string,bool,AudioSource,ETCancellationToken>(url, true, source, cancel);
            self.PoolPlay(res, res.Token).Coroutine();
            return res.Id;
        }
        public static async ETTask PlayHttpAudioAsync(this SoundComponent self, string url, bool loop = false, ETCancellationToken cancel = null)
        {
            if (string.IsNullOrEmpty(url)) return;
            AudioSource source = self.GetClipSource();
            if (source == null)
            {
                Log.Error("GetClipSource fail");
                return;
            }
            source.loop = loop;
            source.volume = self.SoundVolume / 10f;
            SoundItem res = self.AddChild<SoundItem,string,bool,AudioSource,ETCancellationToken>(url, true, source, cancel);
            await self.PoolPlay(res, res.Token);
        }

        public static void StopSound(this SoundComponent self, long id)
        {
            var old = self.Get(id);
            if (old != null)
            {
                old.Dispose();
                self.sounds.Remove(id);
                self.ClearMemory();
            }
        }

        public static void StopAllSound(this SoundComponent self)
        {
            foreach (var item in self.sounds)
            {
                item.Value.Dispose();
            }
            self.sounds.Clear();
        }

        private static async ETTask PoolPlay(this SoundComponent self, SoundItem soundItem, ETCancellationToken token)
        {
            var id = soundItem.Id;
            self.Add(soundItem.Id, soundItem);
            await soundItem.LoadClip();
            if (soundItem.Clip == null)
            {
                return;
            }
            if (token.IsCancel())
            {
                soundItem.Dispose();
                return;
            }
            soundItem.AudioSource.Play();
            if (soundItem.AudioSource.loop) return;
            await TimerComponent.Instance.WaitAsync((long) (soundItem.Clip.length * 1000) + 100, token);
            if (soundItem.Id == id)
            {
                //回来可能被其他提前终止了
                soundItem.Dispose();
            }
        }

        #endregion

        #region Clip

        public static AudioSource CreateClipSource(this SoundComponent self)
        {
            if (self.soundsClipClone == null || self.soundsRoot == null)
            {
                Log.Error("soundsRoot == null");
                return null;
            }

            var obj = GameObject.Instantiate(self.soundsClipClone);
            obj.transform.SetParent(self.soundsRoot, false);
            return obj.GetComponent<AudioSource>();
        }

        private static AudioSource GetClipSource(this SoundComponent self)
        {
            AudioSource clipSource = null;
            if (self.soundsPool.Count > 0)
            {
                clipSource = self.soundsPool.First.Value;
                self.soundsPool.RemoveFirst();
            }

            if (clipSource == null)
            {
                clipSource = self.CreateClipSource();
                if (clipSource == null) return null;
            }
            
            return clipSource;
        }

        static void Add(this SoundComponent self, long id, SoundItem value)
        {
            if (value == null) return;
            self.sounds[id] = value;
        }

        static SoundItem Get(this SoundComponent self, long id)
        {
            if (!self.sounds.TryGetValue(id, out var res) || res == null) return null;
            return res;
        }

        #endregion

        #region Clear

        private static void ClearMemory(this SoundComponent self)
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        #endregion
    }
}