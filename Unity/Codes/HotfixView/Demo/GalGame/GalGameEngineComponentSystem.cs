using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class GalGameEngineComponentAwakeSystem : AwakeSystem<GalGameEngineComponent>
    {
        public override void Awake(GalGameEngineComponent self)
        {
            GalGameEngineComponent.Instance = self;
            self.CancelToken = new ETCancellationToken();
            self.Speed = GalGameEngineComponent.NormalSpeed;
            self.StageRoleMap = new Dictionary<string, string>();
            self.RoleExpressionMap = new Dictionary<string, string>();
            self.RoleExpressionPathMap = new Dictionary<string, Dictionary<string, string>>();
            self.WaitInput = ETTask<KeyCode>.Create();
            self.ReviewItems = new List<GalGameEngineComponent.ReviewItem>();
            for(int i=0;i<RoleExpressionConfigCategory.Instance.GetAllList().Count;i++)
            {
                var item = RoleExpressionConfigCategory.Instance.GetAllList()[i];
                if (!self.RoleExpressionPathMap.ContainsKey(item.NameKey))
                    self.RoleExpressionPathMap.Add(item.NameKey, new Dictionary<string, string>());
                self.RoleExpressionPathMap[item.NameKey].Add(item.Expression, item.Path);
            }
            self.StagePosMap = new Dictionary<string, Vector3>();
            for(int i=0;i<StagePosConfigCategory.Instance.GetAllList().Count;i++)
            {
                var item = StagePosConfigCategory.Instance.GetAllList()[i];
                self.StagePosMap.Add(item.NameKey, new Vector3(item.Position[0], item.Position[1], 0));
            }
            self.FSM = self.AddComponent<FSMComponent>();
            self.FSM.AddState<GalGameEngineReadyState>();
            self.FSM.AddState<GalGameEngineFastForwardState>();
            self.FSM.AddState<GalGameEngineRunningState>();
            self.FSM.AddState<GalGameEngineSuspendedState>();
            self.FSM.ChangeState<GalGameEngineReadyState>().Coroutine();
        }
    }
    [FriendClass(typeof(GalGameEngineComponent))]
    public static class GalGameEngineComponentSystem
    {
        /// <summary>
        /// 加载配置表
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ChapterCategory GetChapterByName(this GalGameEngineComponent self, string name)
        {
            ChapterCategory res = ConfigComponent.Instance.LoadOneConfig<ChapterCategory>("Config/"+name+"Category.bytes");
            if(res == null)
                Log.Error("加载配置表 " + name + "失败");

            return res;
        }
        /// <summary>
        /// 开始播放一段剧情
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async ETTask<bool> PlayChapterByName(this GalGameEngineComponent self, string name,Action<bool> onPlayOver = null)
        {
            if (self.State != GalGameEngineComponent.GalGameEngineState.Ready) return false;
            ChapterCategory category = GetChapterByName(self, name);
            if (category == null) return false;
            self.CurCategory = category;
            self.CurCategory.Order();
            self.StageRoleMap.Clear();
            self.RoleExpressionMap.Clear();
            self.Index = 0;
            self.OnPlayOver = onPlayOver;
            UIManagerComponent.Instance.OpenWindow<UIGalGameHelper>(UIGalGameHelper.PrefabPath).Coroutine();
            await self.FSM.ChangeState<GalGameEngineRunningState>();
            return true;
        }
        /// <summary>
        /// 切换快进状态
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask<bool> ChangePlayFastModel(this GalGameEngineComponent self)
        {
            if (self.State == GalGameEngineComponent.GalGameEngineState.FastForward)
            {
                await self.FSM.ChangeState<GalGameEngineRunningState>();
            }
            else if (self.State == GalGameEngineComponent.GalGameEngineState.Running)
            {
                await self.FSM.ChangeState<GalGameEngineFastForwardState>();
            }
            else
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 播放结束
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static async ETTask PlayOver(this GalGameEngineComponent self)
        {
            Log.Debug("PlayOver");
            await self.FSM.ChangeState<GalGameEngineReadyState,bool>(true);
        }
        /// <summary>
        /// 整理章节内容
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static void Order(this ChapterCategory self)
        {
            if (self.IsOrdered) return;
            var dict = self.GetAll();
            var offset = 0;
            for (int i = 0; i < dict.Count; i++)
            {
                var last_index = i + offset - 1;
                if (!dict.ContainsKey(i))
                {
                    if (!dict.ContainsKey(last_index))
                    {
                        offset--;
                    }
                    else
                    {
                        dict[i] = dict[last_index].Clone() as Chapter;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dict[i].Command))
                    {
                        dict[i].Command = "ShowMessageWindow";//默认改为对话框
                        if (string.IsNullOrEmpty(dict[i].Arg1))
                            dict[i].Arg1 = dict[last_index].Arg1;
                    }
                    dict[i + offset] = dict[i];
                }
            }
            var keys = dict.Keys.ToList();
            keys.Sort();
            for (int i = 0; i <-offset; i++)
            {
                var index = dict.Count - 1 - i;
                dict.Remove(keys[index]);
            }
            self.IsOrdered = true;
        }

        /// <summary>
        /// 打开对话框
        /// </summary>
        /// <param name="self"></param>
        /// <param name="text"></param>
        /// <param name="name"></param>
        /// <param name="windowType"></param>
        /// <param name="pageCtrl"></param>
        /// <param name="voice"></param>
        /// <returns></returns>
        public static async ETTask ShowMessage(this GalGameEngineComponent self,string text,string name , string windowType , string pageCtrl,string voice,long waitTime=1000)
        {
            if (string.IsNullOrEmpty(windowType)) windowType = "MessageWindow";
            if (string.IsNullOrEmpty(pageCtrl)) pageCtrl = "Page";
            bool refresh = pageCtrl.IndexOf("page",StringComparison.OrdinalIgnoreCase)>=0;
            if (pageCtrl.IndexOf("br",StringComparison.OrdinalIgnoreCase)>=0) text += "\n";
            await UIManagerComponent.Instance.CloseWindow<UIMessageWindow>();
            await UIManagerComponent.Instance.CloseWindow<UIBaseMessageWindow>();
            if (windowType.Equals("messagewindow",StringComparison.OrdinalIgnoreCase))
            {
                UIMessageWindow win = await UIManagerComponent.Instance.OpenWindow<UIMessageWindow, float, long>(UIMessageWindow.PrefabPath,self.Speed, waitTime);
                self.AppendReviewItem(name, text, !refresh);
                win.SetName(name);
                await win.SetContent(text,clear: refresh);
            }
            else if (windowType.Equals("messagewindowfull",StringComparison.OrdinalIgnoreCase))
            {
                var win = await UIManagerComponent.Instance.OpenWindow<UIBaseMessageWindow, float, long>(UIBaseMessageWindow.UIMessageWindowFull,self.Speed, waitTime);
                await win.SetContent(text,clear: refresh);
            }
            else if (windowType.Equals("messagewindowmiddle",StringComparison.OrdinalIgnoreCase))
            {
                var win = await UIManagerComponent.Instance.OpenWindow<UIBaseMessageWindow, float, long>(UIBaseMessageWindow.UIMessageWindowMiddle,self.Speed, waitTime);
                await win.SetContent(text,clear: refresh);
            }
            else
                return;
            
           

        }

        public static void AppendReviewItem(this GalGameEngineComponent self, string name,string content, bool isContinue = false)
        {
            if (string.IsNullOrEmpty(content)) return;
            if (self.ReviewItems.Count<=0 || !self.ReviewItems[self.ReviewItems.Count - 1].Continue)
            {
                self.ReviewItems.Add(new GalGameEngineComponent.ReviewItem
                {
                    Name = name,
                    Content = content,
                    Continue = isContinue,
                });
            }
            else
            {
                self.ReviewItems[self.ReviewItems.Count - 1].Content +=","+ content;
                self.ReviewItems[self.ReviewItems.Count - 1].Continue = isContinue;
            }
        }
    }



}
