using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    [UIComponent]
    public class UISlider: Entity,IAwake,IOnCreate,IOnEnable
    {
        public Slider slider;
        public UnityAction<float> onValueChanged;
        public bool isWholeNumbers;
        public ArrayList valueList;
    }
}
