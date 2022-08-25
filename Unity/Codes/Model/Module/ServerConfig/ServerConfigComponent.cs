using System.Collections.Generic;
namespace ET
{
	public class DownLoadInfo
	{
		public string name;
		public string hash;

	}
	[ComponentOf(typeof(Scene))]
	public class ServerConfigComponent: Entity,IAwake
    {
		public readonly string ServerKey = "ServerId";
		public readonly int defaultServer = 1;
		public ServerConfig cur_config;
		public static ServerConfigComponent Instance;
		
		public string m_update_list_cdn_url;
		public string m_cdn_url;
		public bool m_inWhiteList;
		public Dictionary<string, Dictionary<int, Resver>> m_resUpdateList;
		public Dictionary<string, AppConfig> m_appUpdateList;
		
    }
}
