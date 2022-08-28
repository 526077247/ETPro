using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using YooAsset;
using System.Linq;

namespace ET
{
    [UISystem]
    [FriendClass(typeof(UIUpdateView))]
    public class UIUpdateViewOnCreateSystem : OnCreateSystem<UIUpdateView>
    {
        public override void OnCreate(UIUpdateView self)
        {
            self.m_slider = self.AddUIComponent<UISlider>("Loadingscreen/Slider");
        }
    }
    [UISystem]
    [FriendClass(typeof(UIUpdateView))]
    public class UIUpdateViewOnEnableSystem : OnEnableSystem<UIUpdateView,Action>
    {
        public override void OnEnable(UIUpdateView self,Action func)
        {
            self.force_update = Define.ForceUpdate;
            self.OnOver = func;
            self.last_progress = 0;
            self.m_slider.SetValue(0);
            self.StartCheckUpdate().Coroutine();
        }
    }
    [FriendClass(typeof(UIUpdateView))]
    public static class UIUpdateViewSystem
    {
        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        static void SetProgress(this UIUpdateView self, float value)
        {
            if(value> self.last_progress)
                self.last_progress = value;
            self.m_slider.SetNormalizedValue(self.last_progress);
        }
        /// <summary>
        /// 提示窗
        /// </summary>
        /// <param name="self"></param>
        /// <param name="content"></param>
        /// <param name="confirmBtnText"></param>
        /// <param name="cancelBtnText"></param>
        /// <returns></returns>
        async static ETTask<int> ShowMsgBoxView(this UIUpdateView self,string content, string confirmBtnText, string cancelBtnText)
        {
            ETTask<int> tcs = ETTask<int>.Create();
            Action confirmBtnFunc = () =>
             {
                 tcs.SetResult(self.BTN_CONFIRM);
             };

            Action cancelBtnFunc = () =>
            {
                tcs.SetResult(self.BTN_CANCEL);
            };
            I18NComponent.Instance.I18NTryGetText(content, out self.para.Content);
            I18NComponent.Instance.I18NTryGetText(confirmBtnText, out self.para.ConfirmText);
            I18NComponent.Instance.I18NTryGetText(cancelBtnText, out self.para.CancelText);
            self.para.ConfirmCallback = confirmBtnFunc;
            self.para.CancelCallback = cancelBtnFunc;
            await UIManagerComponent.Instance.OpenWindow<UIMsgBoxWin, UIMsgBoxWin.MsgBoxPara>(UIMsgBoxWin.PrefabPath,
                self.para,UILayerNames.TipLayer);
            var result = await tcs;
            await UIManagerComponent.Instance.CloseWindow<UIMsgBoxWin>();
            return result;
        }
        /// <summary>
        /// 开始检测
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask StartCheckUpdate(this UIUpdateView self)
        {
            await self.CheckIsInWhiteList();

            await self.CheckUpdateList();

            var Over = await self.CheckAppUpdate();
            if (Over) return;
            
            var isUpdateDone = await self.CheckResUpdate();
            if (isUpdateDone)
            {
                Log.Info("更新完成，准备进入游戏");
                self.UpdateFinishAndStartGame().Coroutine();
            }
            else
            {
                Log.Info("不需要更新，直接进入游戏");
                self.OnOver?.Invoke();
                await self.CloseSelf();
            }
        }

        #region 更新流程
        
        /// <summary>
        /// 白名单
        /// </summary>
        /// <param name="self"></param>
        async static ETTask CheckIsInWhiteList(this UIUpdateView self)
        {
            var url = ServerConfigComponent.Instance.GetWhiteListCdnUrl();
            if (string.IsNullOrEmpty(url))
            {
                Log.Info(" no white list cdn url");
                return;
            }
            var info = await HttpManager.Instance.HttpGetResult<List<WhiteConfig>>(url);
            if (info != null)
            {
                ServerConfigComponent.Instance.SetWhiteList(info);
                if (ServerConfigComponent.Instance.IsInWhiteList())
                {
                    var btnState = await self.ShowMsgBoxView("Update_White", "Global_Btn_Confirm", "Global_Btn_Cancel");
                    if (btnState == self.BTN_CONFIRM)
                    {
                        ServerConfigComponent.Instance.SetWhiteMode(true);
                    }
                }
                return;
            }
        }

        /// <summary>
        /// 版本号信息
        /// </summary>
        /// <param name="self"></param>
        async static ETTask CheckUpdateList(this UIUpdateView self)
        {
            var url = ServerConfigComponent.Instance.GetUpdateListCdnUrl();
            //UpdateConfig aa = new UpdateConfig
            //{
            //    app_list = new Dictionary<string, AppConfig>
            //    {
                    
            //    },
            //    res_list = new Dictionary<string, Dictionary<string, Resver>>
            //    {
            //        {"100",new Dictionary<string, Resver>{
            //            { "1",new Resver{
            //                channel = new List<string>(){"all"},
            //                update_tailnumber = new List<string>(){"all"},
            //            } }
            //        }}
            //    }
            //};
            var info = await HttpManager.Instance.HttpGetResult<UpdateConfig>(url);
            if (info == null)
            {
                var btnState = await self.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", self.force_update?"Btn_Exit":"Update_Skip");
                if (btnState == self.BTN_CONFIRM)
                {
                    await self.CheckUpdateList();
                }
                else if(self.force_update)
                {
                    GameUtility.Quit();
                    return;
                }
            }
            else
            {
                ServerConfigComponent.Instance.SetUpdateList(info);
            }
        }

        /// <summary>
        /// 是否需要整包更新
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        async static ETTask<bool> CheckAppUpdate(this UIUpdateView self)
        {
            var app_channel = PlatformUtil.GetAppChannel();
            var channel_app_update_list = ServerConfigComponent.Instance.GetAppUpdateListByChannel(app_channel);
            if (channel_app_update_list == null || channel_app_update_list.app_ver == null)
            {
                Log.Info("CheckAppUpdate channel_app_update_list or app_ver is nil, so return");
                return false;
            }
            self.StaticVersion = ServerConfigComponent.Instance.FindMaxUpdateAppVer(app_channel);
            Log.Info("FindMaxUpdateAppVer =" + self.StaticVersion);
            if (self.StaticVersion<0)
            {
                Log.Info("CheckAppUpdate maxVer is nil");
                return false;
            }
            int app_ver = int.Parse(Application.version);
            var flag = app_ver - self.StaticVersion;
            Log.Info(string.Format("CoCheckAppUpdate AppVer:{0} maxVer:{1}", app_ver, self.StaticVersion));
            if (flag >= 0)
            {
                Log.Info("CheckAppUpdate AppVer is Most Max Version, so return; flag = " + flag);
                return false;
            }

            var app_url = channel_app_update_list.app_url;
            var verInfo = channel_app_update_list.app_ver[app_ver];
            Log.Info("CheckAppUpdate app_url = " + app_url);

            self.force_update = Define.ForceUpdate; 
            if (Define.ForceUpdate)//默认强更
            {
                if (verInfo != null && verInfo.force_update == 0)
                    self.force_update = false;
            }
            else
            {
                if (verInfo != null && verInfo.force_update != 0)
                    self.force_update = true;
            }


            var cancelBtnText = self.force_update ? "Btn_Exit" : "Btn_Enter_Game";
            var content_updata = self.force_update ? "Update_ReDownload" : "Update_SuDownload";
            var btnState = await self.ShowMsgBoxView(content_updata, "Global_Btn_Confirm", cancelBtnText);

            if (btnState == self.BTN_CONFIRM)
            {
                GameUtility.OpenURL(app_url);
                //为了防止切换到网页后回来进入了游戏，所以这里需要继续进入该流程
                return await self.CheckAppUpdate();
            }
            else if(self.force_update)
            {
                Log.Info("CheckAppUpdate Need Force Update And User Choose Exit Game!");
                GameUtility.Quit();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 资源更新检查，并根据版本来修改资源cdn地址
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask<bool> CheckResUpdate(this UIUpdateView self)
        {
            var app_channel = PlatformUtil.GetAppChannel();
            var channel = YooAssetsMgr.Instance.Config.Channel;
            self.StaticVersion = ServerConfigComponent.Instance.FindMaxUpdateResVer(channel, app_channel,out var verInfo);
            if (self.StaticVersion<0)
            {
                Log.Info("CheckResUpdate No Max Ver Channel = " + channel + " app_channel " + app_channel);
                return false;
            }
            self.force_update = Define.ForceUpdate; 
            if (!Define.ForceUpdate)//默认强更
            {
                if (verInfo != null && verInfo.force_update != 0)
                    self.force_update = true;
            }
            // if (self.StaticVersion>= maxVer)
            // {
            //     Log.Info("CheckResUpdate ResVer is Most Max Version, so return;");
            //     return false;
            // }

            // 编辑器下跳过。
            // if (Define.IsEditor) return false;
            if (YooAssets.PlayMode != YooAssets.EPlayMode.HostPlayMode)
            {
                Log.Info("非网络运行模式");
                return false;
            }
            ETTask task = ETTask.Create(true);
            // 更新补丁清单
            Log.Info("更新补丁清单");
            var operation = YooAssets.UpdateManifestAsync(self.StaticVersion, 30);
            operation.Completed += (op) =>
            {
                task.SetResult();
            };
            await task;
            int btnState;
            if(operation.Status != EOperationStatus.Succeed)
            {
                btnState = await self.ShowMsgBoxView("Update_Get_Fail", "Update_ReTry", self.force_update?"Btn_Exit":"Update_Skip");
                if (btnState == self.BTN_CONFIRM)
                {
                    return await self.CheckResUpdate();
                }
                else if(self.force_update)
                {
                    GameUtility.Quit();
                    return false;
                }
            }

            Log.Info("创建补丁下载器.");
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            self.Downloader = YooAssets.CreatePatchDownloader(downloadingMaxNum, failedTryAgain);
            if (self.Downloader.TotalDownloadCount == 0)
            {
                Log.Info("没有发现需要下载的资源");
                return false;
            }
            
            //获取需要更新的大小
            var size = self.Downloader.TotalDownloadBytes;
            //提示给用户
            Log.Info("downloadSize " + size);
            double size_mb = size / (1024f * 1024f);
            Log.Info("CheckResUpdate res size_mb is " + size_mb);//不屏蔽
            if (size_mb > 0 && size_mb < 0.01) size_mb = 0.01;

            var ct = I18NComponent.Instance.I18NGetParamText("Update_Info",size_mb.ToString("0.00"));
            btnState = await self.ShowMsgBoxView(ct, "Global_Btn_Confirm", self.force_update?"Btn_Exit":"Update_Skip");
            if (btnState == self.BTN_CANCEL)
            {
                if (self.force_update)
                {
                    GameUtility.Quit();
                    return false;
                }
                return true;
            }

            //开始进行更新

            self.last_progress = 0;
            self.SetProgress(0);
            //2、更新资源
            ETTask<bool> downloadTask = ETTask<bool>.Create(true);
            self.Downloader.OnDownloadOverCallback += (a)=>{downloadTask.SetResult(a);};
            self.Downloader.OnDownloadProgressCallback =(a,b,c,d)=>
            {
                self.SetProgress((float)d/c);
            };
            self.Downloader.BeginDownload();
            Log.Info("CheckResUpdate DownloadContent begin");
            bool result = await downloadTask;
            if (!result) return false;
            Log.Info("CheckResUpdate DownloadContent Success");
            return true;
        }
        
        /// <summary>
        /// 更新完成
        /// </summary>
        /// <param name="self"></param>
        private static async ETTask UpdateFinishAndStartGame(this UIUpdateView self)
        {
            PlayerPrefs.SetInt("STATIC_VERSION",self.StaticVersion);
            PlayerPrefs.Save();
            while (ResourcesComponent.Instance.IsProsessRunning())
            {
                await TimerComponent.Instance.WaitAsync(1);
            }
            ResourcesComponent.Instance.ClearAssetsCache();
            Game.Scene.RemoveAllComponent();
            YooAssetsMgr.Instance.ClearConfigCache();
            //热修复
            // AddressablesManager.Instance.StartInjectFix();
            CodeLoader.Instance.ReStart();
        }
        #endregion

    }
}
