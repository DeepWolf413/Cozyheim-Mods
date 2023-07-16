using BepInEx.Configuration;
using HarmonyLib;

namespace Cozyheim.LevelingSystem
{
    [HarmonyPatch]
    internal class XPBarFade_Patch
    {
        // ref bool __result = returned value of the original method
        // ref <T> __instance = reference to original class (this)
        // object[] __args = original parameters
        // -> Example: Player player = (Player) __args[0];

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "UpdateTeleport")]
        static void Player_UpdateTeleport_Postfix(bool ___m_teleporting)
        {
            if(___m_teleporting)
            {
                UIManager.Instance.FadeOutXPBar();
            } else {
                UIManager.Instance.FadeInXPBar();
            }
        }
        */

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hud), "UpdateBlackScreen")]
        static void Player_SetSleeping_Prefix(Player player)
        {
            if(Player.m_localPlayer == null || ZNetScene.instance == null)
            {
                return;
            }

            float fadeTime = 1f;
            if (player != null)
            {
                if (player.IsDead())
                {
                    fadeTime = 9.5f;
                }
                if (player.IsSleeping())
                {
                    fadeTime = 3f;
                }
            }

            if (player == null || player.IsDead() || player.IsTeleporting() || Game.instance.IsShuttingDown() || player.IsSleeping())
            {
                if (UIManager.Instance == null) {
                    ConsoleLog.Print("UIManager is missing! Can't fade out XPBar", LogType.Error);
                    return;
                }
                
                UIManager.Instance.FadeOutXPBar(fadeTime);
            } else
            {
                if (UIManager.Instance == null) {
                    ConsoleLog.Print("UIManager is missing! Can't fade out XPBar", LogType.Error);
                    return;
                }
                
                UIManager.Instance.FadeInXPBar(fadeTime);
            }
        }
    }
}
