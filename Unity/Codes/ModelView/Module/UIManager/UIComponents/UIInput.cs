using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    public class UIInput: Entity,IAwake,IOnCreate,IOnEnable
    {
        public InputField unity_uiinput;

        public UnityAction<string> __OnValueChange;

        public UnityAction<string> __OnEndEdit;
    }
}
