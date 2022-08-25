using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public class UIText : Entity,IAwake,IOnCreate,IOnCreate<string>,IOnEnable,II18N
    {
        public Text unity_uitext;
        public I18NText unity_i18ncomp_touched;
        public string __text_key;
        public object[] keyParams;
    }
}
