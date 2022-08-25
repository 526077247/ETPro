using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET
{
    [ObjectSystem]
    public class FSMComponentDestroySystem : DestroySystem<FSMComponent>
    {
        public override void Destroy(FSMComponent self)
        {
            self.RemoveAllState().Coroutine();
        }
    }
    [FriendClass(typeof(FSMComponent))]
    public static class FSMComponentSystem
    {
        /// <summary>
        /// 添加指定的状态
        /// </summary>
        /// <typeparam name="State"></typeparam>
        /// <param name="self"></param>
        public static void AddState<State>(this FSMComponent self) where State : Entity,IAwake
        {
            Entity _tmpState = self.GetState<State>();
            if (_tmpState == null)
            {
                var state = self.AddChild<State>();
                self.m_dic.Add(typeof(State), state);
            }
            else
            {
                Log.Warning("FSMSystem(容错)：该状态【{0}】已经被添加！", typeof(State).Name);
            }
        }
        /// <summary>
        /// 添加指定的状态
        /// </summary>
        /// <typeparam name="State"></typeparam>
        /// <param name="self"></param>
        public static void AddState<State, T>(this FSMComponent self, T t) where State : Entity,IAwake<T>
        {
            Entity _tmpState = self.GetState<State>();
            if (_tmpState == null)
            {
                var state = self.AddChild<State,T>(t);
                self.m_dic.Add(typeof(State), state);
            }
            else
            {
                Log.Warning("FSMSystem(容错)：该状态【{0}】已经被添加！", typeof(State).Name);
            }
        }
        /// <summary>
        /// 添加指定的状态
        /// </summary>
        /// <typeparam name="State"></typeparam>
        /// <param name="self"></param>
        public static void AddState<State, T, U>(this FSMComponent self, T t, U u) where State : Entity,IAwake<T,U>
        {
            Entity _tmpState = self.GetState<State>();
            if (_tmpState == null)
            {
                var state = self.AddChild<State,T,U>(t,u);
                self.m_dic.Add(typeof(State), state);
            }
            else
            {
                Log.Warning("FSMSystem(容错)：该状态【{0}】已经被添加！", typeof(State).Name);
            }
        }
        /// <summary>
        /// 添加指定的状态
        /// </summary>
        /// <typeparam name="State"></typeparam>
        /// <param name="self"></param>
        public static void AddState<State, T, U,V>(this FSMComponent self, T t, U u,V v) where State : Entity,IAwake<T,U,V>
        {
            Entity _tmpState = self.GetState<State>();
            if (_tmpState == null)
            {
                var state = self.AddChild<State,T,U,V>(t,u,v);
                self.m_dic.Add(typeof(State), state);
            }
            else
            {
                Log.Warning("FSMSystem(容错)：该状态【{0}】已经被添加！", typeof(State).Name);
            }
        }
        /// <summary>
        /// 删除状态
        /// </summary>
        /// <typeparam name="State"></typeparam>
        /// <param name="self"></param>
        /// <param name="_state"></param>
        public static void RemoveState<State>(this FSMComponent self) where State : Entity
        {
            Entity _tmpState = self.GetState<State>();
            if (_tmpState != null)
            {
                if (self.CurrentState == _tmpState)
                {
                    Log.Warning("FSMSystem(容错)：该状态【{0}】正在进行！", typeof(State).Name);
                }
                else
                {
                    self.m_dic.Remove(typeof(State));
                    _tmpState.Dispose();
                }
            }
            else
            {
                Log.Warning("FSMSystem(容错)：该状态【{0}】已经被移除！", typeof(State).Name);
            }
        }
        public static Entity GetState<State>(this FSMComponent self) where State : Entity
        {
            if (self.m_dic.TryGetValue(typeof(State), out var res))
            {
                return res;
            }
            return null;
        }

        /// <summary>
        /// 状态机状态翻转
        /// </summary>
        /// <param name="state">指定状态机</param>
        /// <returns>执行结果</returns>
        public static async ETTask ChangeState<State>(this FSMComponent self) where State : Entity
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.FSM, self.Id);
                Entity _tmpState = self.GetState<State>();        //要改变的状态不存在
                if (_tmpState == null)
                {
                    Log.Warning("FSMSystem(容错)：该状态【{0}】不存在于状态机中！", typeof(State).Name);
                }
                if (self.CurrentState != null) //当前状态不为空
                {
                    await FSMWatcherComponent.Instance.FSMOnExit(self.CurrentState);
                }
                self.CurrentState = _tmpState; //缓存为当前状态
                await FSMWatcherComponent.Instance.FSMOnEnter(self.CurrentState);//触发当前状态的OnEnter
            }
            finally
            {
                coroutine?.Dispose();
            }
        }
        /// <summary>
        /// 状态机状态翻转
        /// </summary>
        /// <param name="state">指定状态机</param>
        /// <returns>执行结果</returns>
        public static async ETTask ChangeState<State, T>(this FSMComponent self, T t) where State : Entity
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.FSM, self.Id);
                Entity _tmpState = self.GetState<State>();        //要改变的状态不存在
                if (_tmpState == null)
                {
                    Log.Warning("FSMSystem(容错)：该状态【{0}】不存在于状态机中！", typeof(State).Name);
                }
                if (self.CurrentState != null) //当前状态不为空
                {
                    await FSMWatcherComponent.Instance.FSMOnExit(self.CurrentState);
                }
                self.CurrentState = _tmpState; //缓存为当前状态
                await FSMWatcherComponent.Instance.FSMOnEnter(self.CurrentState, t);//触发当前状态的OnEnter
            }
            finally
            {
                coroutine?.Dispose();
            }
        }
        /// <summary>
        /// 状态机状态翻转
        /// </summary>
        /// <param name="state">指定状态机</param>
        /// <returns>执行结果</returns>
        public static async ETTask ChangeState<State, T, U>(this FSMComponent self, T t, U u) where State : Entity
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.FSM, self.Id);
                Entity _tmpState = self.GetState<State>();        //要改变的状态不存在
                if (_tmpState == null)
                {
                    Log.Warning("FSMSystem(容错)：该状态【{0}】不存在于状态机中！", typeof(State).Name);
                }
                if (self.CurrentState != null) //当前状态不为空
                {
                    await FSMWatcherComponent.Instance.FSMOnExit(self.CurrentState);
                }
                self.CurrentState = _tmpState; //缓存为当前状态
                await FSMWatcherComponent.Instance.FSMOnEnter(self.CurrentState, t, u);//触发当前状态的OnEnter
            }
            finally
            {
                coroutine?.Dispose();
            }
        }
        /// <summary>
        /// 状态机状态翻转
        /// </summary>
        /// <param name="state">指定状态机</param>
        /// <returns>执行结果</returns>
        public static async ETTask ChangeState<State, T, U, V>(this FSMComponent self, T t, U u, V v) where State : Entity
        {
            CoroutineLock coroutine = null;
            try
            {
                coroutine = await CoroutineLockComponent.Instance.Wait(CoroutineLockType.FSM, self.Id);
                Entity _tmpState = self.GetState<State>();       //要改变的状态不存在
                if (_tmpState == null)
                {
                    Log.Warning("FSMSystem(容错)：该状态【{0}】不存在于状态机中！", typeof(State).Name);
                }
                if (self.CurrentState != null) //当前状态不为空
                {
                    await FSMWatcherComponent.Instance.FSMOnExit(self.CurrentState);
                }
                self.CurrentState = _tmpState; //缓存为当前状态
                await FSMWatcherComponent.Instance.FSMOnEnter(self.CurrentState, t, u, v);//触发当前状态的OnEnter
            }
            finally
            {
                coroutine?.Dispose();
            }
        }
        public static async ETTask RemoveAllState(this FSMComponent self) //移除所有状态
        {
            if (self.CurrentState != null)
            {
                await FSMWatcherComponent.Instance.FSMOnExit(self.CurrentState);
                self.CurrentState = null;
            }
            self.m_dic.Clear();
        }
    }
}
