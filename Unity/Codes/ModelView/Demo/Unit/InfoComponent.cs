using UnityEngine;
using UnityEngine.UI;
namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class InfoComponent:Entity,IAwake,IDestroy,IUpdate
    {
        public TMPro.TMP_Text Num;
        public Image HpBg;
        public RectTransform obj;
        public Transform head;
    }
}