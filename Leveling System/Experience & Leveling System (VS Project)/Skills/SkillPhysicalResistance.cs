﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PrivilegeManager;

namespace Cozyheim.LevelingSystem
{
    internal class SkillPhysicalResistance : SkillBase
    {
        public static SkillPhysicalResistance Instance;

        public SkillPhysicalResistance(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "") : base(maxLevel, bonusPerLevel, iconName, displayName, unit)
        {
            skillType = SkillType.PhysicalResistance;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "ApplyDamage")]
            private static void Character_PhysicalResistance_Prefix(Character __instance, ref HitData hit)
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
                        hit.m_damage.m_blunt *= multiplier;
                        hit.m_damage.m_slash *= multiplier;
                        hit.m_damage.m_pierce *= multiplier;
                    }
                }
            }
        }

    }
}
