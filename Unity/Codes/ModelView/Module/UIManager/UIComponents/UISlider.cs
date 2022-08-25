using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    public class UISlider: Entity,IAwake,IOnCreate,IOnEnable
    {
        public Slider unity_uislider;
        public UnityAction<float> __onValueChanged;
        public bool isWholeNumbers;
        public ArrayList value_list;
    }
}
