using HarmonyLib;

namespace CharacterProgressionMod.Patches
{
    internal static partial class Patcher
    {
        [HarmonyPatch(typeof(Player))]
        internal static class PlayerInitPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Player.Awake))]
            private static void PlayerAwake_Postfix(Player __instance)
            {
                var playerExpPool = __instance.gameObject.AddComponent<PlayerLevelProgression>();
                playerExpPool.ExperienceTable = _resources.ExperienceTable;
            }
        }
    }
}