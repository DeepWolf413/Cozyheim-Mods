using HarmonyLib;

namespace DeepWolf.CharacterProgressionMod
{
    [HarmonyPatch]
    internal class XPBarFade_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hud), "UpdateBlackScreen")]
        private static void Player_SetSleeping_Prefix(Player player)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) {
                return;
            }

            var fadeTime = 1f;
            if (player != null) {
                if (player.IsDead()) {
                    fadeTime = 9.5f;
                }

                if (player.IsSleeping()) {
                    fadeTime = 3f;
                }
            }

            if (player == null || player.IsDead() || player.IsTeleporting() || Game.instance.IsShuttingDown() ||
                player.IsSleeping()) {
                if (UIManager.Instance == null) {
                    return;
                }

                UIManager.Instance.FadeOutXPBar(fadeTime);
            }
            else {
                if (UIManager.Instance == null) {
                    return;
                }

                UIManager.Instance.FadeInXPBar(fadeTime);
            }
        }
    }
}