using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class KeyCodeComponent:Entity,IAwake,IDestroy
    {
        public static KeyCodeComponent Instance;
        
        public Dictionary<int,int> KeyMap { get; set; }

        public readonly Dictionary<int, int> DefaultKeyCodeMap = new Dictionary<int, int>()
        {
            { KeyCodeType.Skill1, 49 },
            { KeyCodeType.Skill2, 50 },
            { KeyCodeType.Skill3, 51 },
            { KeyCodeType.Skill4, 52 },
            { KeyCodeType.Skill5, 53 },
            { KeyCodeType.Skill6, 54 },
        };
    }
}
