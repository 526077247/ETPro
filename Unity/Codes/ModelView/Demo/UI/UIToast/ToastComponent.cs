using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ToastComponent:Entity,IAwake,IDestroy
    {
        public static ToastComponent Instance;
        public Transform root; 
    }
}
