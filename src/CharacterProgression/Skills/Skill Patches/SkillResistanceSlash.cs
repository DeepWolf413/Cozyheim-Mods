using HarmonyLib;

namespace CharacterProgressionMod
{
    internal class SkillResistanceSlash : SkillBase
    {
        public static SkillResistanceSlash Instance;

        public SkillResistanceSlash(int maxLevel, float bonusPerLevel, string iconName, string displayName,
                                    string unit = "", float baseBonus = 0f) : base(
            maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
        {
            skillType = SkillType.ResistanceSlash;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "ApplyDamage")]
            private static void Character_PhysicalResistance_Prefix(Character __instance, ref HitData hit)
            {
                if (Instance == null) {
                    return;
                }

                if (hit.HaveAttacker()) {
                    if (__instance.m_faction == Character.Faction.Players) {
                        var multiplier = 1 - Instance.level * Instance.bonusPerLevel / 100;
                        hit.m_damage.m_slash *= multiplier;
                    }
                }
            }
        }
    }
}