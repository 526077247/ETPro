using System;
using UnityEngine;

namespace ET
{
    [ChildOf]
    public class UpdateTask: Entity,IAwake<Action<long,long>, UpdateTaskStep[]>
    {
        public int AppVer { get; set; }
        public UpdateTaskStep[] list;
        public Action<long, long> onDownloadSize;
        public int DownloadingMaxNum = 10;
        public int FailedTryAgain = 2;
        public int TimeOut = 8;
        public string[] OtherPackageNames;
        public bool ForceUpdateOtherPackage;
        
        public UIMsgBoxWin.MsgBoxPara para = new UIMsgBoxWin.MsgBoxPara();
    }
}