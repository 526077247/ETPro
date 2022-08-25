using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    
    /// <summary>
    /// 已就绪
    /// </summary>
    public class GalGameEngineReadyState : Entity,IAwake
    {
       
        public FSMComponent FSM;
        public GalGameEngineComponent Engine;
    }
}
