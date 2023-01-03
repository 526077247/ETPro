using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UIComponent]
    public class UIInput: Entity,IAwake,IOnCreate,IOnEnable
    {
        public InputField input;

        public UnityAction<string> onValueChange;

        public UnityAction<string> onEndEdit;
    }
}
