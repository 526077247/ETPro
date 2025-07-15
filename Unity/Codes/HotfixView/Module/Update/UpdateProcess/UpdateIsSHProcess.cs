using UnityEngine;
using YooAsset;

namespace ET
{
    /// <summary>
    /// 需在SetUpdateListProcess后
    /// </summary>
    [UpdateProcess(UpdateTaskStep.UpdateIsSHProcess)]
    public class UpdateIsSHProcess: IUpdateProcess
    {
        public async ETTask<UpdateRes> Process(UpdateTask task)
        {
            await ETTask.CompletedTask;
            var channel = PackageManager.Instance.CdnConfig.Channel;
            int setVal = UnityEngine.PlayerPrefs.GetInt("DEBUG_IsSH", 0);
            if (setVal == 0)
            {
                Define.IsSH = !ServerConfigComponent.Instance.FindMaxUpdateResVerThisAppVer(channel, task.AppVer,out var maxAppResVer);
            }
            else
            {
                Define.IsSH = setVal == 1;
            }
            Log.Info("提审模式："+ Define.IsSH);
            return UpdateRes.Over;
        }

    }
}