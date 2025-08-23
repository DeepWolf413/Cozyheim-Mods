using HarmonyLib;

namespace CharacterProgressionMod
{
    internal class SkillCarryWeight : SkillBase
    {
        public static SkillCarryWeight Instance;

        public SkillCarryWeight(int maxLevel, float bonusPerLevel, string iconName, string displayName,
                                string unit = "",
                                float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit,
                                                             baseBonus)
        {
            skillType = SkillType.CarryWeight;
            Instance = this;
        }


        [HarmonyPatch]
        private class SkillCarryWeight_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SEMan), "ModifyMaxCarryWeight")]
            private static void Player_ModifyMaxCarryWeight_Postfix(SEMan __instance, Character ___m_character,
                                                                    ref float limit)
            {
                if (Instance == null) {
                    return;
                }

                if (___m_character.IsPlayer()) {
                    limit += Instance.level * Instance.bonusPerLevel;
                }
            }
        }
    }
}