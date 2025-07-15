using System.Collections.Generic;
namespace ET
{
	[ComponentOf(typeof(Scene))]
	public class ServerConfigComponent: Entity,IAwake
    {
		public readonly string ServerKey = "ServerId";
		public readonly int DefaultServer = 1;
		public ServerConfig CurConfig;
		public static ServerConfigComponent Instance;
		
		public bool InWhiteList;
		public Dictionary<string, Dictionary<int, Resver>> ResUpdateList;
		public Dictionary<string, AppConfig> AppUpdateList;
		
    }
}
