using UnityEngine;

namespace ET
{
    [UpdateProcess(UpdateTaskStep.SetUpdateListProcess)]
    public class SetUpdateListProcess: IUpdateProcess
    {
        public async ETTask<UpdateRes> Process(UpdateTask task)
        {
            var url = ServerConfigComponent.Instance.GetUpdateListCdnUrl();
            // UpdateConfig aa = new UpdateConfig
            // {
            //     AppList = new()
            //     {
            //         {
            //             "googleplay",
            //             new AppConfig()
            //             {
            //                 AppUrl = "http://127.0.0.1",
            //                 AppVer = new()
            //                 {
            //                     {
            //                         1,
            //                         new Resver()
            //                         {
            //                             Channel = new() {"all"},
            //                             UpdateTailNumber = new() {"all"},
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     },
            //     ResList = new()
            //     {
            //         {
            //             "googleplay",
            //             new()
            //             {
            //                 {
            //                     1,
            //                     new Resver
            //                     {
            //                         Channel = new() {"all"}, UpdateTailNumber = new() {"all"},
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // };
            var info = await HttpManager.Instance.HttpGetResult<UpdateConfig>(url);
            if (info == null)
            {
                var btnState = await task.ShowMsgBoxView(I18NKey.Update_Get_Fail, I18NKey.Update_ReTry, 
                    Define.ForceUpdate?I18NKey.Btn_Exit:I18NKey.Update_Skip);
                
                if (btnState)
                {
                    await this.Process(task);
                }
                else if(Define.ForceUpdate)
                {
                    Application.Quit();
                    return UpdateRes.Quit;
                }
            }
            else
            {
                ServerConfigComponent.Instance.SetUpdateList(info);
            }
            return UpdateRes.Over;
        }
    }
}