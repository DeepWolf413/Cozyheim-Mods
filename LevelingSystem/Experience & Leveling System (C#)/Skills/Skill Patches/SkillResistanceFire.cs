using HarmonyLib;

namespace Cozyheim.LevelingSystem
{
    internal class SkillResistanceFire : SkillBase
    {
        public static SkillResistanceFire Instance;

        public SkillResistanceFire(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
        {
            skillType = SkillType.ResistanceFire;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), nameof(Character.AddFireDamage))]
            private static void Character_ElementalResistance_Prefix(Character __instance, ref float damage)
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
