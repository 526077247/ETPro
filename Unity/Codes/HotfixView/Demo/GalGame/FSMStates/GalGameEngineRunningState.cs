using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class GalGameEngineRunningStateAwakeSystem : AwakeSystem<GalGameEngineRunningState, FSMComponent>
    {
        public override void Awake(GalGameEngineRunningState self, FSMComponent fsm)
        {
            self.FSM = fsm;
            self.Engine = GalGameEngineComponent.Instance;
        }
    }
    [ObjectSystem]
    public class GalGameEngineRunningStateAwakeSystem1 : AwakeSystem<GalGameEngineRunningState>
    {
        public override void Awake(GalGameEngineRunningState self)
        {
            self.FSM = self.GetParent<FSMComponent>();
            self.Engine = GalGameEngineComponent.Instance;
        }
    }
    [FSMSystem]
    [FriendClass(typeof(GalGameEngineComponent))]
    [FriendClass(typeof(GalGameEngineRunningState))]
    public class GalGameEngineRunningStateOnEnterSystem : FSMOnEnterSystem<GalGameEngineRunningState>
    {
        public override async ETTask FSMOnEnter(GalGameEngineRunningState self)
        {
            self.Engine.State = GalGameEngineComponent.GalGameEngineState.Running;
            self.ChapterCategory = self.Engine.CurCategory;
            self.MainRun().Coroutine();
            await ETTask.CompletedTask;
        }
    }

    [FSMSystem]
    public class GalGameEngineRunningStateOnExitSystem : FSMOnExitSystem<GalGameEngineRunningState>
    {
        public override async ETTask FSMOnExit(GalGameEngineRunningState self)
        {
            await self.Stop();
        }
    }
    /// <summary>
    /// 运行
    /// </summary>
    [FriendClass(typeof(GalGameEngineComponent))]
    [FriendClass(typeof(GalGameEngineRunningState))]
    [FriendClass(typeof(I18NComponent))]
    public static class GalGameEngineRunningStateSystem 
    {
        public static async ETTask MainRun(this GalGameEngineRunningState self)
        {
            self.isRunning = true;
            self.stop = false;
            while (!self.stop)
            {
                await self.RunNextCommand();
            }
            self.isRunning = false;
        }

        public static async ETTask Stop(this GalGameEngineRunningState self)
        {
            self.stop = true;
            GalGameEngineComponent.Instance.CancelToken?.Cancel();//广播出去，业务逻辑收到后快速处理完了就直接return
            GalGameEngineComponent.Instance.CancelToken = null;
            while (self.isRunning)
            {
                await TimerComponent.Instance.WaitFrameAsync();
            }
        }

        public static async ETTask RunNextCommand(this GalGameEngineRunningState self)
        {
            if (!self.ChapterCategory.TryGet(self.Engine.Index, out var command))
            {
                self.Engine.PlayOver().Coroutine();
                return;
            }
            GalGameEnginePara para = new GalGameEnginePara();
            para.Command = command.Command;
            //参数赋值
            para.Arg1 = command.Arg1;
            para.Arg2 = command.Arg2;
            para.Arg3 = command.Arg3;
            para.Arg4 = command.Arg4;
            para.Arg5 = command.Arg5;
            para.Arg6 = command.Arg6;
            //多语言处理
            if(I18NComponent.Instance.curLangType==I18NComponent.LangType.Chinese)
                para.Text = command.Chinese;
            else if(I18NComponent.Instance.curLangType==I18NComponent.LangType.English)
                para.Text = command.English;
            else
                para.Text = command.Chinese;
            para.PageCtrl = command.PageCtrl;
            para.WindowType = command.WindowType;
            para.Voice = command.Voice;
            await CommandWatcherComponent.Instance.Run(command.Command, self.Engine, para);
            self.Engine.Index++;
        }
    }
}
