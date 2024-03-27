using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [UIComponent]
    public class UIRawImage: Entity,IAwake,IOnCreate,IOnCreate<string>,IOnEnable
    {
        public string spritePath;
        public RawImage image;
        public BgRawAutoFit bgRawAutoFit;
        public bool grayState;
    }
}
