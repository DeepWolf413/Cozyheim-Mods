using HarmonyLib;

namespace CharacterProgressionMod.Patches
{
    internal static partial class Patcher
    {
        [HarmonyPatch(typeof(Character))]
        internal static class CharacterInitPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Character.Awake))]
            private static void CharacterAwake_Postfix(Character __instance)
            {
                if (__instance.IsPlayer() || !__instance.IsOwner()) {
                    return;
                }

                __instance.gameObject.AddComponent<DamageRegistry>();
                __instance.gameObject.AddComponent<RewardExpOnDeath>();
            }
        }
    }
}