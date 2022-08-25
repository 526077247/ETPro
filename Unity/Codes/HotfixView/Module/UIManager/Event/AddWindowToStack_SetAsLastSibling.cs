using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
	[FriendClass(typeof(UIWindow))]
	[FriendClass(typeof(UITransform))]
	public class AddWindowToStack_SetAsLastSibling : AEvent<UIEventType.AddWindowToStack>
	{
		protected override void Run(UIEventType.AddWindowToStack args)
		{
			var view = args.window.GetComponent(args.window.ViewType);
			var uiTrans = view.GetUIComponent<UITransform>();
			if (uiTrans!=null)
			{
				uiTrans.transform.SetAsLastSibling();
				GuidanceComponent.Instance.NoticeEvent("Open_"+args.window.Name);
			}
		}
	}
}
