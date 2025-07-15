using UnityEngine;
using YooAsset;

namespace ET
{
    [FriendClass(typeof(UpdateTask))]
    [UpdateProcess(UpdateTaskStep.MainPackageUpdateProcess)]
    public class MainPackageUpdateProcess: IUpdateProcess
    {
        public async ETTask<UpdateRes> Process(UpdateTask task)
        {
            var channel = PackageManager.Instance.CdnConfig.Channel;
            
            bool isAppMaxVer = !ServerConfigComponent.Instance.FindMaxUpdateResVerThisAppVer(channel, task.AppVer, out var maxAppResVer);

            int version = PackageManager.Instance.GetPackageVersion();
            bool forceUpdate = Define.ForceUpdate;
            var verInfo = ServerConfigComponent.Instance.GetResVerInfo(PackageManager.Instance.CdnConfig, version);
            if (verInfo != null && verInfo.ForceUpdate == 1)
                forceUpdate = true;
            
            var maxVer = ServerConfigComponent.Instance.FindMaxUpdateResVer(PackageManager.Instance.CdnConfig, "", maxAppResVer);
            if (maxVer < 0)
            {
                Log.Info("CheckResUpdate No Max Ver Channel = " + channel + " ");
                return UpdateRes.Over;
            }

            if (isAppMaxVer)
            {
                maxVer = version;
            }

            // 非网络运行模式下跳过。
            if (PackageManager.Instance.PlayMode == EPlayMode.WebPlayMode && 
                PackageManager.Instance.PlayMode == EPlayMode.HostPlayMode)
            {
                Log.Info("非网络运行模式");
                return UpdateRes.Over;
            }
            // 更新补丁清单
            Log.Info("更新补丁清单 "+maxVer);

            var op = PackageManager.Instance.UpdatePackageManifestAsync(maxVer.ToString(), task.TimeOut, null);
            await op.Task;
            if(op.Status!= EOperationStatus.Succeed)
            {
                Log.Error(op.Error);
                return await UpdateFail(task, maxVer != version, forceUpdate);
            }

            Log.Info("创建补丁下载器.");
            
            var downloader = PackageManager.Instance.CreateResourceDownloader(task.DownloadingMaxNum, task.FailedTryAgain, task.TimeOut, null);
            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("没有发现需要下载的资源");
                return UpdateRes.Over;
            }
            
            //获取需要更新的大小
            var size = downloader.TotalDownloadBytes;
            //提示给用户
            Log.Info("downloadSize " + size);
            double sizeMb = size / (1024f * 1024f);
            Log.Info("CheckResUpdate res size_mb is " + sizeMb);//不屏蔽
            if (sizeMb > 0 && sizeMb < 0.01) sizeMb = 0.01;
           

            var ct = I18NComponent.Instance.I18NGetParamText(I18NKey.Update_Info, sizeMb.ToString("0.00"));
            var btnState = await task.ShowMsgBoxView(ct, I18NKey.Global_Btn_Confirm,
                forceUpdate ? I18NKey.Btn_Exit : I18NKey.Update_Skip);
            if (!btnState)
            {
                if (forceUpdate)
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }

                //版本号设回去
                if (version != maxVer)
                {
                    return await ResetVersion(task);
                }
                return UpdateRes.Over;
            }

            
            //开始进行更新
            task.SetDownloadSize(size,0);

            //更新资源
            downloader.DownloadUpdateCallback = (a) =>
            {
                task.SetDownloadSize(a.TotalDownloadBytes,a.CurrentDownloadBytes);
            };
            downloader.BeginDownload();
            Log.Info("CheckResUpdate DownloadContent begin");
            await downloader.Task;

            if (downloader.Status != EOperationStatus.Succeed)
            {
                Log.Error(downloader.Error);
                return await UpdateFail(task, maxVer != version, forceUpdate);
            }
            
            Log.Info("CheckResUpdate DownloadContent Success");
            return UpdateRes.Restart;
        }
        
        /// <summary>
        /// 版本号设回去
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async ETTask<UpdateRes> ResetVersion(UpdateTask task)
        {
            var version = PackageManager.Instance.GetPackageVersion().ToString();
            var op = PackageManager.Instance
                .UpdatePackageManifestAsync(version, task.TimeOut, null);
            await op.Task;
            bool res = op.Status == EOperationStatus.Succeed;

            if(!res)
            {
                Log.Error(op.Error);
                //设回去失败
                var btnState =
                    await task.ShowMsgBoxView(I18NKey.Update_Get_Fail, I18NKey.Update_ReTry, I18NKey.Btn_Exit);
                if (btnState)
                {
                    return await Process(task);
                }
                else
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }
            }
            return UpdateRes.Over;
        }
        /// <summary>
        /// 下载失败
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private async ETTask<UpdateRes> UpdateFail(UpdateTask task, bool reset, bool forceUpdate)
        {
            var btnState = await task.ShowMsgBoxView(I18NKey.Update_Get_Fail, I18NKey.Update_ReTry,
                forceUpdate ? I18NKey.Btn_Exit : I18NKey.Update_Skip);
            if (btnState)
            {
                return await this.Process(task);
            }
            else if(forceUpdate)
            {
                Application.Quit();
                return UpdateRes.Quit;
            }

            if(reset) return await ResetVersion(task);
            return UpdateRes.Over;
        }
    }
}