using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class I18NComponent : Entity,IAwake,IDestroy
    {
        public static I18NComponent Instance;
        //语言类型枚举
       
        public LangType CurLangType;
        public Dictionary<int, string> I18nTextKeyDic;
        public Dictionary<long, Entity> I18NEntity;
    }

}