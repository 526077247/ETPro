﻿namespace ET
{
	public enum SceneType:byte
	{
		Process = 0,
		Manager = 1,
		Realm = 2,
		Gate = 3,
		Http = 4,
		Location = 5,
		Map = 6,
		Router = 7,

		// 客户端Model层
		Client = 30,
		Zone = 31,
		Login = 32,
		Robot = 33,
		Current = 34,
	}
}