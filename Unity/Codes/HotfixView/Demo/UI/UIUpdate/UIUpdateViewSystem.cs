using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using YooAsset;
using System.Linq;

namespace ET
{
    [UISystem]
    [FriendClass(typeof (UIUpdateView))]
    public class UIUpdateViewOnCreateSystem: OnCreateSystem<UIUpdateView>
    {
        public override void OnCreate(UIUpdateView self)
        {
            self.Slider = self.AddUIComponent<UISlider>("Loadingscreen/Slider");
        }
    }

    [UISystem]
    [FriendClass(typeof (UIUpdateView))]
    public class UIUpdateViewOnEnableSystem: OnEnableSystem<UIUpdateView, Action>
    {
        public override void OnEnable(UIUpdateView self, Action func)
        {
            self.OnOver = func;
            self.LastProgress = 0;
            self.Slider.SetValue(0);
            self.StartCheckUpdate().Coroutine();
        }
    }
    [FriendClass(typeof (UpdateTask))]
    [FriendClass(typeof (UIUpdateView))]
    public static class UIUpdateViewSystem
    {
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        static void SetProgress(this UIUpdateView self, float value)
        {
            if (value > self.LastProgress)
                self.LastProgress = value;
            self.Slider.SetNormalizedValue(self.LastProgress);
        }

        /// <summary>
        /// 开始检测
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask StartCheckUpdate(this UIUpdateView self)
        {
            self.SetProgress(0);
            UpdateTask task = self.AddChild<UpdateTask, Action<long, long>, UpdateTaskStep[]>(self.OnDownloadCallBack,
                new UpdateTaskStep[]
                {
                    UpdateTaskStep.SetWhiteListProcess, 
                    UpdateTaskStep.SetUpdateListProcess, 
                    UpdateTaskStep.UpdateIsSHProcess,
                    UpdateTaskStep.AppUpdateProcess, 
                    UpdateTaskStep.MainPackageUpdateProcess,
                    UpdateTaskStep.OtherPackageUpdateProcess,
                });
            using (ListComponent<string> names = ListComponent<string>.Create())
            {
                if (PackageManager.Instance.Config.OtherPackageMaxVer != null)
                {
                    foreach (var item in PackageManager.Instance.Config.OtherPackageMaxVer)
                    {
                        if (item.Value != null) names.AddRange(item.Value);
                    }
                }
                task.OtherPackageNames = names.ToArray();
            }

            var res = await task.Process();
            if (res == UpdateRes.Restart)
            {
                Log.Info("更新完成，准备进入游戏");
                self.UpdateFinishAndStartGame().Coroutine();
            }
            else if (res == UpdateRes.Over)
            {
                Log.Info("不需要重启，直接进入游戏");
                self.OnOver?.Invoke();
            }
            else
            {
                Log.Error("UpdateTask fail");
#if UNITY_EDITOR
                self.OnOver?.Invoke();
#endif
            }
        }
        
        private static void OnDownloadCallBack(this UIUpdateView self, long c, long d)
        {
            float percent = (float) d / c;
            self.SetProgress(percent);
            // size.SetText($"{(d / (1024f * 1024f)).ToString("0.00")}MB/{(c / (1024f * 1024f)).ToString("0.00")}MB");
        }

        /// <summary>
        /// 更新完成
        /// </summary>
        private static async ETTask UpdateFinishAndStartGame(this UIUpdateView self)
        {
            while (ResourcesComponent.Instance.IsProsessRunning())
            {
                await TimerComponent.Instance.WaitAsync(1);
            }

            await UIManagerComponent.Instance.DestroyAllWindow();
            GameObjectPoolComponent.Instance.Cleanup();
            ResourcesComponent.Instance.ClearAssetsCache();
            await PackageManager.Instance.UnloadUnusedAssets(Define.DefaultName);
            ObjectPool.Instance.Dispose();
            CodeLoader.Instance.ReStart();
        }


    }
}
