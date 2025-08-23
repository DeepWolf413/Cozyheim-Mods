using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace DeepWolf.CharacterProgressionMod
{
    internal class PatchXP_Monster : MonoBehaviour
    {
        [HarmonyPatch]
        private class PatchClass
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Player), "Start")]
            private static void Player_Start_Postfix(ref ZNetView ___m_nview)
            {
                if (ZNet.instance != null && Player.m_localPlayer != null) {
                    if (UIManager.Instance == null) {
                        Instantiate(PrefabManager.Instance.GetPrefab("LevelingSystemUI"));
                    }
                }
            }


            [HarmonyPrefix]
            [HarmonyPatch(typeof(Game), "Logout")]
            private static void Game_Logout_Prefix()
            {
                if (UIManager.Instance != null) {
                    UIManager.Instance.DestroySelf();
                }
            }
        }
    }
}