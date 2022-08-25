using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ObjectSystem]
    public class UIManagerComponentAwakeSystem : AwakeSystem<UIManagerComponent>
    {
        public override void Awake(UIManagerComponent self)
        {
            UIManagerComponent.Instance = self;
            self.windows = new Dictionary<string, UIWindow>();
            self.window_stack = new Dictionary<UILayerNames, LinkedList<string>>();
            Game.EventSystem.Publish(new UIEventType.AfterUIManagerCreate());
            InputWatcherComponent.Instance.RegisterInputEntity(self);
        }
    }

    [ObjectSystem]
    public class UIManagerComponentDestroySystem : DestroySystem<UIManagerComponent>
    {
        public async override void Destroy(UIManagerComponent self)
        {
            await self.DestroyAllWindow();
            self.windows.Clear();
            self.windows = null;
            self.window_stack.Clear();
            self.window_stack = null;
            InputWatcherComponent.Instance?.RemoveInputEntity(self);
            Log.Info("UIManagerComponent Dispose");
        }

    }
    [ObjectSystem]
    public class UIManagerComponentLoadSystem: LoadSystem<UIManagerComponent>
    {
        public override void Load(UIManagerComponent self)
        {
            self.DestroyUnShowWindow().Coroutine();
        }
    }
    /// <summary>
    /// fd: UI管理类，所有UI都应该通过该管理类进行创建 
    /// UIManager.Instance.OpenWindow<T>();
    /// 提供UI操作、UI层级、UI消息、UI资源加载、UI调度、UI缓存等管理
    /// </summary>
    [FriendClass(typeof(UIManagerComponent))]
    [FriendClass(typeof(UIWindow))]
    public static class UIManagerComponentSystem
    {
        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="ui_name"></param>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public static UIWindow GetWindow(this UIManagerComponent self, string ui_name, int active = 0)
        {
            if (self.windows.TryGetValue(ui_name, out var target))
            {
                if (active == 0 || active == (target.Active ? 1 : -1))
                {
                    return target;
                }
                return null;
            }
            return null;
        }
        /// <summary>
        /// 获取最上层window
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ignore">忽略的层级</param>
        /// <returns></returns>
        public static UIWindow GetTopWindow(this UIManagerComponent self,params UILayerNames[] ignore)
        {
            using (HashSetComponent<UILayerNames> ignores = HashSetComponent<UILayerNames>.Create())
            {
                for (int i = 0; i < ignore.Length; i++)
                {
                    ignores.Add(ignore[i]);   
                }
                for (int i = (byte)UILayerNames.TopLayer; i >=0; i--)
                {
                    var layer = (UILayerNames) i;
                    if (!ignores.Contains(layer))
                    {
                        var win = self.GetTopWindow(layer);
                        if (win != null) return win;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 获取最上层window
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static UIWindow GetTopWindow(this UIManagerComponent self,UILayerNames layer)
        {
            var wins = self.window_stack[layer];
            if (wins.Count <= 0) return null;
            for (var node = wins.First; node!=null; node=node.Next)
            {
                var name = node.Value;
                var win = self.GetWindow(name,1);
                if (win != null) 
                    return win;
            }
            return null;

        }
        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public static T GetWindow<T>(this UIManagerComponent self, int active = 0) where T : Entity
        {
            string ui_name = typeof(T).Name;
            if (self!=null&&self.windows!=null&&self.windows.TryGetValue(ui_name, out var target))
            {
                if (active == 0 || active == (target.Active ? 1 : -1))
                {
                    return target.GetComponent<T>();
                }
                return null;
            }
            return null;
        }
        public static async ETTask CloseWindow(this UIManagerComponent self, Entity window)
        {
            string ui_name = window.GetType().Name;
            await self.CloseWindow(ui_name);
        }
        public static async ETTask CloseWindow<T>(this UIManagerComponent self)
        {
            string ui_name = typeof(T).Name;
            await self.CloseWindow(ui_name);
        }
        public static async ETTask CloseWindow(this UIManagerComponent self, string ui_name)
        {
            var target = self.GetWindow(ui_name, 1);
            if (target == null) return;
            while (target.LoadingState != UIWindowLoadingState.LoadOver)
            {
                await TimerComponent.Instance.WaitAsync(1);
            }
            self.__RemoveFromStack(target);
            self.__InnnerCloseWindow(target);
        }
        public static async ETTask CloseSelf(this Entity self)
        {
            await UIManagerComponent.Instance.CloseWindow(self);
        }
        public static async ETTask CloseWindowByLayer(this UIManagerComponent self, UILayerNames layer, params string[] except_ui_names)
        {
            Dictionary<string, bool> dict_ui_names = null;
            if (except_ui_names != null && except_ui_names.Length > 0)
            {
                dict_ui_names = new Dictionary<string, bool>();
                for (int i = 0; i < except_ui_names.Length; i++)
                {
                    dict_ui_names[except_ui_names[i]] = true;
                }
            }

            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var item in self.windows)
                {
                    if (item.Value.Layer == layer && (dict_ui_names == null || !dict_ui_names.ContainsKey(item.Key)))
                    {
                        TaskScheduler.Add(self.CloseWindow(item.Key));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }

        public static async ETTask DestroyWindow<T>(this UIManagerComponent self) where T:Entity
        {
            string ui_name = typeof(T).Name;
            await self.DestroyWindow(ui_name);
        }
        public static async ETTask DestroyWindow(this UIManagerComponent self, string ui_name)
        {
            var target = self.GetWindow(ui_name);
            if (target != null)
            {
                await self.CloseWindow(ui_name);
                Game.EventSystem.Publish(new UIEventType.InnerDestroyWindow() { target = target });
                self.windows.Remove(target.Name);
                target.Dispose();
            }
        }

        /// <summary>
        /// 销毁隐藏状态的窗口
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask DestroyUnShowWindow(this UIManagerComponent self)
        {
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var key in self.windows.Keys.ToList())
                {
                    if (!self.windows[key].Active)
                    {
                        TaskScheduler.Add(self.DestroyWindow(key));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        //打开窗口
        public static async ETTask<T> OpenWindow<T>(this UIManagerComponent self, string path, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : Entity,IAwake,IOnCreate,IOnEnable, new()
        {
            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name);
            if (target == null)
            {
                target = self.__InitWindow<T>(path, layer_name);
                self.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await self.__InnerOpenWindow<T>(target);

        }
        //打开窗口
        public static async ETTask<T> OpenWindow<T, P1>(this UIManagerComponent self, string path, P1 p1, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : Entity,IAwake,IOnCreate,IOnEnable<P1>, new()
        {

            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name);
            if (target == null)
            {
                target = self.__InitWindow<T>(path, layer_name);
                self.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await self.__InnerOpenWindow<T, P1>(target, p1);

        }
        //打开窗口
        public static async ETTask<T> OpenWindow<T, P1, P2>(this UIManagerComponent self, string path, P1 p1, P2 p2, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : Entity,IAwake,IOnCreate,IOnEnable<P1,P2>, new()
        {

            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name);
            if (target == null)
            {
                target = self.__InitWindow<T>(path, layer_name);
                self.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await self.__InnerOpenWindow<T, P1, P2>(target, p1, p2);

        }
        //打开窗口
        public static async ETTask<T> OpenWindow<T, P1, P2, P3>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : Entity,IAwake,IOnCreate,IOnEnable<P1,P2,P3>, new()
        {

            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name);
            if (target == null)
            {
                target = self.__InitWindow<T>(path, layer_name);
                self.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await self.__InnerOpenWindow<T, P1, P2, P3>(target, p1, p2, p3);

        }
        //打开窗口
        public static async ETTask<T> OpenWindow<T, P1, P2, P3, P4>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3, P4 p4, 
            UILayerNames layer_name = UILayerNames.NormalLayer,bool banKey=true) where T : Entity,IAwake,IOnCreate,IOnEnable<P1,P2,P3,P4>, new()
        {

            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name);
            if (target == null)
            {
                target = self.__InitWindow<T>(path, layer_name);
                self.windows[ui_name] = target;
            }
            target.Layer = layer_name;
            target.BanKey = banKey;
            return await self.__InnerOpenWindow<T, P1, P2, P3, P4>(target, p1, p2, p3, p4);

        }
        
        //打开窗口
        public static async ETTask OpenWindowTask<T>(this UIManagerComponent self, string path, UILayerNames layer_name = UILayerNames.NormalLayer) where T : Entity,IAwake,IOnCreate,IOnEnable, new()
        {
            await self.OpenWindow<T>(path,layer_name);
        }
        //打开窗口
        public static async ETTask OpenWindowTask<T, P1>(this UIManagerComponent self, string path, P1 p1, UILayerNames layer_name = UILayerNames.NormalLayer) where T : Entity,IAwake,IOnCreate,IOnEnable<P1>, new()
        {
            await self.OpenWindow<T,P1>(path,p1,layer_name);
        }
        //打开窗口
        public static async ETTask OpenWindowTask<T, P1, P2>(this UIManagerComponent self, string path, P1 p1, P2 p2, UILayerNames layer_name = UILayerNames.NormalLayer) where T : Entity,IAwake,IOnCreate,IOnEnable<P1,P2>, new()
        {
            await self.OpenWindow<T, P1, P2>(path, p1, p2, layer_name);
        }
        //打开窗口
        public static async ETTask OpenWindowTask<T, P1, P2, P3>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3, UILayerNames layer_name = UILayerNames.NormalLayer) where T : Entity,IAwake,IOnCreate,IOnEnable<P1,P2,P3>, new()
        {
            await self.OpenWindow<T, P1, P2,P3>(path, p1, p2,p3, layer_name);
        }
        //打开窗口
        public static async ETTask OpenWindowTask<T, P1, P2, P3, P4>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3, P4 p4, UILayerNames layer_name = UILayerNames.NormalLayer) where T : Entity,IAwake,IOnCreate,IOnEnable<P1,P2,P3,P4>, new()
        {
            await self.OpenWindow<T, P1, P2,P3,P4>(path, p1, p2,p3,p4, layer_name);
        }
        //销毁指定窗口所有窗口
        public static async ETTask DestroyWindowExceptNames(this UIManagerComponent self, string[] type_names = null)
        {
            Dictionary<string, bool> dict_ui_names = new Dictionary<string, bool>();
            if (type_names != null)
            {
                for (int i = 0; i < type_names.Length; i++)
                {
                    dict_ui_names[type_names[i]] = true;
                }
            }
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    if (!dict_ui_names.ContainsKey(keys[i]))
                    {
                        TaskScheduler.Add(self.DestroyWindow(keys[i]));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        //销毁指定层级外层级所有窗口
        public static async ETTask DestroyWindowExceptLayer(this UIManagerComponent self, UILayerNames layer)
        {
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    if (self.windows[keys[i]].Layer != layer)
                    {
                        TaskScheduler.Add(self.DestroyWindow(keys[i]));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        //销毁层级所有窗口
        public static async ETTask DestroyWindowByLayer(this UIManagerComponent self, UILayerNames layer)
        {
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    if (self.windows[keys[i]].Layer == layer)
                    {
                        TaskScheduler.Add(self.DestroyWindow(self.windows[keys[i]].Name));
                    }
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }
        public static async ETTask DestroyAllWindow(this UIManagerComponent self)
        {
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> TaskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    TaskScheduler.Add(self.DestroyWindow(self.windows[keys[i]].Name));
                }
                await ETTaskHelper.WaitAll(TaskScheduler);
            }
        }

        //判断窗口是否打开
        public static bool IsActiveWindow<T>(this UIManagerComponent self) where T : Entity
        {
            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name);
            if (target == null)
            {
                return false;
            }
            return target.Active;
        }
        #region 私有方法
        /// <summary>
        /// 初始化window
        /// </summary>
        static UIWindow __InitWindow<T>(this UIManagerComponent self, string path, UILayerNames layer_name) where T : Entity,IAwake, new()
        {
            UIWindow window = self.AddChild<UIWindow>();
            var type = typeof(T);
            window.Name = type.Name;
            window.Active = false;
            window.ViewType = type;
            window.Layer = layer_name;
            window.LoadingState = UIWindowLoadingState.NotStart;
            window.PrefabPath = path;
            window.AddComponent<T>();
            return window;
        }

        static void __Deactivate(UIWindow target)
        {
            var view = target.GetComponent(target.ViewType);
            if (view != null)
                view.SetActive(false);
        }

        static async ETTask<T> __InnerOpenWindow<T>(this UIManagerComponent self, UIWindow target) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                if (need_load)
                {
                    target.LoadingState = UIWindowLoadingState.Loading;
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }
                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.__AddWindowToStack(target);
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }

        }
        static async ETTask<T> __InnerOpenWindow<T, P1>(this UIManagerComponent self, UIWindow target, P1 p1) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                if (need_load)
                {
                    target.LoadingState = UIWindowLoadingState.Loading;
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }
                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.__AddWindowToStack(target, p1);
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        static async ETTask<T> __InnerOpenWindow<T, P1, P2>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                if (need_load)
                {
                    target.LoadingState = UIWindowLoadingState.Loading;
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }
                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.__AddWindowToStack(target, p1, p2);
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        static async ETTask<T> __InnerOpenWindow<T, P1, P2, P3>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                if (need_load)
                {
                    target.LoadingState = UIWindowLoadingState.Loading;
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }
                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.__AddWindowToStack(target, p1, p2, p3);
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        static async ETTask<T> __InnerOpenWindow<T, P1, P2, P3, P4>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var need_load = target.LoadingState == UIWindowLoadingState.NotStart;
                if (need_load)
                {
                    target.LoadingState = UIWindowLoadingState.Loading;
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }
                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.__AddWindowToStack(target, p1, p2, p3, p4);
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }
        static void __InnnerCloseWindow(this UIManagerComponent self, UIWindow target)
        {
            if (target.Active)
            {
                __Deactivate(target);
                target.Active = false;
            }
        }
        /// <summary>
        /// 将窗口移到当前层级最上方
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static void MoveWindowToTop<T>(this UIManagerComponent self) where T:Entity
        {
            string ui_name = typeof(T).Name;
            var target = self.GetWindow(ui_name,1);
            if (target == null)
            {
               return;
            }
            var layer_name = target.Layer;
            if (self.window_stack[layer_name].Contains(ui_name))
            {
                self.window_stack[layer_name].Remove(ui_name);
            }
            self.window_stack[layer_name].AddFirst(ui_name);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
        }
        static async ETTask __AddWindowToStack(this UIManagerComponent self, UIWindow target)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (self.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                self.window_stack[layer_name].Remove(ui_name);
            }
            self.window_stack[layer_name].AddFirst(ui_name);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        static async ETTask __AddWindowToStack<P1>(this UIManagerComponent self, UIWindow target, P1 p1)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (self.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                self.window_stack[layer_name].Remove(ui_name);
            }
            self.window_stack[layer_name].AddFirst(ui_name);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        static async ETTask __AddWindowToStack<P1, P2>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (self.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                self.window_stack[layer_name].Remove(ui_name);
            }
            self.window_stack[layer_name].AddFirst(ui_name);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1, p2);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        static async ETTask __AddWindowToStack<P1, P2, P3>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (self.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                self.window_stack[layer_name].Remove(ui_name);
            }
            self.window_stack[layer_name].AddFirst(ui_name);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1, p2, p3);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        static async ETTask __AddWindowToStack<P1, P2, P3, P4>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            bool isFirst = true;
            if (self.window_stack[layer_name].Contains(ui_name))
            {
                isFirst = false;
                self.window_stack[layer_name].Remove(ui_name);
            }
            self.window_stack[layer_name].AddFirst(ui_name);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1, p2, p3, p4);
            if (isFirst && (layer_name == UILayerNames.BackgroudLayer || layer_name == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, ui_name);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, ui_name);
            }
        }
        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        static void __RemoveFromStack(this UIManagerComponent self, UIWindow target)
        {
            var ui_name = target.Name;
            var layer_name = target.Layer;
            if (self.window_stack.ContainsKey(layer_name))
            {
                self.window_stack[layer_name].Remove(ui_name);
            }
            else
            {
                Log.Error("not layer, name :" + layer_name);
            }
        }
        #endregion

        #region 屏幕适配

        public static void SetWidthPadding(this UIManagerComponent self, float value)
        {
            self.WidthPadding = value;
            foreach (var layer in self.window_stack.Values)
            {
                if (layer != null)
                {
                    for (LinkedListNode<string> node = layer.First; null != node; node = node.Next)
                    {
                        var target = self.GetWindow(node.Value);
                        var win = target.GetComponent(target.ViewType) as IOnWidthPaddingChange;
                        if (win != null)
                        {
                            EventSystem.Instance.Publish(new UIEventType.OnWidthPaddingChange
                                {entity = win as Entity});
                        }
                    }
                }
            }
        }
        
        #endregion
    }
}
