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
        
        //获取环境更新列表cdn地址
        public static string GetUpdateListUrl(this ServerConfigComponent self)
        {
            return RemoteServices.Instance?.whiteMode == true? 
                    PackageManager.Instance.CdnConfig.TestUpdateListUrl:PackageManager.Instance.CdnConfig.UpdateListUrl;
        }

        public static int GetEnvId(this ServerConfigComponent self)
        {
            return self.CurConfig.EnvId;
        }

        #region 白名单

        //获取白名单下载地址
        public static string GetWhiteListCdnUrl(this ServerConfigComponent self)
        {
            var url = self.GetUpdateListUrl();
            if (string.IsNullOrEmpty(url)) return url;
            return string.Format("{0}/white.list?timestamp={1}", url, TimeInfo.Instance.ClientNow());
        }

        //设置白名单模式
        public static void SetWhiteMode(this ServerConfigComponent self, bool whiteMode)
        {
            RemoteServices.Instance.whiteMode = whiteMode;
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
                if (item.EnvId == env_id && item.Account == account)
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
            var url = string.Format("{0}/update_{1}.list?timestamp={2}", self.GetUpdateListUrl(), 
                PlatformUtil.GetStrPlatformIgnoreEditor(),TimeInfo.Instance.ClientNow());
            Log.Info("GetUpdateListUrl url = "+url);
            return url;
        }

        //设置更新列表
        public static void SetUpdateList(this ServerConfigComponent self, UpdateConfig info)
        {
            self.AppUpdateList = info.AppList;
            self.ResUpdateList = info.ResList;
        }

        //根据渠道获取app更新列表
        public static AppConfig GetAppUpdateListByChannel(this ServerConfigComponent self, string channel)
        {
            if (self.AppUpdateList == null) return null;
            if (self.AppUpdateList.TryGetValue(channel, out var data))
            {
                if (self.GetJumpChannel(data.JumpChannel,out var jumpData))
                {
                    var newData = new AppConfig();
                    newData.AppVer = jumpData.AppVer;
                    newData.AppUrl = data.AppUrl;
                    return newData;
                }

                return data;
            }

            return null;
        }
        
        private static bool GetJumpChannel(this ServerConfigComponent self,string jumpChannel,out AppConfig jumpData)
        {
            if (!string.IsNullOrEmpty(jumpChannel) && self.AppUpdateList.TryGetValue(jumpChannel, out jumpData))
            {
                if(self.GetJumpChannel(jumpData.JumpChannel,out var newdata))
                {
                    jumpData = newdata;
                }
                return true;
            }
            jumpData = null;
            return false;
        }

        //找到可以更新的最大app版本号
        public static int FindMaxUpdateAppVer(this ServerConfigComponent self, string channel)
        {
            if (self.AppUpdateList == null) return -1;
            int lastVer = -1;
            if (self.AppUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.JumpChannel))
                    data = self.AppUpdateList[data.JumpChannel];
                foreach (var item in data.AppVer)
                {
                    if (lastVer == -1) lastVer = item.Key;
                    else
                    {
                        if(item.Key > lastVer
                           && self.IsStrInList(channel,item.Value.Channel) && self.IsInTailNumber(item.Value.UpdateTailNumber))
                        {
                            lastVer = item.Key;
                        }
                    }
                }
            }
            return lastVer;
        }
        
        public static bool FindMaxUpdateResVerThisAppVer(this ServerConfigComponent self, string channel,int appVer,out int version)
        {
            version = -1;
            if (self.AppUpdateList == null) return false;
            if (self.AppUpdateList.TryGetValue(channel, out var data))
            {
                if (!string.IsNullOrEmpty(data.JumpChannel))
                    data = self.AppUpdateList[data.JumpChannel];
                if (data.AppVer.TryGetValue(appVer, out var res))
                {
                    version = res.MaxResVer;
                    return true;
                }
            }
            return false;
        }

        //找到可以更新的最大资源版本号
        public static int FindMaxUpdateResVer(this ServerConfigComponent self, CDNConfig config, string resverChannel,int appResVer)
        {
            var configChannel = config.GetChannel();
            if (string.IsNullOrEmpty(configChannel) || self.ResUpdateList == null || 
                !self.ResUpdateList.TryGetValue(configChannel, out var resVerList)) return -1;
            if (resVerList == null) return -1;
            var verList = new List<int>();
            foreach (var item in resVerList)
            {
                verList.Add(item.Key);
            }
            verList.Sort((a, b) => { return b-a; });
            int lastVer = -1;
            for (int i = 0; i < verList.Count; i++)
            {
                var info = resVerList[verList[i]];
                if(self.IsStrInList(resverChannel,info.Channel)&& self.IsInTailNumber(info.UpdateTailNumber))
                {
                    lastVer = verList[i];
                    break;
                }
            }

            if (appResVer>0 && lastVer > appResVer&&resVerList.ContainsKey(appResVer))
            {
                return appResVer;
            }

            return lastVer;
        }
        public static Resver GetResVerInfo(this ServerConfigComponent self,CDNConfig config, int version)
        {
            var configChannel = config.GetChannel();
            if (string.IsNullOrEmpty(configChannel) || self.ResUpdateList == null || 
                !self.ResUpdateList.TryGetValue(configChannel, out var resVerList)) return null;
            if (resVerList.TryGetValue(version, out var res))
            {
                return res;
            }

            return null;
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