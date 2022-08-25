using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [FriendClass(typeof(GhostComponent))]
    public static class GhostComponentSystem
    {
        public class AwakeSystem:AwakeSystem<GhostComponent>
        {
            public override void Awake(GhostComponent self)
            {
                self.AreaIds = new Dictionary<int, int>();
                self.IsGoast = false;
            }
        }

        public class DestroySystem:DestroySystem<GhostComponent>
        {
            public override void Destroy(GhostComponent self)
            {
                if (!self.IsGoast)
                {
                    foreach (var item in self.AreaIds)
                    {
                        Log.Info(self.DomainScene().Id+"   "+ item.Key);
                        var scene = StartSceneConfigCategory.Instance.Get(item.Key);
                        if (scene.InstanceId != self.DomainScene().InstanceId)
                            ActorMessageSenderComponent.Instance.Send(scene.InstanceId, new M2M_UnitAreaRemove() { UnitId = self.Id });
                    }
                }
                self.AreaIds.Clear();
                self.AreaIds = null;
            }
        }
        public static void AddListenerAreaIds(this GhostComponent self, int sceneId)
        {
            if(!self.AreaIds.ContainsKey(sceneId))
            {
                self.AreaIds.Add(sceneId,1);
                if (!self.IsGoast)
                {
                    var scene = StartSceneConfigCategory.Instance.Get(sceneId);
                    if (scene.InstanceId != self.DomainScene().InstanceId)
                        TransferHelper.AreaAdd(self.Parent.GetParent<Unit>(), scene.InstanceId);
                }
            }
            else
            {
                self.AreaIds[sceneId]++;
            }
        }
        
        public static void RemoveListenerAreaIds(this GhostComponent self, int sceneId)
        {
            if(!self.AreaIds.ContainsKey(sceneId))
            {
                self.AreaIds.Add(sceneId,-1);
                Log.Error("移除不存在的监听");
            }
            else
            {
                self.AreaIds[sceneId]--;
                
            }
            if (self.AreaIds[sceneId] <= 0)
            {
                if (!self.IsGoast)
                {
                    var scene = StartSceneConfigCategory.Instance.Get(sceneId);
                    if (scene.InstanceId != self.DomainScene().InstanceId)
                        ActorMessageSenderComponent.Instance.Send(scene.InstanceId, new M2M_UnitAreaRemove() { UnitId = self.Id });
                }

                self.AreaIds.Remove(sceneId);
            }
        }

        public static async ETTask AreaTransfer(this GhostComponent self, int newSceneId,Vector3 pos)
        {
            if (!self.IsGoast)
            {
                if (newSceneId != self.RealAreaId)
                {
                    if (self.LeavePos != null)//超过2m才传送，防止在边缘反复横跳
                    {
                        if (Vector3.SqrMagnitude((Vector3) self.LeavePos, pos) > 16)
                        {
                            await TransferHelper.AreaTransfer(self.GetParent<AOIUnitComponent>(),
                                StartSceneConfigCategory.Instance.Get(newSceneId).InstanceId);
                        }
                    }
                    else
                    {
                        self.LeavePos = pos;
                    }
                }
                else
                {
                    self.LeavePos = null;
                }
            }
        }

        #region 需要同步的协议

        /// <summary>
        /// 处理转发协议
        /// </summary>
        /// <param name="self"></param>
        /// <param name="msg"></param>
        /// <typeparam name="T"></typeparam>
        public static void HandleMsg<T>(this GhostComponent self, T msg) where T: IActorMessage
        {
            var type = msg.GetType();
            if (self.AreaIds.Count>1&&GhostComponent.MsgMap.TryGetValue(type,out var newType))
            {
                var newMsg = Activator.CreateInstance(newType) as IActorRequest;
                var props = type.GetProperties();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = newType.GetProperty(props[i].Name);
                    if (prop != null)
                    {
                        var val = props[i].GetValue(msg);
                        prop.SetValue(newMsg,val);
                    }
                }
                var fields = type.GetFields();
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = newType.GetField(fields[i].Name);
                    if (field != null)
                    {
                        var val = fields[i].GetValue(msg);
                        field.SetValue(newMsg,val);
                    }
                }
                var id = self.DomainScene().InstanceId;
                foreach (var item in self.AreaIds)
                {
                    var scene = StartSceneConfigCategory.Instance.Get(item.Key);
                    if (id != scene.InstanceId)
                    {
                        ActorMessageSenderComponent.Instance.Send(scene.InstanceId, newMsg);
                    }
                }
            }
        }

        #endregion
    }
}