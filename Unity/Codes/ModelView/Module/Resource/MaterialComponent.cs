using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class MaterialComponent : Entity,IAwake
    {
        public static MaterialComponent Instance { get; set; }
        public Dictionary<string, Material> m_cacheMaterial;
    }
}
