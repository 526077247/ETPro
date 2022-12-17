using UnityEngine;
using System;
namespace ET
{
    public class AppStart_Init : AEvent<EventType.AppStart>
    {
        protected override void Run(EventType.AppStart args)
        {
            RunAsync(args).Coroutine();
        }
        
        private async ETTask RunAsync(EventType.AppStart args)
        {
            //计时器和协程锁
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();
            
            //资源管理
            Game.Scene.AddComponent<ResourcesComponent>();
            Game.Scene.AddComponent<MaterialComponent>();
            Game.Scene.AddComponent<ImageLoaderComponent>();
            Game.Scene.AddComponent<ImageOnlineComponent>();
            Game.Scene.AddComponent<GameObjectPoolComponent>();
            
            //配置管理
            Game.Scene.AddComponent<ConfigComponent>();
            ConfigComponent.Instance.Load();
            Game.Scene.AddComponent<ServerConfigComponent>();
            
            //多语言系统
            Game.Scene.AddComponent<I18NComponent>();
            
            //输入订阅组件
            Game.Scene.AddComponent<InputComponent>();
            Game.Scene.AddComponent<InputWatcherComponent>();
            
            //ui管理
            Game.Scene.AddComponent<UIManagerComponent>();
            Game.Scene.AddComponent<UIWatcherComponent>();
            Game.Scene.AddComponent<ToastComponent>();
            
            //摄像机与场景管理
            Game.Scene.AddComponent<CameraManagerComponent>();
            Game.Scene.AddComponent<SceneManagerComponent>();
            
            await UIManagerComponent.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
            if(Define.Networked||Define.ForceUpdate)//有网络或者设置为默认强更新的情况下
                await UIManagerComponent.Instance.OpenWindow<UIUpdateView,Action>(UIUpdateView.PrefabPath,StartGame);//尝试下载热更资源
            else
                StartGame();
        }
        
        public void StartGame()
        {

            //音频
            Game.Scene.AddComponent<SoundComponent>();
            
            //红点系统
            Game.Scene.AddComponent<RedDotComponent>();
            
            //网络
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<SessionStreamDispatcher>();
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();
            
            //全局变量
            Game.Scene.AddComponent<GlobalComponent>();
            
            //状态机
            Game.Scene.AddComponent<FSMWatcherComponent>();
            
            //对话
            Game.Scene.AddComponent<GalGameEngineComponent>();
            Game.Scene.AddComponent<CommandWatcherComponent>();
            
            //技能
            Game.Scene.AddComponent<SkillStepComponent>();
            Game.Scene.AddComponent<SkillWatcherComponent>();
            Game.Scene.AddComponent<BuffWatcherComponent>();
            Game.Scene.AddComponent<SelectWatcherComponent>();
            Game.Scene.AddComponent<ConditionWatcherComponent>();
            
            //Ai
            Game.Scene.AddComponent<AIDispatcherComponent>();
            
            //数值
            Game.Scene.AddComponent<NumericWatcherComponent>();
            
            //新手引导
            Game.Scene.AddComponent<UIRouterComponent>();
            Game.Scene.AddComponent<GuidanceComponent>();
            
            //大世界aoi场景
            Game.Scene.AddComponent<AOISceneViewComponent>();
            
            Scene zoneScene = SceneFactory.CreateZoneScene(1, "Game", Game.Scene);
            
            Game.EventSystem.PublishAsync(new EventType.AppStartInitFinish() { ZoneScene = zoneScene }).Coroutine();
        }
    }
}
