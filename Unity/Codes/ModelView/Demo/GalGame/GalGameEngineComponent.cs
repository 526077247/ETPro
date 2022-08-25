using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
	[ComponentOf(typeof(Scene))]
	public class GalGameEngineComponent : Entity,IAwake
	{
		public class ReviewItem
		{
			public string Name;
			public string Content;
			public bool Continue;
		}
		public enum GalGameEngineState
		{
			Preparing = 0,//准备中
			Ready, //就绪
			Running, // 运行中
			Suspended, //暂停
			FastForward, //快进

			Max, //在上方添加状态
		}
		public static GalGameEngineComponent Instance { get; set; }

		public static int FastSpeed = 1000;
		public static int NormalSpeed = 1;
		public FSMComponent FSM;//状态机

		public float Speed;//速度 正常速度是 1

		public Dictionary<string, Dictionary<string, string>> RoleExpressionPathMap;//角色表情路径索引

		public Dictionary<string, Vector3> StagePosMap;//舞台位置坐标索引

		public GalGameEngineState State = GalGameEngineState.Preparing; //引擎状态

		public ChapterCategory CurCategory; //当前播放的剧情

		public int Index;//当前执行到的命令

		public bool AutoPlay;//自动播放

		public ETTask<KeyCode> WaitInput;//等待输入

		public Dictionary<string, string> StageRoleMap;//舞台上的角色 位置:角色

		public Dictionary<string, string> RoleExpressionMap;//角色表情 角色:表情

		public Action<bool> OnPlayOver; //当播放结束，true播完，fale中途退出

		public List<ReviewItem> ReviewItems;//历史记录

		public ETCancellationToken CancelToken;
	}
}
