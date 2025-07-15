using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace ET
{
    [ObjectSystem]
    public class SoundItemAwakeSystem: AwakeSystem<SoundItem,string,bool,AudioSource,ETCancellationToken>
    {
        public override void Awake(SoundItem self,string path, bool isHttp, AudioSource audioSource,
            ETCancellationToken cancel)
        {
            self.Path = path;
            self.IsHttp = isHttp;
            self.AudioSource = audioSource;
            if (cancel == null)
                self.Token = new ETCancellationToken();
            else
                self.Token = cancel;
        }

    }
    
    [ObjectSystem]
    public class SoundItemDestroySystem: DestroySystem<SoundItem>
    {
        public override void Destroy(SoundItem self)
        {
            if (self.Token != null)
            {
                var token = self.Token;
                self.Token = null;
                token?.Cancel();

                if (self.Clip != null)
                {
                    if (!self.IsHttp)
                    {
                        ResourcesComponent.Instance.ReleaseAsset(self.Clip);
                    }
                    else
                    {
                        GameObject.Destroy(self.Clip);
                    }

                    self.Clip = null;
                }

                if (self.AudioSource != null)
                {
                    self.AudioSource.Stop();
                    self.AudioSource.clip = null;
                    SoundComponent.Instance?.soundsPool.AddLast(self.AudioSource);
                }
                self.AudioSource = null;
            }
        }
    }

    [FriendClass(typeof(SoundItem))]
    public static class SoundItemSystem
    {
        public static async ETTask LoadClip(this SoundItem self)
        {
            if (self.Clip == null)
            {
                var id = self.Id;
                self.isLoading = true;
                if (self.IsHttp)
                {
                    var clip = await self.GetOnlineClip(self.Path);
                    self.isLoading = false;
                    if (self.Id != id)
                    {
                        GameObject.Destroy(clip);
                        ObjectPool.Instance.Recycle(self);
                        return;
                    }
                    if(clip == null) return;
                    self.Clip = clip;
                }
                else
                {
                    var clip = await ResourcesComponent.Instance.LoadAsync<AudioClip>(self.Path);
                    self.isLoading = false;
                    if (self.Id != id)
                    {
                        ResourcesComponent.Instance.ReleaseAsset(clip);
                        ObjectPool.Instance.Recycle(self);
                        return;
                    }
                    if(clip == null) return;
                    self.Clip = clip;
                }
                self.AudioSource.clip = self.Clip;
            }
        }

        #region 在线音频

        private static async ETTask<AudioClip> GetOnlineClip(this SoundItem self,string url, int tryCount = 3,
            ETCancellationToken cancel = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock =
                    await CoroutineLockComponent.Instance.Wait(CoroutineLockType.Resources, url.GetHashCode());

                var clip = await HttpManager.Instance.HttpGetSoundOnline(url, true, cancelToken: cancel);
                if (clip != null) //本地已经存在
                {
                    return clip;
                }
                else
                {
                    for (int i = 0; i < tryCount; i++)
                    {
                        clip = await HttpManager.Instance.HttpGetSoundOnline(url, false, cancelToken: cancel);
                        if (clip != null) break;
                    }

                    if (clip != null)
                    {
#if !UNITY_WEBGL || UNITY_EDITOR
                        var bytes = GetRealAudio(clip);
                        int hz = clip.frequency;
                        int channels = clip.channels;
                        int samples = clip.samples;
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            var path = HttpManager.Instance.LocalFile(url, "downloadSound", ".wav");
                            using (FileStream fs = CreateEmpty(path))
                            {
                                fs.Write(bytes, 0, bytes.Length);
                                WriteHeader(fs, hz, channels, samples);
                            }
                        });
#endif
                        return clip;
                    }
                    else
                    {
                        Log.Error("网络无资源 " + url);
                    }
                }
            }
            finally
            {
                coroutineLock?.Dispose();
            }

            return null;
        }

        /// <summary>
        /// 创建wav格式文件头
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private static FileStream CreateEmpty(string filepath)
        {
            FileStream fileStream = new FileStream(filepath, FileMode.Create);
            byte emptyByte = new byte();

            for (int i = 0; i < 44; i++) //为wav文件头留出空间
            {
                fileStream.WriteByte(emptyByte);
            }

            return fileStream;
        }

        /// <summary>
        /// 写文件头
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="hz"></param>
        /// <param name="channels"></param>
        /// <param name="samples"></param>
        private static void WriteHeader(Stream stream, int hz, int channels, int samples)
        {
            stream.Seek(0, SeekOrigin.Begin);

            Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            stream.Write(riff, 0, 4);

            Byte[] chunkSize = BitConverter.GetBytes(stream.Length - 8);
            stream.Write(chunkSize, 0, 4);

            Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            stream.Write(wave, 0, 4);

            Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            stream.Write(fmt, 0, 4);

            Byte[] subChunk1 = BitConverter.GetBytes(16);
            stream.Write(subChunk1, 0, 4);

            UInt16 one = 1;

            Byte[] audioFormat = BitConverter.GetBytes(one);
            stream.Write(audioFormat, 0, 2);

            Byte[] numChannels = BitConverter.GetBytes(channels);
            stream.Write(numChannels, 0, 2);

            Byte[] sampleRate = BitConverter.GetBytes(hz);
            stream.Write(sampleRate, 0, 4);

            Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
            stream.Write(byteRate, 0, 4);

            UInt16 blockAlign = (ushort) (channels * 2);
            stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            UInt16 bps = 16;
            Byte[] bitsPerSample = BitConverter.GetBytes(bps);
            stream.Write(bitsPerSample, 0, 2);

            Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
            stream.Write(datastring, 0, 4);

            Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
            stream.Write(subChunk2, 0, 4);
        }

        /// <summary>
        /// 获取真正大小的录音
        /// </summary>
        /// <param name="recordedClip"></param>
        /// <returns></returns>
        private static byte[] GetRealAudio(AudioClip recordedClip)
        {
            var position = recordedClip.samples;
            float[] soundata = new float[position * recordedClip.channels];
            recordedClip.GetData(soundata, 0);
            int rescaleFactor = 32767;
            byte[] outData = new byte[soundata.Length * 2];
            for (int i = 0; i < soundata.Length; i++)
            {
                short temshort = (short) (soundata[i] * rescaleFactor);
                byte[] temdata = BitConverter.GetBytes(temshort);
                outData[i * 2] = temdata[0];
                outData[i * 2 + 1] = temdata[1];
            }

            //Debug.Log("position=" + position + "  outData.leng=" + outData.Length);
            return outData;
        }

        #endregion
    }
}