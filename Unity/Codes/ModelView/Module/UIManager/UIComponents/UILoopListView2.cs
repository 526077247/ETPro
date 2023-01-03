using SuperScrollView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ET
{
    [UIComponent]
    public class UILoopListView2: Entity,IAwake,IOnCreate,IOnEnable
    {
        public LoopListView2 loopListView;
    }
}
