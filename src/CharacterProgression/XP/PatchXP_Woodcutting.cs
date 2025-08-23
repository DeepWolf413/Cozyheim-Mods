using CharacterProgressionMod.Core;
using HarmonyLib;
using UnityEngine;

namespace CharacterProgressionMod
{
    internal class PatchXP_Woodcutting : MonoBehaviour
    {
        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(TreeBase), "Damage")]
            private static void TreeBase_Damage_Prefix(TreeBase __instance, HitData hit, ZNetView ___m_nview)
            {
                if (__instance == null || hit == null || ___m_nview == null) {
                    return;
                }

                if (!___m_nview.IsValid()) {
                    return;
                }

                if (hit.m_toolTier < __instance.m_minToolTier) {
                    return;
                }

                WoodcuttingXP(__instance.name, hit);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TreeLog), "Damage")]
            private static void TreeLog_Damage_Prefix(TreeLog __instance, HitData hit, ZNetView ___m_nview)
            {
                if (__instance == null || hit == null || ___m_nview == null) {
                    return;
                }

                if (!___m_nview.IsValid()) {
                    return;
                }

                if (hit.m_toolTier < __instance.m_minToolTier) {
                    return;
                }

                WoodcuttingXP(__instance.name, hit);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Destructible), "Damage")]
            private static void Destructible_Damage_Prefix(Destructible __instance, HitData hit, ZNetView ___m_nview,
                                                           bool ___m_firstFrame)
            {
                if (__instance == null || hit == null || ___m_nview == null) {
                    return;
                }

                if (!___m_nview.IsValid() || ___m_firstFrame) {
                    return;
                }

                if (hit.m_toolTier < __instance.m_minToolTier) {
                    return;
                }

                WoodcuttingXP(__instance.name, hit);
            }

            private static void WoodcuttingXP(string name, HitData hit)
            {
                // Check if the XP system is enabled
                if (!ModEntry.ModConfig.WoodcuttingXpEnabled.Value) {
                    return;
                }

                if (hit.GetAttacker() == null) {
                    return;
                }

                // Check if the attacker is a player
                var player = hit.GetAttacker().GetComponent<Player>();
                if (player == null) {
                    return;
                }

                if (hit.m_damage.m_chop <= 0) {
                    return;
                }

                // Check if the hit did any damage
                if (hit.GetTotalDamage() <= 0) {
                    return;
                }

                if (!player.gameObject.TryGetComponent(out PlayerExpPool expPool)) {
                    return;
                }

                expPool.CreateGenericExperienceReward(ExpSource.Woodcutting, name);
            }
        }
    }
}