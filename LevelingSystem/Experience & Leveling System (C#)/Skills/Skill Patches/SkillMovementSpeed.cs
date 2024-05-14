using HarmonyLib;

namespace Cozyheim.LevelingSystem;

internal class SkillMovementSpeed : SkillBase
{
    public static SkillMovementSpeed Instance;

    public SkillMovementSpeed(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "",
        float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
    {
        skillType = SkillType.MovementSpeed;
        Instance = this;
    }


    [HarmonyPatch]
    private class PatchClass
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetJogSpeedFactor")]
        private static void Player_GetJogSpeedFactor_Postfix(Player __instance, ref float __result)
        {
            if (Instance == null) return;

            if (__instance == Player.m_localPlayer) {
                var bonusValue = Instance.level * Instance.bonusPerLevel / 100f;
                __result += bonusValue;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetRunSpeedFactor")]
        private static void Player_GetRunSpeedFactor_Postfix(Player __instance, ref float __result)
        {
            if (Instance == null) return;

            if (__instance == Player.m_localPlayer) {
                var bonusValue = Instance.level * Instance.bonusPerLevel / 100f;
                var threshold = bonusValue * 0.25f;

                var runValue = __result + bonusValue * 0.25f;
                var jogValue = 1f + __instance.GetEquipmentMovementModifier() + bonusValue;

                __result = runValue > jogValue + threshold ? runValue : runValue + threshold;
            }
        }
    }
}