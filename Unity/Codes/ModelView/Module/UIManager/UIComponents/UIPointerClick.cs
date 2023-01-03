using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace ET
{
    [UIComponent]
    public class UIPointerClick : Entity,IAwake,IOnCreate,IOnEnable
    {
        public UnityAction onClick;
        public PointerClick pointerClick;
    }
}