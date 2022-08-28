using UnityEngine;
using System.Collections.Generic;
namespace ET
{
    [FriendClass(typeof(ServerConfigComponent))]
    public static class ServerConfigComponentSystem
    {
        [ObjectSystem]
        public class ServerConfigComponentAwakeSystem : AwakeSystem<ServerConfigComponent>
        {
            public override void Awake(ServerConfigComponent self)
            {
                ServerConfigComponent.Instance = self;
                if(Define.Debug)
                    self.cur_config = ServerConfigCategory.Instance.Get(PlayerPrefs.GetInt(self.ServerKey, self.defaultServer));
                if (self.cur_config == null)
                {
                    foreach (var item in ServerConfigCategory.Instance.GetAll())
                    {
                        self.cur_config = item.Value;
                        if (item.Value.IsPriority==1)
                            break;
                    }
                }

                self.m_update_list_cdn_url = self.cur_config.UpdateListUrl;
                self.m_cdn_url = self.cur_config.ResUrl;
            }
        }


        public static ServerConfig GetCurConfig(this ServerConfigComponent self)
        {
            return self.cur_config;

        }

        public static ServerConfig ChangeEnv(this ServerConfigComponent self,int id)
        {
            var conf = ServerConfigCategory.Instance.Get(id);
            if(conf!=null)
            {
                self.cur_config = conf;
                if (Define.Debug)
                    PlayerPrefs.SetInt(self.ServerKey, id);
            }
            return self.cur_config;

        }
        
        //获取测试环境更新列表cdn地址
        public static string GetTestUpdateListCdnUrl(this ServerConfigComponent self)
        {
            return self.cur_config.TestUpdateListUrl;
        }

        public static int GetEnvId(this ServerConfigComponent self)
        {
            return self.cur_config.EnvId;
        }
        
        #region 白名单
        //获取白名单下载地址
        public static string GetWhiteListCdnUrl(this ServerConfigComponent self)
        {
            if (string.IsNullOrEmpty(self.m_update_list_cdn_url)) return self.m_update_list_cdn_url;
            return string.Format("{0}/white.list", self.m_update_list_cdn_url);
        }

        //设置白名单模式
        public static void SetWhiteMode(this ServerConfigComponent self,bool whiteMode)
        {
            if (whiteMode)
            {
                self.m_update_list_cdn_url = self.GetTestUpdateListCdnUrl();
            }
        }

        //设置白名单列表
        //格式为json格式
	    //{
		// "WhiteList":[
    	//	    {"env_id":1, "uid":11111}
    	//    ]
        //}
        public static void SetWhiteList(this ServerConfigComponent self,List<WhiteConfig> info)
        {
            self.m_inWhiteList = false;
            var env_id = self.GetEnvId();
            var account = PlayerPrefs.GetString(CacheKeys.Account);
            foreach (var item in info)
            {
                if (item.env_id == env_id && item.account == account)
                {
                    self.m_inWhiteList = true;
                    Log.Info(" user is in white list {0}".Fmt(account));
                    break;
                }
            }
        }
        //是否在白名单中
        public static bool IsInWhiteList(this ServerConfigComponent self)
        {
            return self.m_inWhiteList;
        }
        #endregion

        //获取更新列表地址, 平台独立
        //格式为json格式
        //    {
        //        "res_list" : {
        //                "googleplay": {
        //                       "1.0.0": {"channel": ["all"], "update_tailnumber": ["all"]},
        //                 }
        //        },
        //        "app_list" : { 
        //                 "googleplay": {
        //                      "app_url": "https://www.baidu.com/",
        //                       "app_ver": {
        //	                           "1.0.1": { "force_update": 1 }
        //                       }
        //                  }
        //         }
        //    }
        public static string GetUpdateListCdnUrl(this ServerConfigComponent self)
        {
            var url = string.Format("{0}/update_{1}.list", self.m_update_list_cdn_url, PlatformUtil.GetStrPlatformIgnoreEditor());
            Log.Info("GetUpdateListUrl url = {0}".Fmt(url));
            return url;
        }

        //设置更新列表
        public static void SetUpdateList(this ServerConfigComponent self,UpdateConfig info)
        {
            self.m_appUpdateList = info.app_list;
            self.m_resUpdateList = info.res_list;
        }

        //根据渠道获取app更新列表
        public static  AppConfig GetAppUpdateListByChannel(this ServerConfigComponent self,string channel)
        {
            if (self.m_appUpdateList == null) return null;
            if(self.m_appUpdateList.TryGetValue(channel,out var data))
            {
                if (!string.IsNullOrEmpty(data.jump_channel))
                    data = self.m_appUpdateList[data.jump_channel];
                return data;
            }
            return null;
        }
        //找到可以更新的最大app版本号
        public static int FindMaxUpdateAppVer(this ServerConfigComponent self,string channel)
        {
            if (self.m_appUpdateList == null) return -1;
            int last_ver = -1;
            if (self.m_appUpdateList.TryGetValue(channel, out var data))
            {
                foreach (var item in data.app_ver)
                {
                    if (last_ver == -1) last_ver = item.Key;
                    else
                    {
                        if(item.Key > last_ver)
                        {
                            last_ver = item.Key;
                        }
                    }
                }
            }
            return last_ver;
        }

        //找到可以更新的最大资源版本号
        public static int FindMaxUpdateResVer(this ServerConfigComponent self,string appchannel, string channel,out Resver resver)
        {
            resver = null;
            if (string.IsNullOrEmpty(appchannel) || self.m_resUpdateList == null || 
                !self.m_resUpdateList.TryGetValue(appchannel, out var resVerList)) return -1;
            if (resVerList == null) return -1;
            var verList = new List<int>();
            foreach (var item in resVerList)
            {
                verList.Add(item.Key);
            }
            verList.Sort((a, b) => { return b-a; });
            int last_ver = -1;
            for (int i = 0; i < verList.Count; i++)
            {
                var info = resVerList[verList[i]];
                if(self.IsStrInList(channel,info.channel)&& self.IsInTailNumber(info.update_tailnumber))
                {
                    last_ver = verList[i];
                    break;
                }
            }
            resver = resVerList[last_ver];
            return last_ver;
        }
        //检测灰度更新，检测是否在更新尾号列表
        public static bool IsInTailNumber(this ServerConfigComponent self,List<string> list)
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

        public static bool IsStrInList(this ServerConfigComponent self,string str,List<string> list)
        {
            if (list == null) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i] == "all" || str == list[i])
                    return true;
            return false;
        }

        //根据资源版本号获取在cdn上的资源地址
        public static string GetUpdateCdnResUrlByVersion(this ServerConfigComponent self,string resver)
        {
            var platformStr = PlatformUtil.GetStrPlatformIgnoreEditor();
            var url = string.Format("{0}/{1}_{2}", self.m_cdn_url, resver, platformStr);
            Log.Info("GetUpdateCdnResUrl url = {0}".Fmt(url));
            return url;
        }
    }
}