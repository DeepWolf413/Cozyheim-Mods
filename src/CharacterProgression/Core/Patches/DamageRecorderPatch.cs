using HarmonyLib;

namespace CharacterProgressionMod.Core.Patches
{
    [HarmonyPatch(typeof(Character))]
    internal static class DamageRecorderPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Character.ApplyDamage))]
        private static void ApplyDamage_Postfix(Character __instance, HitData hit)
        {
            if (__instance.IsPlayer() || __instance.IsDebugFlying() || __instance.IsDead() ||
                __instance.IsTeleporting() ||
                __instance.InCutscene()) {
                return;
            }

            if (!__instance.TryGetComponent(out DamageRegistry damageRegistry)) {
                return;
            }

            const float minDamage = 0.1f;
            if (hit.GetTotalDamage() <= minDamage) {
                return;
            }

            damageRegistry.AddEntry(hit);
        }
    }
}