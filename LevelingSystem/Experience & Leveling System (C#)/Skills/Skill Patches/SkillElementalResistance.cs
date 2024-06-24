using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cozyheim.LevelingSystem
{
    internal class SkillElementalResistance : SkillBase
    {
        public static SkillElementalResistance Instance;

        public SkillElementalResistance(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
        {
            skillType = SkillType.ElementalResistance;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "ApplyDamage")]
            private static void Character_ElementalResistance_Prefix(Character __instance, ref HitData hit)
            {
                if (Instance == null)
                {
                    return;
                }

                if (hit.HaveAttacker())
                {
                    if (__instance.m_faction == Character.Faction.Players)
                    {
                        float multiplier = 1 - ((Instance.level * Instance.bonusPerLevel) / 100);
                        hit.m_damage.m_lightning *= multiplier;
                        hit.m_damage.m_frost *= multiplier;
                        hit.m_damage.m_spirit *= multiplier;
                    }
                }
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), nameof(Character.AddFireDamage))]
            private static void Character_FireResistance_Prefix(Character __instance, ref float damage)
            {
                if (Instance == null || damage <= 0.0f) {
                    return;
                }

                if (__instance.m_faction == Character.Faction.Players) {
                    float multiplier = 1 - ((Instance.level * Instance.bonusPerLevel) / 100);
                    damage *= multiplier;
                }
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), nameof(Character.AddPoisonDamage))]
            private static void Character_PoisonResistance_Prefix(Character __instance, ref float damage)
            {
                if (Instance == null || damage <= 0.0f) {
                    return;
                }

                if (__instance.m_faction == Character.Faction.Players) {
                    float multiplier = 1 - ((Instance.level * Instance.bonusPerLevel) / 100);
                    damage *= multiplier;
                }
            }
        }
    }
}
