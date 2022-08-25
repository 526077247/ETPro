using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [ObjectSystem]
    public class RedDotComponentAwakeSystem: AwakeSystem<RedDotComponent>
    {
        public override void Awake(RedDotComponent self)
        {
            RedDotComponent.Instance = self;
            foreach (var item in RedDotConfigCategory.Instance.GetAll())
            {
                self.AddRodDotNode(item.Value.Parent,item.Value.Target);
            }
        }
    }

    [ObjectSystem]
    public class RedDotComponentDestroySystem: DestroySystem<RedDotComponent>
    {
        public override void Destroy(RedDotComponent self)
        {
            foreach (var List in self.RedDotNodeParentsDict.Values)
            {
                List.Dispose();
            }
            self.RedDotNodeParentsDict.Clear();
            self.ToParentDict.Clear();
            self.RedDotMonoViewDict.Clear();
            self.RetainViewCount.Clear();
            RedDotComponent.Instance = null;
        }
    }
    [FriendClass(typeof(RedDotComponent))]
    public static class RedDotComponentSystem
    {
        /// <summary>
        /// 创建树————添加节点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        public static void AddRodDotNode(this RedDotComponent self, string parent, string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Log.Error($"target is null");
                return;
            }
            
            if (string.IsNullOrEmpty(parent))
            {
                Log.Error($"parent is null");
                return;
            }

            if (self.ToParentDict.ContainsKey(target))
            {
                Log.Error($"{target} is already exist!");
                return;
            }

            self.ToParentDict.Add(target, parent);

            if (!self.RetainViewCount.ContainsKey(target))
            {
                self.RetainViewCount.Add(target, 0);
            }

            if (self.RedDotNodeParentsDict.TryGetValue(parent, out ListComponent<string> list))
            {
                list.Add(target);
                return;
            }

            var listComponent = ListComponent<string>.Create();
            listComponent.Add(target);
            self.RedDotNodeParentsDict.Add(parent, listComponent);
        }
        /// <summary>
        /// 创建树————移除节点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        public static void RemoveRedDotNode(this RedDotComponent self, string target)
        {
            if (!self.ToParentDict.TryGetValue(target, out string parent))
            {
                return ;
            }

            if (!self.IsLeafNode(target))
            {
                Log.Error("can not remove parent node!");
                return ;
            }
            
            self.ToParentDict.Remove(target);
            if (!string.IsNullOrEmpty(parent))
            {
                self.RedDotNodeParentsDict[parent].Remove(target);
                if ( self.RedDotNodeParentsDict[parent].Count <= 0 )
                {
                    self.RedDotNodeParentsDict[parent].Dispose();
                    self.RedDotNodeParentsDict.Remove(parent);
                }
            }
        }

        /// <summary>
        /// 添加UI红点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="uiRedDotComponent"></param>
        public static void AddUIRedDotComponent(this RedDotComponent self, string target, Entity uiRedDotComponent)
        {
            self.RedDotMonoViewDict[target] = uiRedDotComponent;
        }
        /// <summary>
        /// 移除UI红点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="uiRedDotComponent"></param>
        public static void RemoveUIRedDotComponent(this RedDotComponent self, string target,out Entity uiRedDotComponent)
        {
            if (self.RedDotMonoViewDict.TryGetValue(target, out uiRedDotComponent))
            {
                self.RedDotMonoViewDict.Remove(target);
            }
            
        }
        /// <summary>
        /// 是否是子节点
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsLeafNode(this RedDotComponent self, string target)
        {
            return !self.RedDotNodeParentsDict.ContainsKey(target);
        }
        /// <summary>
        /// 刷新红点数量
        /// </summary>
        /// <param name="self"></param>
        /// <param name="target"></param>
        /// <param name="Count"></param>
        public static void RefreshRedDotViewCount(this RedDotComponent self, string target, int Count)
        {
            if (!self.IsLeafNode(target))
            {
                Log.Error("can not refresh parent node view count");
                return;
            }
            
            self.RedDotMonoViewDict.TryGetValue(target, out Entity uiRedDotComponent);

            self.RetainViewCount[target] = Count;

            if (uiRedDotComponent != null)
            {
                UIWatcherComponent.Instance.OnChangeRedDotActive(uiRedDotComponent, self.RetainViewCount[target]);
            }
            
            bool isParentExist = self.ToParentDict.TryGetValue(target, out string parent);

            while (isParentExist)
            {
                var viewCount = 0;
                
                foreach (var childNode in self.RedDotNodeParentsDict[parent])
                {
                    viewCount += self.RetainViewCount[childNode];
                }

                self.RetainViewCount[parent] = viewCount;
                
                if (self.RedDotMonoViewDict.TryGetValue(parent, out uiRedDotComponent))
                {
                    UIWatcherComponent.Instance.OnChangeRedDotActive(uiRedDotComponent, viewCount);
                }

                isParentExist = self.ToParentDict.TryGetValue(parent, out parent);
            }
        }
        
    }
}