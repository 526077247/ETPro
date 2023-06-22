using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ObjectSystem]
    public class UIManagerComponentAwakeSystem: AwakeSystem<UIManagerComponent>
    {
        public override void Awake(UIManagerComponent self)
        {
            UIManagerComponent.Instance = self;
            self.windows = new Dictionary<string, UIWindow>();
            self.windowStack = new Dictionary<UILayerNames, LinkedList<string>>();
            Game.EventSystem.Publish(new UIEventType.AfterUIManagerCreate());
            InputWatcherComponent.Instance.RegisterInputEntity(self);
        }
    }

    [ObjectSystem]
    public class UIManagerComponentDestroySystem: DestroySystem<UIManagerComponent>
    {
        public async override void Destroy(UIManagerComponent self)
        {
            await self.DestroyAllWindow();
            self.windows.Clear();
            self.windows = null;
            self.windowStack.Clear();
            self.windowStack = null;
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
    /// UI管理类，所有UI都应该通过该管理类进行创建 
    /// UIManager.Instance.OpenWindow();
    /// 提供UI操作、UI层级、UI消息、UI资源加载、UI调度、UI缓存等管理
    /// </summary>
    [FriendClass(typeof (UIManagerComponent))]
    [FriendClass(typeof (UIWindow))]
    public static class UIManagerComponentSystem
    {
        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="self"></param>
        /// <param name="uiName"></param>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public static UIWindow GetWindow(this UIManagerComponent self, string uiName, int active = 0)
        {
            if (self.windows.TryGetValue(uiName, out var target))
            {
                if (active == 0 || active == (target.Active? 1 : -1))
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
        public static UIWindow GetTopWindow(this UIManagerComponent self, params UILayerNames[] ignore)
        {
            using (HashSetComponent<UILayerNames> ignores = HashSetComponent<UILayerNames>.Create())
            {
                for (int i = 0; i < ignore.Length; i++)
                {
                    ignores.Add(ignore[i]);
                }

                for (int i = (byte) UILayerNames.TopLayer; i >= 0; i--)
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
        public static UIWindow GetTopWindow(this UIManagerComponent self, UILayerNames layer)
        {
            var wins = self.windowStack[layer];
            if (wins.Count <= 0) return null;
            for (var node = wins.First; node != null; node = node.Next)
            {
                var name = node.Value;
                var win = self.GetWindow(name, 1);
                if (win != null)
                    return win;
            }

            return null;

        }

        /// <summary>
        /// 获取UI窗口
        /// </summary>
        /// <param name="self"></param>
        /// <param name="active">1打开，-1关闭,0不做限制</param>
        /// <returns></returns>
        public static T GetWindow<T>(this UIManagerComponent self, int active = 0) where T : Entity
        {
            string uiName = TypeInfo<T>.TypeName;
            if (self != null && self.windows != null && self.windows.TryGetValue(uiName, out var target))
            {
                if (active == 0 || active == (target.Active? 1 : -1))
                {
                    return target.GetComponent<T>();
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="self"></param>
        /// <param name="window"></param>
        public static async ETTask CloseWindow(this UIManagerComponent self, Entity window)
        {
            string uiName = window.GetType().Name;
            await self.CloseWindow(uiName);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static async ETTask CloseWindow<T>(this UIManagerComponent self)
        {
            string uiName = TypeInfo<T>.TypeName;
            await self.CloseWindow(uiName);
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="self"></param>
        /// <param name="uiName"></param>
        public static async ETTask CloseWindow(this UIManagerComponent self, string uiName)
        {
            var target = self.GetWindow(uiName, 1);
            if (target == null) return;
            while (target.LoadingState != UIWindowLoadingState.LoadOver)
            {
                await Game.WaitFrameFinish();
            }

            self.RemoveFromStack(target);
            self.InnnerCloseWindow(target);
        }

        /// <summary>
        /// 关闭自身
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask CloseSelf(this Entity self)
        {
            await UIManagerComponent.Instance.CloseWindow(self);
        }

        /// <summary>
        /// 通过层级关闭
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layer"></param>
        /// <param name="exceptUiNames"></param>
        public static async ETTask CloseWindowByLayer(this UIManagerComponent self, UILayerNames layer, params string[] exceptUiNames)
        {
            Dictionary<string, bool> dictUINames = null;
            if (exceptUiNames != null && exceptUiNames.Length > 0)
            {
                dictUINames = new Dictionary<string, bool>();
                for (int i = 0; i < exceptUiNames.Length; i++)
                {
                    dictUINames[exceptUiNames[i]] = true;
                }
            }

            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var item in self.windows)
                {
                    if (item.Value.Layer == layer && (dictUINames == null || !dictUINames.ContainsKey(item.Key)))
                    {
                        taskScheduler.Add(self.CloseWindow(item.Key));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static async ETTask DestroyWindow<T>(this UIManagerComponent self) where T : Entity
        {
            string uiName = TypeInfo<T>.TypeName;
            await self.DestroyWindow(uiName);
        }

        /// <summary>
        /// 销毁窗体
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static async ETTask DestroyWindow(this UIManagerComponent self, string uiName)
        {
            var target = self.GetWindow(uiName);
            if (target != null)
            {
                await self.CloseWindow(uiName);
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
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                foreach (var key in self.windows.Keys.ToList())
                {
                    if (!self.windows[key].Active)
                    {
                        taskScheduler.Add(self.DestroyWindow(key));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 打开窗口 对应 <see cref="AwakeSystem{T}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask<T> OpenWindow<T>(this UIManagerComponent self, string path,
        UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true) where T : Entity, IAwake, IOnCreate, IOnEnable, new()
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName);
            if (target == null)
            {
                target = self.InitWindow<T>(path, layerName);
                self.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await self.InnerOpenWindow<T>(target);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="AwakeSystem{T,P1 }" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask<T> OpenWindow<T, P1>(this UIManagerComponent self, string path, P1 p1,
        UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true) where T : Entity, IAwake, IOnCreate, IOnEnable<P1>, new()
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName);
            if (target == null)
            {
                target = self.InitWindow<T>(path, layerName);
                self.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await self.InnerOpenWindow<T, P1>(target, p1);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="AwakeSystem{T,P1,P2}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask<T> OpenWindow<T, P1, P2>(this UIManagerComponent self, string path, P1 p1, P2 p2,
        UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true) where T : Entity, IAwake, IOnCreate, IOnEnable<P1, P2>, new()
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName);
            if (target == null)
            {
                target = self.InitWindow<T>(path, layerName);
                self.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await self.InnerOpenWindow<T, P1, P2>(target, p1, p2);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="AwakeSystem{T,P1,P2,P3}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask<T> OpenWindow<T, P1, P2, P3>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3,
        UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true) where T : Entity, IAwake, IOnCreate, IOnEnable<P1, P2, P3>, new()
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName);
            if (target == null)
            {
                target = self.InitWindow<T>(path, layerName);
                self.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await self.InnerOpenWindow<T, P1, P2, P3>(target, p1, p2, p3);

        }

        /// <summary>
        /// 打开窗口 对应 <see cref="AwakeSystem{T,P1,P2,P3,P4}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <param name="banKey">是否禁止监听返回键事件</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask<T> OpenWindow<T, P1, P2, P3, P4>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3, P4 p4,
        UILayerNames layerName = UILayerNames.NormalLayer, bool banKey = true) where T : Entity, IAwake, IOnCreate, IOnEnable<P1, P2, P3, P4>, new()
        {

            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName);
            if (target == null)
            {
                target = self.InitWindow<T>(path, layerName);
                self.windows[uiName] = target;
            }

            target.Layer = layerName;
            target.BanKey = banKey;
            return await self.InnerOpenWindow<T, P1, P2, P3, P4>(target, p1, p2, p3, p4);

        }

        /// <summary>
        /// 打开窗口（返回ETTask） 对应 <see cref="AwakeSystem{T}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask OpenWindowTask<T>(this UIManagerComponent self, string path, UILayerNames layerName = UILayerNames.NormalLayer)
                where T : Entity, IAwake, IOnCreate, IOnEnable, new()
        {
            await self.OpenWindow<T>(path, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="AwakeSystem{T,P1}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask OpenWindowTask<T, P1>(this UIManagerComponent self, string path, P1 p1,
        UILayerNames layerName = UILayerNames.NormalLayer) where T : Entity, IAwake, IOnCreate, IOnEnable<P1>, new()
        {
            await self.OpenWindow<T, P1>(path, p1, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="AwakeSystem{T,P1,P2}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask OpenWindowTask<T, P1, P2>(this UIManagerComponent self, string path, P1 p1, P2 p2,
        UILayerNames layerName = UILayerNames.NormalLayer) where T : Entity, IAwake, IOnCreate, IOnEnable<P1, P2>, new()
        {
            await self.OpenWindow<T, P1, P2>(path, p1, p2, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask） 对应 <see cref="AwakeSystem{T,P1,P2,P3}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask OpenWindowTask<T, P1, P2, P3>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3,
        UILayerNames layerName = UILayerNames.NormalLayer) where T : Entity, IAwake, IOnCreate, IOnEnable<P1, P2, P3>, new()
        {
            await self.OpenWindow<T, P1, P2, P3>(path, p1, p2, p3, layerName);
        }

        /// <summary>
        /// 打开窗口（返回ETTask）  对应 <see cref="AwakeSystem{T,P1,P2,P3,P4}" />
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">预制体路径</param>
        /// <param name="layerName">UI层级</param>
        /// <typeparam name="T">要打开的窗口</typeparam>
        /// <returns></returns>
        public static async ETTask OpenWindowTask<T, P1, P2, P3, P4>(this UIManagerComponent self, string path, P1 p1, P2 p2, P3 p3, P4 p4,
        UILayerNames layerName = UILayerNames.NormalLayer) where T : Entity, IAwake, IOnCreate, IOnEnable<P1, P2, P3, P4>, new()
        {
            await self.OpenWindow<T, P1, P2, P3, P4>(path, p1, p2, p3, p4, layerName);
        }

        /// <summary>
        /// 销毁除指定窗口外所有窗口
        /// </summary>
        /// <param name="self"></param>
        /// <param name="typeNames">指定窗口</param>
        public static async ETTask DestroyWindowExceptNames(this UIManagerComponent self, string[] typeNames = null)
        {
            Dictionary<string, bool> dictUINames = new Dictionary<string, bool>();
            if (typeNames != null)
            {
                for (int i = 0; i < typeNames.Length; i++)
                {
                    dictUINames[typeNames[i]] = true;
                }
            }

            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    if (!dictUINames.ContainsKey(keys[i]))
                    {
                        taskScheduler.Add(self.DestroyWindow(keys[i]));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁指定层级外层级所有窗口
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layer">指定层级</param>
        public static async ETTask DestroyWindowExceptLayer(this UIManagerComponent self, UILayerNames layer)
        {
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    if (self.windows[keys[i]].Layer != layer)
                    {
                        taskScheduler.Add(self.DestroyWindow(keys[i]));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁指定层级所有窗口
        /// </summary>
        /// <param name="self"></param>
        /// <param name="layer">指定层级</param>
        public static async ETTask DestroyWindowByLayer(this UIManagerComponent self, UILayerNames layer)
        {
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    if (self.windows[keys[i]].Layer == layer)
                    {
                        taskScheduler.Add(self.DestroyWindow(self.windows[keys[i]].Name));
                    }
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 销毁所有窗体
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask DestroyAllWindow(this UIManagerComponent self)
        {
            var keys = self.windows.Keys.ToArray();
            using (ListComponent<ETTask> taskScheduler = ListComponent<ETTask>.Create())
            {
                for (int i = self.windows.Count - 1; i >= 0; i--)
                {
                    taskScheduler.Add(self.DestroyWindow(self.windows[keys[i]].Name));
                }

                await ETTaskHelper.WaitAll(taskScheduler);
            }
        }

        /// <summary>
        /// 判断窗口是否打开
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsActiveWindow<T>(this UIManagerComponent self) where T : Entity
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName);
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
        static UIWindow InitWindow<T>(this UIManagerComponent self, string path, UILayerNames layerName) where T : Entity, IAwake, new()
        {
            UIWindow window = self.AddChild<UIWindow>();
            var type = TypeInfo<T>.Type;
            window.Name = type.Name;
            window.Active = false;
            window.ViewType = type;
            window.Layer = layerName;
            window.LoadingState = UIWindowLoadingState.NotStart;
            window.PrefabPath = path;
            window.AddComponent<T>();
            return window;
        }

        static void Deactivate(UIWindow target)
        {
            var view = target.GetComponent(target.ViewType);
            if (view != null)
                view.SetActive(false);
        }

        #region 内部加载窗体,依次加载prefab、AwakeSystem、InitializationSystem、OnCreateSystem、OnEnableSystem

        static async ETTask<T> InnerOpenWindow<T>(this UIManagerComponent self, UIWindow target) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }

                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.AddWindowToStack(target);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }

        }

        static async ETTask<T> InnerOpenWindow<T, P1>(this UIManagerComponent self, UIWindow target, P1 p1) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }

                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.AddWindowToStack(target, p1);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        static async ETTask<T> InnerOpenWindow<T, P1, P2>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }

                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.AddWindowToStack(target, p1, p2);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        static async ETTask<T> InnerOpenWindow<T, P1, P2, P3>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3) where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }

                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.AddWindowToStack(target, p1, p2, p3);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }

        static async ETTask<T> InnerOpenWindow<T, P1, P2, P3, P4>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
                where T : Entity
        {
            CoroutineLock coroutineLock = null;
            try
            {
                coroutineLock = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.UIManager, target.GetHashCode());
                target.Active = true;
                T res = target.GetComponent(target.ViewType) as T;
                var needLoad = target.LoadingState == UIWindowLoadingState.NotStart;
                target.LoadingState = UIWindowLoadingState.Loading;
                if (needLoad)
                {
                    await Game.EventSystem.PublishAsync(new UIEventType.InnerOpenWindow() { path = target.PrefabPath, window = target });
                }

                Game.EventSystem.Publish(new UIEventType.ResetWindowLayer() { window = target });
                await self.AddWindowToStack(target, p1, p2, p3, p4);
                target.LoadingState = UIWindowLoadingState.LoadOver;
                return res;
            }
            finally
            {
                coroutineLock?.Dispose();
            }
        }


        #endregion

        /// <summary>
        /// 内部关闭窗体，OnDisableSystem
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        static void InnnerCloseWindow(this UIManagerComponent self, UIWindow target)
        {
            if (target.Active)
            {
                Deactivate(target);
                target.Active = false;
            }
        }

        /// <summary>
        /// 将窗口移到当前层级最上方
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="T"></typeparam>
        public static void MoveWindowToTop<T>(this UIManagerComponent self) where T : Entity
        {
            string uiName = TypeInfo<T>.TypeName;
            var target = self.GetWindow(uiName, 1);
            if (target == null)
            {
                return;
            }

            var layerName = target.Layer;
            if (self.windowStack[layerName].Contains(uiName))
            {
                self.windowStack[layerName].Remove(uiName);
            }

            self.windowStack[layerName].AddFirst(uiName);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
        }

        static async ETTask AddWindowToStack(this UIManagerComponent self, UIWindow target)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (self.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                self.windowStack[layerName].Remove(uiName);
            }

            self.windowStack[layerName].AddFirst(uiName);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        static async ETTask AddWindowToStack<P1>(this UIManagerComponent self, UIWindow target, P1 p1)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (self.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                self.windowStack[layerName].Remove(uiName);
            }

            self.windowStack[layerName].AddFirst(uiName);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        static async ETTask AddWindowToStack<P1, P2>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (self.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                self.windowStack[layerName].Remove(uiName);
            }

            self.windowStack[layerName].AddFirst(uiName);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1, p2);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        static async ETTask AddWindowToStack<P1, P2, P3>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (self.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                self.windowStack[layerName].Remove(uiName);
            }

            self.windowStack[layerName].AddFirst(uiName);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1, p2, p3);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        static async ETTask AddWindowToStack<P1, P2, P3, P4>(this UIManagerComponent self, UIWindow target, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            bool isFirst = true;
            if (self.windowStack[layerName].Contains(uiName))
            {
                isFirst = false;
                self.windowStack[layerName].Remove(uiName);
            }

            self.windowStack[layerName].AddFirst(uiName);
            Game.EventSystem.Publish(new UIEventType.AddWindowToStack() { window = target });
            var view = target.GetComponent(target.ViewType);
            view.SetActive(true, p1, p2, p3, p4);
            if (isFirst && (layerName == UILayerNames.BackgroudLayer || layerName == UILayerNames.GameBackgroudLayer))
            {
                //如果是背景layer，则销毁所有的normal层|BackgroudLayer
                await self.CloseWindowByLayer(UILayerNames.NormalLayer);
                await self.CloseWindowByLayer(UILayerNames.GameLayer);
                await self.CloseWindowByLayer(UILayerNames.BackgroudLayer, uiName);
                await self.CloseWindowByLayer(UILayerNames.GameBackgroudLayer, uiName);
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        static void RemoveFromStack(this UIManagerComponent self, UIWindow target)
        {
            var uiName = target.Name;
            var layerName = target.Layer;
            if (self.windowStack.ContainsKey(layerName))
            {
                self.windowStack[layerName].Remove(uiName);
            }
            else
            {
                Log.Error("not layer, name :" + layerName);
            }
        }

        #endregion

        #region 屏幕适配

        /// <summary>
        /// 修改边缘宽度
        /// </summary>
        /// <param name="self"></param>
        /// <param name="value"></param>
        public static void SetWidthPadding(this UIManagerComponent self, float value)
        {
            self.WidthPadding = value;
            foreach (var layer in self.windowStack.Values)
            {
                if (layer != null)
                {
                    for (LinkedListNode<string> node = layer.First; null != node; node = node.Next)
                    {
                        var target = self.GetWindow(node.Value);
                        var win = target.GetComponent(target.ViewType) as IOnWidthPaddingChange;
                        if (win != null)
                        {
                            EventSystem.Instance.Publish(new UIEventType.OnWidthPaddingChange { entity = win as Entity });
                        }
                    }
                }
            }
        }

        #endregion
    }
}
