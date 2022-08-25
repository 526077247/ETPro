using System;
namespace ET
{
    public class DamageInfo :IDisposable
    {
        public float Value;
        
        
        public static DamageInfo Create()
        {
            return MonoPool.Instance.Fetch(typeof (DamageInfo)) as DamageInfo;
        }

        public void Dispose()
        {
            Value = 0;
            MonoPool.Instance.Recycle(this);
        }
    }
}