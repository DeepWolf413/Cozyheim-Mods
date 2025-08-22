using HarmonyLib;

namespace Cozyheim.LevelingSystem.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal static class PlayerInitPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Awake))]
        private static void PlayerAwake_Postfix(Player __instance)
        {
            __instance.gameObject.AddComponent<PlayerExpPool>();
        }
    }
}