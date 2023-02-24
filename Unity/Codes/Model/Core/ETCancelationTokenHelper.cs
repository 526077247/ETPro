namespace ET
{
    public static class ETCancelationTokenHelper
    {
        public static async ETTask CancelAfter(this ETCancellationToken self, long afterTimeCancel)
        {
            if (self.IsDispose())
            {
                return;
            }

            await TimerComponent.Instance.WaitAsync(afterTimeCancel);
            
            if (self.IsDispose())
            {
                return;
            }
            
            self.Cancel();
        }
    }
}