using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ET
{
    [InputSystem((int)KeyCode.Mouse0,InputType.KeyDown,int.MaxValue-1000)]
    [InputSystem((int)KeyCode.Mouse0,InputType.KeyUp,int.MaxValue-1000)]
    [InputSystem((int)KeyCode.Mouse0,InputType.Key,int.MaxValue-1000)]
    [InputSystem((int)KeyCode.Mouse1,InputType.KeyDown,int.MaxValue-1000)]
    [InputSystem((int)KeyCode.Mouse1,InputType.KeyUp,int.MaxValue-1000)]
    [InputSystem((int)KeyCode.Mouse1,InputType.Key,int.MaxValue-1000)]
    public class UIManagerComponentInputSystem : InputSystem<UIManagerComponent>
    {
        public override void Run(UIManagerComponent self, int key, int type, ref bool stop)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                stop = true;
            }
        }
    }
    [ObjectSystem]
    [FriendClass(typeof(UIWindow))]
    public class UIManagerComponentUpdateSystem : UpdateSystem<UIManagerComponent>
    {
        public override void Update(UIManagerComponent self)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var win = self.GetTopWindow();
                if (win != null)
                {
                    if(!win.BanKey)
                        UIManagerComponent.Instance.CloseWindow(win.Name).Coroutine();
                }
            }
        }
    }
}