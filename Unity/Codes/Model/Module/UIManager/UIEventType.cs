using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    namespace UIEventType
    {
        public struct AfterUIManagerCreate
        {

        }

        public struct InnerDestroyWindow
        {
            public UIWindow target;
        }

        public struct InnerOpenWindow
        {
            public UIWindow window;
            public string path;
        }
        public struct ResetWindowLayer
        {
            public UIWindow window;
        }
        public struct AddWindowToStack
        {
            public UIWindow window;
        }

        //加载界面
        public struct LoadingBegin
        {

        }
        public struct LoadingProgress
        {
            public float Progress;
        }

        public struct LoadingFinish
        {
            public string[] cleanup_besides_path;
        }
        //即时消息弹出框
        public struct ShowToast
        {
            public Scene Scene;
            public string Text;
        }
        //新创建UI组件
        public struct AddComponent
        {
            public string Path;
            public Entity entity;
        } 
        public struct SetActive
        {
            public Entity entity;
            public bool Active;
        }
        public struct OnWidthPaddingChange
        {
            public Entity entity;
        }
        /// <summary>
        /// 注册多语言
        /// </summary>
        public struct RegisterI18NEntity
        {
            public Entity entity;
        }
        /// <summary>
        /// 注销多语言
        /// </summary>
        public struct RemoveI18NEntity
        {
            public Entity entity;
        }
        
        /// <summary>
        /// 引导物体
        /// </summary>
        public struct FocuGameObejct
        {
            public UIWindow Win;
            public string Path;
            public int Type;
        }
    }
}
