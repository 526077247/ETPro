using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public class UIImage:Entity,IAwake,IOnCreate,IOnCreate<string>,IOnEnable
    {
        public string sprite_path;
        public Image unity_uiimage;
        public BgAutoFit BgAutoFit;
    }
}
