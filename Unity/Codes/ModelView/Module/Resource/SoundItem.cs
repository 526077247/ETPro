using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

namespace ET
{
    [ChildOf(typeof(SoundComponent))]
    public class SoundItem : Entity,IAwake<string,bool,AudioSource,ETCancellationToken>,IDestroy
    {
        public bool IsHttp;
        public AudioSource AudioSource;
        public AudioClip Clip;
        public string Path;
        public ETCancellationToken Token;

        public bool isLoading;
    }
}