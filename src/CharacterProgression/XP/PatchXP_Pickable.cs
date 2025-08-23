using CharacterProgressionMod.Core;
using HarmonyLib;
using UnityEngine;

namespace CharacterProgressionMod
{
    internal class PatchXP_Pickable : MonoBehaviour
    {
        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Beehive), "Interact")]
            private static void Beehive_Interact_Prefix(Beehive __instance, Humanoid character, ZNetView ___m_nview)
            {
                if (__instance == null || character == null || ___m_nview == null) {
                    return;
                }

                if (!ModEntry.ModConfig.PickableXpEnabled.Value) {
                    return;
                }

                var honeyLevel = ___m_nview.GetZDO().GetInt("level");
                if (honeyLevel <= 0) {
                    return;
                }

                if (!character.TryGetComponent(out PlayerExpPool expPool)) {
                    return;
                }

                expPool.CreateGenericExperienceReward(ExpSource.Pickable, __instance.name, honeyLevel);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Pickable), nameof(Pickable.RPC_SetPicked))]
            private static void Pickable_RPCSetPicked_Prefix(Pickable __instance, long sender, bool picked,
                                                             bool ___m_picked)
            {
                var localPlayer = Player.m_localPlayer;
                if (localPlayer == null || __instance == null) {
                    return;
                }

                if (localPlayer.GetZDOID().UserID != sender) {
                    return;
                }

                if (picked == ___m_picked || !picked) {
                    return;
                }

                if (!localPlayer.TryGetComponent(out PlayerExpPool expPool)) {
                    return;
                }

                expPool.CreateGenericExperienceReward(ExpSource.Pickable, __instance.name);
            }
        }
    }
}