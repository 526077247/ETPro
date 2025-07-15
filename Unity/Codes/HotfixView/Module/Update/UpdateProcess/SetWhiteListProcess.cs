using System.Collections.Generic;

namespace ET
{
    [UpdateProcess(UpdateTaskStep.SetWhiteListProcess)]
    public class SetWhiteListProcess: IUpdateProcess
    {
        public async ETTask<UpdateRes> Process(UpdateTask task)
        {
            var url = ServerConfigComponent.Instance.GetWhiteListCdnUrl();
            if (string.IsNullOrEmpty(url))
            {
                Log.Info(" no white list cdn url");
                return UpdateRes.Over;
            }

            var info = await HttpManager.Instance.HttpGetResult<List<WhiteConfig>>(url);
            if (info != null)
            {
                ServerConfigComponent.Instance.SetWhiteList(info);
                if (ServerConfigComponent.Instance.IsInWhiteList())
                {
                    var btnState = await task.ShowMsgBoxView(I18NKey.Update_White, I18NKey.Global_Btn_Confirm,
                        I18NKey.Global_Btn_Cancel);
                    if (btnState)
                    {
                        ServerConfigComponent.Instance.SetWhiteMode(true);
                    }
                }

                return UpdateRes.Over;
            }

            return UpdateRes.Over;
        }
    }
}