using HarmonyLib;

namespace CharacterProgressionMod
{
    internal class SkillPhysicalDamage : SkillBase
    {
        public static SkillPhysicalDamage Instance;

        public SkillPhysicalDamage(int maxLevel, float bonusPerLevel, string iconName, string displayName,
                                   string unit = "",
                                   float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit,
                                                                baseBonus)
        {
            skillType = SkillType.PhysicalDamage;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "ApplyDamage")]
            private static void Character_PhysicalDamage_Prefix(Character __instance, ref HitData hit)
            {
                if (Instance == null) {
                    return;
                }

                if (hit.HaveAttacker()) {
                    if (__instance.m_faction != Character.Faction.Players &&
                        hit.GetAttacker().m_faction == Character.Faction.Players) {
                        var multiplier = 1 + Instance.level * Instance.bonusPerLevel / 100;
                        hit.m_damage.m_blunt *= multiplier;
                        hit.m_damage.m_slash *= multiplier;
                        hit.m_damage.m_pierce *= multiplier;
                    }
                }
            }
        }
    }
}