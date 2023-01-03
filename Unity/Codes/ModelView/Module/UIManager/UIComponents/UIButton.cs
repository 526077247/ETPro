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
    [UIComponent]
    public class UIButton : Entity,IAwake,IOnCreate,IOnEnable
    {
        public UnityAction onClick;
        public bool grayState;
        public string spritePath;
        public Button button;
        public Image image;
    }
}