using  System.Linq;

namespace ET
{
    /// <summary>
    /// 等待一段时间
    /// </summary>
    [CommandWatcher("Wait")]
    [FriendClass(typeof(GalGameEngineComponent))]
    public class CommandWatcher_Wait : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            if (!string.IsNullOrEmpty(para.Arg6))
            {
                if (float.TryParse(para.Arg6, out float wait_time))
                {
                    if (self.Speed > 0)
                    {
                        if (GalGameEngineComponent.Instance.CancelToken == null)
                            GalGameEngineComponent.Instance.CancelToken = new ETCancellationToken();
                        await TimerComponent.Instance.WaitAsync((long) (wait_time * 1000 / self.Speed),
                            self.CancelToken);
                    }
                }
            }
        }
    }


    /// <summary>
    /// 打开背景
    /// </summary>
    [CommandWatcher("Bg")]
    public class CommandWatcher_Bg : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            await UIManagerComponent.Instance.OpenWindow<UIBgView, string>(UIBgView.PrefabPath, para.Arg1);
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 关闭背景
    /// </summary>
    [CommandWatcher("BgOff")]
    public class CommandWatcher_BgOff : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            await UIManagerComponent.Instance.CloseWindow<UIBgView>();
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    [CommandWatcher("Bgm")]
    public class CommandWatcher_Bgm : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            if(!string.IsNullOrEmpty(para.Voice))
                SoundComponent.Instance.PlayMusic(para.Voice);
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    [CommandWatcher("StopBgm")]
    public class CommandWatcher_StopBgm : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            SoundComponent.Instance.StopMusic();
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 播放文本
    /// </summary>
    [CommandWatcher("ShowMessageWindow")]
    [FriendClass(typeof(GalGameEngineComponent))]
    public class CommandWatcher_ShowMessageWindow : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            if (!string.IsNullOrEmpty(para.Arg1))
            {
                if (self.RoleExpressionMap.ContainsKey(para.Arg1)) //在场上
                {
                    if (!string.IsNullOrEmpty(para.Arg3))
                        self.StageRoleMap[para.Arg3] = para.Arg1;
                    if (!string.IsNullOrEmpty(para.Arg2))
                        self.RoleExpressionMap[para.Arg1] = para.Arg2;
                }
                else
                {
                    if (string.IsNullOrEmpty(para.Arg3)) para.Arg3 = "default";
                    if (string.IsNullOrEmpty(para.Arg2)) para.Arg2 = "default";
                    self.StageRoleMap[para.Arg3] = para.Arg1;
                    self.RoleExpressionMap[para.Arg1] = para.Arg2;
                }

                await UIManagerComponent.Instance.OpenWindow<UIStageView, GalGameEngineComponent>(
                    UIStageView.PrefabPath, self);
                UIManagerComponent.Instance.MoveWindowToTop<UIMessageWindow>();
            }

            if (float.TryParse(para.Arg6, out var wait_time))
                await self.ShowMessage(para.Text, para.Arg1, para.WindowType, para.PageCtrl, para.Voice,
                    (long) (wait_time * 1000));
            else
                await self.ShowMessage(para.Text, para.Arg1, para.WindowType, para.PageCtrl, para.Voice);
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 隐藏文本框
    /// </summary>
    [CommandWatcher("HideMessageWindow")]
    public class CommandWatcher_HideMessageWindow : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            await UIManagerComponent.Instance.CloseWindow<UIMessageWindow>();
            await UIManagerComponent.Instance.CloseWindow<UIBaseMessageWindow>();
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 结束情景
    /// </summary>
    [CommandWatcher("EndScenario")]
    public class CommandWatcher_EndScenario : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            self.PlayOver().Coroutine();
            await ETTask.CompletedTask;
            //todo: 结束情景
        }
    }

    /// <summary>
    /// 淡入
    /// </summary>
    [CommandWatcher("FadeIn")]
    [FriendClass(typeof(GalGameEngineComponent))]
    public class CommandWatcher_FadeIn : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            if (!float.TryParse(para.Arg6, out var wait_time))
                wait_time = 1;
            if(self.State!= GalGameEngineComponent.GalGameEngineState.FastForward)
                UIManagerComponent.Instance.OpenWindow<UIMaskView,string,float,bool>(UIMaskView.PrefabPath,para.Arg1,wait_time,
                    true,UILayerNames.TopLayer).Coroutine();
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 淡出
    /// </summary>
    [CommandWatcher("FadeOut")]
    [FriendClass(typeof(GalGameEngineComponent))]
    public class CommandWatcher_FadeOut : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            if (!float.TryParse(para.Arg6, out var wait_time))
                wait_time = 1;
            if(self.State!= GalGameEngineComponent.GalGameEngineState.FastForward)
                UIManagerComponent.Instance.OpenWindow<UIMaskView,string,float,bool>(UIMaskView.PrefabPath,para.Arg1,wait_time,
                    false,UILayerNames.TopLayer).Coroutine();
            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }
    }

    /// <summary>
    /// 人物离场
    /// </summary>
    [CommandWatcher("CharacterOff")]
    [FriendClass(typeof(GalGameEngineComponent))]
    public class CommandWatcher_CharacterOff : ICommandWatcher
    {
        public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
        {
            if (string.IsNullOrEmpty(para.Arg1)) //全下场
            {
                self.StageRoleMap.Clear();
                self.RoleExpressionMap.Clear();
                await UIManagerComponent.Instance.CloseWindow<UIStageView>();
                await UIManagerComponent.Instance.CloseWindow<UIMessageWindow>();
            }
            else //下场指定角色
            {
                if (self.RoleExpressionMap.ContainsKey(para.Arg1))
                    self.RoleExpressionMap.Remove(para.Arg1);
                var keys = self.StageRoleMap.Keys.ToList();
                for (int i = 0; i < keys.Count; i++)
                {
                    if (self.StageRoleMap[keys[i]] == para.Arg1)
                    {
                        self.StageRoleMap.Remove(keys[i]);
                        break;
                    }
                }

                await UIManagerComponent.Instance.OpenWindow<UIStageView, GalGameEngineComponent,GalGameEnginePara>(
                    UIStageView.PrefabPath, self,para);
                await UIManagerComponent.Instance.CloseWindow<UIMessageWindow>();
            }

            await CommandWatcherComponent.Instance.Run("Wait", self, para);
        }

        /// <summary>
        /// 人物登场
        /// </summary>
        [CommandWatcher("CharacterOn")]
        [FriendClass(typeof(GalGameEngineComponent))]
        public class CommandWatcher_CharacterOn : ICommandWatcher
        {
            public async ETTask Run(GalGameEngineComponent self, GalGameEnginePara para)
            {
                if (!string.IsNullOrEmpty(para.Arg1))
                {
                    if (self.RoleExpressionMap.ContainsKey(para.Arg1)) //在场上
                    {
                        if (!string.IsNullOrEmpty(para.Arg3))
                            self.StageRoleMap[para.Arg3] = para.Arg1;
                        if (!string.IsNullOrEmpty(para.Arg2))
                            self.RoleExpressionMap[para.Arg1] = para.Arg2;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(para.Arg3)) para.Arg3 = "default";
                        if (string.IsNullOrEmpty(para.Arg2)) para.Arg2 = "default";
                        self.StageRoleMap[para.Arg3] = para.Arg1;
                        self.RoleExpressionMap[para.Arg1] = para.Arg2;
                    }

                    await UIManagerComponent.Instance
                        .OpenWindow<UIStageView, GalGameEngineComponent, GalGameEnginePara>(
                            UIStageView.PrefabPath, self, para);
                    UIManagerComponent.Instance.MoveWindowToTop<UIMessageWindow>();
                }

                await CommandWatcherComponent.Instance.Run("Wait", self, para);
            }
        }
    }
}