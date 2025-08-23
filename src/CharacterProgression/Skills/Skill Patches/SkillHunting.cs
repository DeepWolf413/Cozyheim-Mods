using HarmonyLib;

namespace CharacterProgressionMod
{
    internal class SkillHunting : SkillBase
    {
        public static SkillHunting Instance;

        public SkillHunting(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "",
                            float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit,
                                                         baseBonus)
        {
            skillType = SkillType.Hunting;
            Instance = this;
        }


        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Player), "Awake")]
            private static void Player_Awake_Prefix()
            {
                if (Instance == null) { }

                // Patch code here
            }
        }
    }
}