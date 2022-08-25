namespace ET
{
    // 添加BUFF
    [Event]
    [FriendClass(typeof(BuffComponent))]
    [FriendClass(typeof(Buff))]
    public class AfterAddBuff_Broadcast: AEvent<EventType.AfterAddBuff>
    {
        protected override void Run(EventType.AfterAddBuff args)
        {
            var unit = args.Buff.GetParent<BuffComponent>().unit;
            M2C_AddBuff msg = new M2C_AddBuff { ConfigId = args.Buff.ConfigId, Timestamp = args.Buff.Timestamp, UnitId = unit.Id, };
            MessageHelper.Broadcast(unit,msg);
        }
    }
}