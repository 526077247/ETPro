using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
	public enum UIWindowLoadingState:byte
    {
		NotStart, // 未开始
		Loading, //加载中
		LoadOver, //加载完成
    }
	[ChildOf(typeof(UIManagerComponent))]
	public class UIWindow : Entity,IAwake
	{
		/// <summary>
		/// 窗口名字
		/// </summary>
		public string Name;
		/// <summary>
		/// 是否激活
		/// </summary>
		public bool Active;
		/// <summary>
		/// 是否正在加载
		/// </summary>
		public UIWindowLoadingState LoadingState;
		/// <summary>
		/// 预制体路径
		/// </summary>
		public string PrefabPath;
		/// <summary>
		/// 窗口层级
		/// </summary>
		public UILayerNames Layer;
		/// <summary>
		/// 窗口类型
		/// </summary>
		public Type ViewType { get; set; }
		/// <summary>
		/// 禁止物理按键
		/// </summary>
		public bool BanKey;
	}
}
