using System;
using System.Threading;

namespace ET
{
	public static class Entry
	{
		public static void Start()
		{
			try
			{
				CodeLoader.Instance.Update += Game.Update;
				CodeLoader.Instance.LateUpdate += Game.LateUpdate;
				CodeLoader.Instance.OnApplicationQuit += Game.Close;
				
				MongoRegister.Init();
				Game.EventSystem.Add(CodeLoader.Instance.GetHotfixTypes());

				
				Game.EventSystem.Publish(new EventType.AppStart());
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}