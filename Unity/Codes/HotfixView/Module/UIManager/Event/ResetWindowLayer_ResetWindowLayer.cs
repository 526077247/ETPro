using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [FriendClass(typeof(UIWindow))]
    [FriendClass(typeof(UITransform))]
    public class ResetWindowLayer_ResetWindowLayer : AEvent<UIEventType.ResetWindowLayer>
    {
        protected override void Run(UIEventType.ResetWindowLayer args)
        {
            var target = args.window;
            var view = target.GetComponent(target.ViewType);
            var uiTrans = view.GetUIComponent<UITransform>();
            if (uiTrans!=null)
            {
                var layer = UIManagerComponent.Instance.GetLayer(target.Layer);
                uiTrans.transform.SetParent(layer.transform, false);
            }
        }
    }
}
