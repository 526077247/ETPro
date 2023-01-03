using UnityEngine.Events;
namespace ET
{
    [UIComponent]
    public class UIInputTextmesh:Entity,IAwake,IOnCreate,IOnEnable
    {
        public TMPro.TMP_InputField input;
        
        public UnityAction<string> onValueChange;

        public UnityAction<string> onEndEdit;
    }
}
