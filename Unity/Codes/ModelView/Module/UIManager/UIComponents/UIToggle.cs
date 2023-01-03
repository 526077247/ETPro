using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UIComponent]
    public class UIToggle:Entity,IAwake,IOnCreate,IOnEnable
    {
        public Toggle toggle;
        public UnityAction<bool> onValueChange;
    }
}