using UnityEngine;
namespace ET
{
    [ActorMessageHandler]
    public class M2M_UseSkillHandler : AMActorLocationHandler<Scene, M2M_UseSkill>
    {
        protected override async ETTask Run(Scene scene, M2M_UseSkill message)
        {
            Unit unit = null;
            var uc = scene.GetComponent<UnitComponent>();
            if (uc != null)
            {
                unit = uc.Get(message.Sender);
                if (unit == null)
                {
                    return;
                }
            }
            var combatU = unit.GetComponent<CombatUnitComponent>();
            if (combatU != null&&combatU.TryGetSkillAbility(message.SkillConfigId,out SkillAbility skill))
            {
                if (skill.SkillConfig.PreviewType == SkillPreviewType.SelectCircularInCircularArea)
                {
                    combatU.GetComponent<SpellComponent>().SpellWithPoint(skill,new Vector3(message.X,message.Y,message.Z));
                }
                else if (skill.SkillConfig.PreviewType == SkillPreviewType.SelectTarget)
                {
                    var aim = unit.GetParent<UnitComponent>().Get(message.Reciver);
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
            await ETTask.CompletedTask;
        }
    }
}