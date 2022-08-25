using System.Collections.Generic;
using UnityEngine;
using System;
namespace ET
{
    public class UIUpdateView : Entity,IAwake,IOnCreate,IOnEnable<Action>
    {
        public readonly int BTN_CANCEL = 1;
        public readonly int BTN_CONFIRM = 2;

        public UISlider m_slider;

        public UIMsgBoxWin.MsgBoxPara para { get; private set; } = new UIMsgBoxWin.MsgBoxPara();
        
        public float last_progress;
        public static string PrefabPath => "UI/UIUpdate/Prefabs/UIUpdateView.prefab";

        public Action OnOver;
        public bool force_update;

        public YooAsset.PatchDownloaderOperation Downloader;
        public int StaticVersion;
    }
}
