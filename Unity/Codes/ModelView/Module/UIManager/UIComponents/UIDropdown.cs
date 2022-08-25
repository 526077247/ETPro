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
    public class UIDropdown : Entity, IAwake
    {
        public Dropdown unity_uidropdown;
        public UnityAction<int> __onValueChanged;
    }
}