using System.Collections.Generic;
using UnityEngine;
using System;
namespace ET
{
    [UIComponent]
    public class UIUpdateView : Entity,IAwake,IOnCreate,IOnEnable<Action>
    {
        public UISlider Slider;
        public float LastProgress;
        public Action OnOver;
        public static string PrefabPath => "UI/UIUpdate/Prefabs/UIUpdateView.prefab";
    }
}
