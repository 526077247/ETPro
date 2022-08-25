namespace ET
{
    /// <summary>
    /// 监视hp数值变化，改变血条值
    /// </summary>
    [NumericWatcher(NumericType.Hp)]
    public class NumericWatcher_Hp_ShowUI : INumericWatcher
    {
        public void Run(EventType.NumbericChange args)
        {
            Unit unit = args.Parent as Unit;
            if (unit == null)
            {
                Log.Error("监视hp数值变化，改变血条值 Unit 未找到 ！");
                return;
            }

            InfoComponent ic = unit.GetComponent<InfoComponent>();
            ic?.RefreshUI();
        }
    }
}