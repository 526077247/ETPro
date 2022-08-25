using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    public class UIToggle:Entity,IAwake,IOnCreate,IOnEnable
    {
        public Toggle unity_uitoggle;
        public UnityAction<bool> CallBack;
    }
}