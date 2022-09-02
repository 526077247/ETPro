using System;
using System.Collections.Generic;

namespace ET
{
    public static class Game
    {
        public static ThreadSynchronizationContext ThreadSynchronizationContext => ThreadSynchronizationContext.Instance;

        public static TimeInfo TimeInfo => TimeInfo.Instance;
        
        public static EventSystem EventSystem => EventSystem.Instance;

        private static Scene scene;
        public static Scene Scene
        {
            get
            {
                if (scene != null)
                {
                    return scene;
                }
                scene = EntitySceneFactory.CreateScene(IdGenerater.Instance.GenerateInstanceId(), 0, SceneType.Process, "Process");
                return scene;
            }
        }

        public static ObjectPool ObjectPool => ObjectPool.Instance;

        public static IdGenerater IdGenerater => IdGenerater.Instance;

        public static Options Options => Options.Instance;

        private static Queue<ETTask> frameFinishTask = new Queue<ETTask>();
        private static Queue<ETTask> frameFinishTask2 = new Queue<ETTask>();
        public static async ETTask WaitFrameFinish()
        {
            ETTask task = ETTask.Create(true);
            frameFinishTask.Enqueue(task);
            await task;
        }
        
        public static void Update()
        {
            ThreadSynchronizationContext.Update();
            TimeInfo.Update();
            EventSystem.Update();
        }
        
        public static void LateUpdate()
        {
            EventSystem.LateUpdate();
        }
        
        public static void FrameFinishUpdate()
        {
            ObjectHelper.Swap(ref frameFinishTask2,ref frameFinishTask);
            while (frameFinishTask2.Count > 0)
            {
                ETTask task = frameFinishTask2.Dequeue();
                task.SetResult();
            }
        }
        
        public static void Close()
        {
            scene?.Dispose();
            scene = null;
            MonoPool.Instance.Dispose();
            EventSystem.Instance.Dispose();
            IdGenerater.Instance.Dispose();
        }
    }
}