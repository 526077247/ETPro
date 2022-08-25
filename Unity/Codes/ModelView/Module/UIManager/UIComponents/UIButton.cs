using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ET
{
    public class UIButton : Entity,IAwake,IOnCreate,IOnEnable
    {
        public UnityAction __onclick;
        public bool gray_state;
        public string sprite_path;
        public Button unity_uibutton;
        public Image unity_uiimage;
    }
}