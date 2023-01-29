using UnityEngine;
using System.Collections.Generic;

namespace ET
{
    [FriendClass(typeof (ServerConfigComponent))]
    public static class ServerConfigComponentSystem
    {
        [ObjectSystem]
        public class ServerConfigComponentAwakeSystem: AwakeSystem<ServerConfigComponent>
        {
            public override void Awake(ServerConfigComponent self)
            {
                ServerConfigComponent.Instance = self;
                if (Define.Debug)
                    self.CurConfig = ServerConfigCategory.Instance.Get(PlayerPrefs.GetInt(self.ServerKey, self.DefaultServer));
                if (self.CurConfig == null)
                {
                    foreach (var item in ServerConfigCategory.Instance.GetAll())
                    {
                        self.CurConfig = item.Value;
                        if (item.Value.IsPriority == 1)
                            break;
                    }
                }

                self.UpdateListCdnUrl = self.CurConfig.UpdateListUrl;
            }
        }

        public static ServerConfig GetCurConfig(this ServerConfigComponent self)
        {
            return self.CurConfig;
        }

        public static ServerConfig ChangeEnv(this ServerConfigComponent self, int id)
        {
            var conf = ServerConfigCategory.Instance.Get(id);
            if (conf != null)
            {
                self.CurConfig = conf;
                if (Define.Debug)
                    PlayerPrefs.SetInt(self.ServerKey, id);
            }

            return self.CurConfig;
        }

        //获取测试环境更新列表cdn地址
        public static string GetTestUpdateListCdnUrl(this ServerConfigComponent self)
        {
            return self.CurConfig.TestUpdateListUrl;
        }

        public static int GetEnvId(this ServerConfigComponent self)
        {
            return self.CurConfig.EnvId;
        }

        #region 白名单

        //获取白名单下载地址
        public static string GetWhiteListCdnUrl(this ServerConfigComponent self)
        {
            if (string.IsNullOrEmpty(self.UpdateListCdnUrl)) return self.UpdateListCdnUrl;
            return string.Format("{0}/white.list", self.UpdateListCdnUrl);
        }

        //设置白名单模式
        public static void SetWhiteMode(this ServerConfigComponent self, bool whiteMode)
        {
            if (whiteMode)
            {
                self.UpdateListCdnUrl = self.GetTestUpdateListCdnUrl();
            }
        }

        //设置白名单列表
        //格式为json格式
        //{
        // "WhiteList":[
        //	    {"env_id":1, "uid":11111}
        //    ]
        //}
        public static void SetWhiteList(this ServerConfigComponent self, List<WhiteConfig> info)
        {
            self.InWhiteList = false;
            var env_id = self.GetEnvId();
            var account = PlayerPrefs.GetString(CacheKeys.Account);
            foreach (var item in info)
            {
                if (item.env_id == env_id && item.account == account)
                {
                    self.InWhiteList = true;
                    Log.Info(" user is in white list {0}".Fmt(account));
                    break;
                }
            }
        }

        //是否在白名单中
        public static bool IsInWhiteList(this ServerConfigComponent self)
        {
            return self.InWhiteList;
        }

        #endregion

        //获取更新列表地址, 平台独立
        public static string GetUpdateListCdnUrl(this ServerConfigComponent self)
        {
            var url = string.Format("{0}/update_{1}.list", self.UpdateListCdnUrl, PlatformUtil.GetStrPlatformIgnoreEditor());
            Log.Info("GetUpdateListUrl url = {0}".Fmt(url));
            return url;
        }

        //设置更新列表
        public static void SetUpdateList(this ServerConfigComponent self, UpdateConfig info)
        {
            self.AppUpdateList = info.app_list;
            self.ResUpdateList = info.res_list;
        }

        //根据渠道获取app更新列表
        public static AppConfig GetAppUpdateListByChannel(this ServerConfigComponent self, string channel)
        {
            if (self.AppUpdateList == null) return null;
            if (self.AppUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = self.AppUpdateList[data.jump_channel];
                return data;
            }

            return null;
        }

        //找到可以更新的最大app版本号
        public static int FindMaxUpdateAppVer(this ServerConfigComponent self, string channel)
        {
            if (self.AppUpdateList == null) return -1;
            int last_ver = -1;
            if (self.AppUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = self.AppUpdateList[data.jump_channel];
                foreach (var item in data.app_ver)
                {
                    if (last_ver == -1) last_ver = item.Key;
                    else
                    {
                        if (item.Key > last_ver
                            && self.IsStrInList(channel, item.Value.channel) && self.IsInTailNumber(item.Value.update_tailnumber))
                        {
                            last_ver = item.Key;
                        }
                    }
                }
            }

            return last_ver;
        }

        //找到可以更新的最大资源版本号
        public static int FindMaxUpdateResVer(this ServerConfigComponent self, string appchannel, string channel, out Resver resver)
        {
            resver = null;
            if (string.IsNullOrEmpty(appchannel) || self.ResUpdateList == null ||
                !self.ResUpdateList.TryGetValue(appchannel, out var resVerList)) return -1;
            if (resVerList == null) return -1;
            var verList = new List<int>();
            foreach (var item in resVerList)
            {
                verList.Add(item.Key);
            }

            verList.Sort((a, b) => { return b - a; });
            int last_ver = -1;
            for (int i = 0; i < verList.Count; i++)
            {
                var info = resVerList[verList[i]];
                if (self.IsStrInList(channel, info.channel) && self.IsInTailNumber(info.update_tailnumber))
                {
                    last_ver = verList[i];
                    break;
                }
            }

            resver = resVerList[last_ver];
            return last_ver;
        }

        //检测灰度更新，检测是否在更新尾号列表
        public static bool IsInTailNumber(this ServerConfigComponent self, List<string> list)
        {
            if (list == null) return false;
            var account = PlayerPrefs.GetString(CacheKeys.Account, "");
            var tail_number = "";
            if (!string.IsNullOrEmpty(account))
                tail_number = account[account.Length - 1].ToString();
            for (int i = 0; i < list.Count; i++)
                if (list[i] == "all" || tail_number == list[i])
                    return true;
            return false;
        }

        public static bool IsStrInList(this ServerConfigComponent self, string str, List<string> list)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == "all" || str == list[i])
                    return true;
            return false;
        }
    }
}