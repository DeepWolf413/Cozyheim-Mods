using HarmonyLib;

namespace CharacterProgressionMod
{
    internal class SkillStaminaRegen : SkillBase
    {
        public static SkillStaminaRegen Instance;

        public SkillStaminaRegen(int maxLevel, float bonusPerLevel, string iconName, string displayName,
                                 string unit = "",
                                 float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit,
                                                              baseBonus)
        {
            skillType = SkillType.StaminaRegen;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(SEMan), "ModifyStaminaRegen")]
            private static void SEMan_ModifyStaminaRegen_Postfix(ref float staminaMultiplier)
            {
                if (Instance == null) {
                    return;
                }

                staminaMultiplier += Instance.level * Instance.bonusPerLevel / 100f;
            }
        }
    }
}