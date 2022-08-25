using UnityEngine.Events;
namespace ET
{
    public class UIInputTextmesh:Entity,IAwake,IOnCreate,IOnEnable
    {
        public TMPro.TMP_InputField unity_uiinput;
        
        public UnityAction<string> __OnValueChange;

        public UnityAction<string> __OnEndEdit;
    }
}
