using System;
using UnityEngine;
namespace ET
{
    [FriendClass(typeof(UpdateTask))]
    public static class UpdateTaskSystem
    {
        [ObjectSystem]
        public class AwakeSystem:AwakeSystem<UpdateTask,Action<long,long>, UpdateTaskStep[]>
        {
            public override void Awake(UpdateTask self, Action<long, long> downloadSizeCallBack, UpdateTaskStep[] process)
            {
                self.onDownloadSize = downloadSizeCallBack;
                self.list = process;
                var vs = Application.version.Split('.');
                self.AppVer = int.Parse(vs[vs.Length-1]);
            }
        }
        
        public static async ETTask<UpdateRes> Process(this UpdateTask self)
        {
            if (self.list == null)
            {
                Log.Error("UpdateTask 未 Init");
                return UpdateRes.Fail;
            }

            for (int i = 0; i < self.list.Length; i++)
            {
                var res = await UpdateTaskWatcherComponent.Instance.Process(self.list[i], self);
                switch (res)
                {
                    case UpdateRes.Fail:
                        Log.Error("Update Fail "+self.list[i].GetType().Name);
                        return UpdateRes.Fail;
                    case UpdateRes.Over:
                        break;
                    case UpdateRes.Quit:
                        Application.Quit();
                        return UpdateRes.Quit;
                    case UpdateRes.Restart:
                        return UpdateRes.Restart;
                }
            }

            return UpdateRes.Over;
        }


        public static void SetDownloadSize(this UpdateTask self,long totalDownloadBytes, long currentDownloadBytes)
        {
            self.onDownloadSize?.Invoke(totalDownloadBytes,currentDownloadBytes);
        }


        #region MsgBox

       

        /// <summary>
        /// 提示窗
        /// </summary>
        /// <param name="content"></param>
        /// <param name="confirmBtnText"></param>
        /// <param name="cancelBtnText"></param>
        /// <returns></returns>
        public static async ETTask<bool> ShowMsgBoxView(this UpdateTask self,I18NKey content, I18NKey confirmBtnText, I18NKey cancelBtnText)
        {
            ETTask<bool> tcs = ETTask<bool>.Create();
            void ConfirmBtnFunc()
            { 
                tcs.SetResult(true);
                UIManagerComponent.Instance.CloseWindow<UIMsgBoxWin>().Coroutine();
            }
            void CancelBtnFunc()
            {
                tcs.SetResult(false);
                UIManagerComponent.Instance.CloseWindow<UIMsgBoxWin>().Coroutine();
            }

            I18NComponent.Instance.I18NTryGetText(content, out self.para.Content);
            I18NComponent.Instance.I18NTryGetText(confirmBtnText, out self.para.ConfirmText);
            I18NComponent.Instance.I18NTryGetText(cancelBtnText, out self.para.CancelText);
            self.para.ConfirmCallback = ConfirmBtnFunc;
            self.para.CancelCallback = CancelBtnFunc;
            await UIManagerComponent.Instance.OpenWindow<UIMsgBoxWin,UIMsgBoxWin.MsgBoxPara>(UIMsgBoxWin.PrefabPath,
                self.para,UILayerNames.TipLayer);
            var result = await tcs;
            return result;
        }
        
        /// <summary>
        /// 提示窗
        /// </summary>
        /// <param name="content"></param>
        /// <param name="confirmBtnText"></param>
        /// <param name="cancelBtnText"></param>
        /// <returns></returns>
        public static async ETTask<bool> ShowMsgBoxView(this UpdateTask self, string content, I18NKey confirmBtnText, I18NKey cancelBtnText)
        {
            ETTask<bool> tcs = ETTask<bool>.Create();
            void ConfirmBtnFunc()
            { 
                tcs.SetResult(true);
                UIManagerComponent.Instance.CloseWindow<UIMsgBoxWin>().Coroutine();
            }
            void CancelBtnFunc()
            {
                tcs.SetResult(false);
                UIManagerComponent.Instance.CloseWindow<UIMsgBoxWin>().Coroutine();
            }

            self.para.Content = content;
            I18NComponent.Instance.I18NTryGetText(confirmBtnText, out self.para.ConfirmText);
            I18NComponent.Instance.I18NTryGetText(cancelBtnText, out self.para.CancelText);
            self.para.ConfirmCallback = ConfirmBtnFunc;
            self.para.CancelCallback = CancelBtnFunc;
            await UIManagerComponent.Instance.OpenWindow<UIMsgBoxWin,UIMsgBoxWin.MsgBoxPara>(UIMsgBoxWin.PrefabPath,
                self.para,UILayerNames.TipLayer);
            var result = await tcs;
            return result;
        }

        #endregion
    }
}