using HarmonyLib;

namespace CharacterProgressionMod.Patches
{
    [HarmonyPatch]
    internal static class ForagingExpSourcePatch
    {
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
        [HarmonyPrefix]
        private static void Interact_Prefix(Pickable __instance, Humanoid character)
        {
            if (__instance.m_picked) {
                return;
            }

            var player = (Player)character;
            if (!player.TryGetComponent(out PlayerLevelProgression playerExpPool)) {
                return;
            }
            
            playerExpPool.AddExperience(5);
        }
    }
}