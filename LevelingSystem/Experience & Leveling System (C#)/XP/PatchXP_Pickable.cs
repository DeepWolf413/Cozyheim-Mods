using HarmonyLib;
using UnityEngine;

namespace Cozyheim.LevelingSystem;

internal class PatchXP_Pickable : MonoBehaviour
{
	[HarmonyPatch]
	private class PatchClass
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Beehive), "Interact")]
		private static void Beehive_Interact_Prefix(Beehive __instance, Humanoid character, ZNetView ___m_nview)
		{
			if (__instance == null || character == null || ___m_nview == null) return;

			if (!Main.ModConfig.PickableXpEnabled.Value) return;

			var honeyLevel = ___m_nview.GetZDO().GetInt("level");
			if (honeyLevel <= 0) return;

			var player = character.GetComponent<Player>();
			if (player == null) return;

			// Get xp from server and send it to the player
			var playerID = player.GetPlayerID();
			XPManager.Instance.GetXPFromServer(playerID, __instance.name, "Pickable", honeyLevel);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Pickable), nameof(Pickable.RPC_SetPicked))]
		private static void Pickable_RPCSetPicked_Prefix(Pickable __instance, long sender, bool picked, bool ___m_picked)
		{
			var localPlayer = Player.m_localPlayer;
			if (localPlayer == null || __instance == null) {
				return;
			}
			
			// Making sure only the player picking up the pickable is given xp
			if (localPlayer.GetZDOID().UserID != sender)
			{
				return;
			}
			
			// Ignore if already picked
			if (picked == ___m_picked || !picked)
			{
				return;
			}

			var localPlayerId = localPlayer.GetPlayerID();
			XPManager.Instance.GetXPFromServer(localPlayerId, __instance.name, "Pickable");
		}
	}
}