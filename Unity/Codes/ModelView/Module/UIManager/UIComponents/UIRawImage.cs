using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
   
    public class UIRawImage: Entity,IAwake,IOnCreate,IOnCreate<string>,IOnEnable
    {
        public string sprite_path;
        public RawImage unity_uiimage;
        public BgRawAutoFit BgRawAutoFit;
    }
}
