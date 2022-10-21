namespace ET
{
    [Timer(TimerType.RemoveBuff)]
    public class RemoveBuff: ATimer<Buff>
    {
        public override void Run(Buff self)
        {
            try
            {
                if(self==null||self.IsDisposed) return;
                self.GetParent<BuffComponent>().Remove(self.Id,true);
            }
            catch (System.Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }

    [FriendClass(typeof(BuffComponent))]
    [ObjectSystem]
    public class BuffAwakeSystem : AwakeSystem<Buff,int,long,long>
    {
        public override void Awake(Buff self,int id,long timestamp,long sourceId)
        {
            Log.Info("添加BUFF id="+id);
            self.ConfigId = id;
            self.Timestamp = timestamp;
            self.FromUnitId = sourceId;
            if(timestamp>=0)
                self.TimerId = TimerComponent.Instance.NewOnceTimer(timestamp, TimerType.RemoveBuff, self);
        }
    }

    [FriendClass(typeof(BuffComponent))]
    [ObjectSystem]
    public class BuffDestroySystem : DestroySystem<Buff>
    {
        public override void Destroy(Buff self)
        {
            TimerComponent.Instance.Remove(ref self.TimerId);
            Log.Info("移除BUFF id="+self.ConfigId);
        }
    }

    [FriendClass(typeof(Buff))]
    [FriendClass(typeof(BuffComponent))]
    public static class BuffSystem
    {

    }
}