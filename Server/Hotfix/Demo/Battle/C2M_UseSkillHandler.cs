using UnityEngine;

namespace ET
{
    [ActorMessageHandler]
    [FriendClass(typeof(CombatUnitComponent))]
    public class C2M_UseSkillHandler : AMActorLocationHandler<Unit, C2M_UseSkill>
    {
        protected override async ETTask Run(Unit unit, C2M_UseSkill message)
        {
            var combatU = unit.GetComponent<CombatUnitComponent>();
            if (combatU != null&&combatU.TryGetSkillAbility(message.SkillConfigId,out SkillAbility skill))
            {
                if (skill.SkillConfig.PreviewType == SkillPreviewType.SelectCircularInCircularArea)
                {
                    combatU.GetComponent<SpellComponent>().SpellWithPoint(skill,new Vector3(message.X,message.Y,message.Z));
                }
                else if (skill.SkillConfig.PreviewType == SkillPreviewType.SelectTarget)
                {
                    var aim = unit.GetParent<UnitComponent>().Get(message.Id);
                    if (aim != null)
                    {
                        message.X = aim.Position.x;
                        message.Y = aim.Position.y;
                        message.Z = aim.Position.z;
                        combatU.GetComponent<SpellComponent>().SpellWithTarget(skill,aim.GetComponent<CombatUnitComponent>());
                    }
                    else
                    {
                        Log.Error("目标不存在");
                        return;
                    }
                }
                else if (skill.SkillConfig.PreviewType == SkillPreviewType.SelectRectangleArea)
                {
                    combatU.GetComponent<SpellComponent>().SpellWithDirect(skill,new Vector3(message.X,message.Y,message.Z));
                }
                else
                {
                    Log.Error("未处理的类型"+skill.SkillConfig.PreviewType);
                }
            }
            else
            {
                Log.Error("技能不存在");
                return;
            }

            M2C_UseSkill info = new M2C_UseSkill()
            {
                Sender = unit.Id,
                Reciver = message.Id,
                X = message.X,
                Y = message.Y,
                Z = message.Z,
                SkillConfigId = message.SkillConfigId
            };
            MessageHelper.Broadcast(unit,info);
            await ETTask.CompletedTask;
        }
    }
}