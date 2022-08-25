using System;
namespace ET
{
    [ObjectSystem]
    public class ZhuiZhuAimComponentAwakeSystem:AwakeSystem<ZhuiZhuAimComponent,Unit,Action>
    {
        public override void Awake(ZhuiZhuAimComponent self,Unit a, Action b)
        {
            self.Aim = a;
            self.OnArrived = b;
        }
    }

    [ObjectSystem]
    [FriendClass(typeof(ZhuiZhuAimComponent))]
    public static class ZhuiZhuAimComponentSystem
    {
        public static void Arrived(this ZhuiZhuAimComponent self)
        {
            self.Aim = null;
            self.OnArrived?.Invoke();
            self.OnArrived = null;
        }
    }
}