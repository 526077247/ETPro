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
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();
            Game.Scene.AddComponent<ResourcesComponent>();
                                    
            //输入订阅组件
            Game.Scene.AddComponent<InputComponent>();
            Game.Scene.AddComponent<InputWatcherComponent>();
            
            Game.Scene.AddComponent<MaterialComponent>();
            Game.Scene.AddComponent<ImageLoaderComponent>();
            Game.Scene.AddComponent<ImageOnlineComponent>();
            Game.Scene.AddComponent<GameObjectPoolComponent>();
            Game.Scene.AddComponent<UIManagerComponent>();
            Game.Scene.AddComponent<UIWatcherComponent>();
            Game.Scene.AddComponent<CameraManagerComponent>();
            Game.Scene.AddComponent<SceneManagerComponent>();
            Game.Scene.AddComponent<AOISceneViewComponent>();
            Game.Scene.AddComponent<ToastComponent>();
            Game.Scene.AddComponent<SoundComponent>();
            // 加载配置
            Game.Scene.AddComponent<SkillStepComponent>();
            Game.Scene.AddComponent<ConfigComponent>();
            ConfigComponent.Instance.Load();
            
            Game.Scene.AddComponent<I18NComponent>();//多语言系统
            Game.Scene.AddComponent<RedDotComponent>();//红点系统
            Game.Scene.AddComponent<ServerConfigComponent>();
            
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            
            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<SessionStreamDispatcher>();
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();
            
            Game.Scene.AddComponent<GlobalComponent>();
            Game.Scene.AddComponent<FSMWatcherComponent>();
            
            // gal命令订阅组件
            Game.Scene.AddComponent<GalGameEngineComponent>();
            Game.Scene.AddComponent<CommandWatcherComponent>();
            
            // 技能订阅组件
            Game.Scene.AddComponent<SkillWatcherComponent>();
            Game.Scene.AddComponent<BuffWatcherComponent>();
            
            Game.Scene.AddComponent<AIDispatcherComponent>();
            Game.Scene.AddComponent<NumericWatcherComponent>();


            Game.Scene.AddComponent<SelectWatcherComponent>();
            
            Game.Scene.AddComponent<UIRouterComponent>();
            Game.Scene.AddComponent<GuidanceComponent>();
            
            await UIManagerComponent.Instance.OpenWindow<UILoadingView>(UILoadingView.PrefabPath);
            if(Define.Networked||Define.ForceUpdate) 
                    //下方代码会初始化Addressables,手机关闭网络等情况访问不到cdn的时候,会卡10s左右。todo:游戏启动时在mono层检查网络
                await UIManagerComponent.Instance.OpenWindow<UIUpdateView,Action>(UIUpdateView.PrefabPath,StartGame);//下载热更资源
            else
                StartGame();
        }
        
        public void StartGame()
        {
            Scene zoneScene = SceneFactory.CreateZoneScene(1, "Game", Game.Scene);
            
            Game.EventSystem.PublishAsync(new EventType.AppStartInitFinish() { ZoneScene = zoneScene }).Coroutine();
        }
    }
}
