﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cozyheim.LevelingSystem
{
    internal class SkillHPRegen : SkillBase
    {
        public static SkillHPRegen Instance;

        public SkillHPRegen(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
        {
            skillType = SkillType.HPRegen;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SEMan), "ModifyHealthRegen")]
            static void SEMan_ModifyHealthRegen_Postfix(ref float regenMultiplier)
            {
                if (Instance == null)
                {
                    return;
                }

                regenMultiplier += (Instance.level * Instance.bonusPerLevel) / 100f;
            }
        }
    }
}
