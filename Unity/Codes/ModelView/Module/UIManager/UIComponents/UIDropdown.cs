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
    public class UIDropdown : Entity, IAwake
    {
        public Dropdown dropdown;
        public UnityAction<int> onValueChanged;
    }
}