using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cozyheim.LevelingSystem
{
    internal class SkillElementalDamage : SkillBase
    {
        public static SkillElementalDamage Instance;

        public SkillElementalDamage(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
        {
            skillType = SkillType.ElementalDamage;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "ApplyDamage")]
            private static void Character_ElementalDamage_Prefix(Character __instance, ref HitData hit)
            {
                if (Instance == null)
                {
                    return;
                }

                if (hit.HaveAttacker())
                {
                    if (__instance.m_faction != Character.Faction.Players && hit.GetAttacker().m_faction == Character.Faction.Players)
                    {
                        float multiplier = 1 + ((Instance.level * Instance.bonusPerLevel) / 100);
                        hit.m_damage.m_fire *= multiplier;
                        hit.m_damage.m_lightning *= multiplier;
                        hit.m_damage.m_frost *= multiplier;
                        hit.m_damage.m_poison *= multiplier;
                        hit.m_damage.m_spirit *= multiplier;
                    }
                }
            }
        }
    }
}
