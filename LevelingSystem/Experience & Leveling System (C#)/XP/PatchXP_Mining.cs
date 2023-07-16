using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Cozyheim.LevelingSystem
{
    internal class PatchXP_Mining : MonoBehaviour
    {
        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MineRock5), "Start")]
            private static void MineRock5_Start_Prefix(MineRock5 __instance, ref object[] __state)
            {
                if (__instance == null)
                {
                    return;
                }

                __state = new object[] { __instance.name };
            }
            
            [HarmonyPostfix]
            [HarmonyPatch(typeof(MineRock5), "Start")]
            private static void MineRock5_Start_Postfix(MineRock5 __instance, object[] __state)
            {
                if (__instance == null)
                {
                    return;
                }

                if (__state.Length == 0) {
                    return;
                }

                __instance.name = (string)__state[0];
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MineRock5), "Damage")]
            private static void MineRock5_Damage_Prefix(MineRock5 __instance, HitData hit, ZNetView ___m_nview)
            {
                if (__instance == null || hit == null || ___m_nview == null)
                {
                    return;
                }

                if(!___m_nview.IsValid())
                {
                    return;
                }

                if (hit.m_toolTier < __instance.m_minToolTier)
                {
                    return;
                }

                MiningXP(__instance.name, hit);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MineRock), "Damage")]
            private static void MineRock_Damage_Prefix(MineRock __instance, HitData hit, ZNetView ___m_nview)
            {
                if (__instance == null || hit == null || ___m_nview == null)
                {
                    return;
                }

                if (!___m_nview.IsValid())
                {
                    return;
                }

                if (hit.m_toolTier < __instance.m_minToolTier)
                {
                    return;
                }

                MiningXP(__instance.name, hit);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Destructible), "Damage")]
            private static void Destructible_Damage_Prefix(Destructible __instance, HitData hit, ZNetView ___m_nview, bool ___m_firstFrame)
            {
                if(__instance == null || hit == null || ___m_nview == null)
                {
                    return;
                }

                if (!___m_nview.IsValid() || ___m_firstFrame)
                {
                    return;
                }

                if(hit.m_toolTier < __instance.m_minToolTier)
                {
                    return;
                }

                MiningXP(__instance.name, hit);
            }

            private static void MiningXP(string name, HitData hit)
            {
                // Check if the XP system is enabled
                if (!Main.miningXpEnabled.Value)
                {
                    return;
                }

                if(hit.m_damage.m_pickaxe <= 0)
                {
                    return;
                }

                Character attacker = hit.GetAttacker();
                if(attacker == null) {
                    return;
                }

                // Check if the attacker is a player
                Player player = hit.GetAttacker().GetComponent<Player>();
                if (player == null)
                {
                    return;
                }

                // Check if the hit did any damage
                if (hit.GetTotalDamage() <= 0)
                {
                    return;
                }

                // Get xp from server and send it to the player
                long playerID = player.GetPlayerID();
                XPManager.Instance.GetXPFromServer(playerID, name, "Mining");
            }
        }
    }
}
