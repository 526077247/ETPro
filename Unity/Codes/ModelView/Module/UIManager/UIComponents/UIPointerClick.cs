using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace ET
{
    public class UIPointerClick : Entity,IAwake,IOnCreate,IOnEnable
    {
        public UnityAction __onclick;
        public PointerClick unity_pointerclick;
    }
}