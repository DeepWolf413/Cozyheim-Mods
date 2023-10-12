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

			if (!Main.pickableXpEnabled.Value) return;

			var honeyLevel = ___m_nview.GetZDO().GetInt("level");
			if (honeyLevel <= 0) return;

			var player = character.GetComponent<Player>();
			if (player == null) return;

			// Get xp from server and send it to the player
			var playerID = player.GetPlayerID();
			XPManager.Instance.GetXPFromServer(playerID, __instance.name, "Pickable", honeyLevel);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Pickable), "RPC_SetPicked")]
		private static void Pickable_RPCSetPicked_Prefix(Pickable __instance, long sender, bool picked, bool ___m_picked)
		{
			if (__instance == null || Player.m_localPlayer.GetZDOID().UserID != sender) {
				ConsoleLog.Print($"Sender id ({sender}) doesn't match with nview uid ({Player.m_localPlayer.GetZDOID().UserID})", LogType.Error);
				return;
			}

			// Ignore if already picked.
			if (picked == ___m_picked || !picked) {
				ConsoleLog.Print("Already picked!", LogType.Error);
				return;
			}

			var player = Player.m_localPlayer;
			if (player == null) {
				ConsoleLog.Print("Failed to get local player from Pickable_RPCSetPicked", LogType.Error);
				return;
			}

			// Get xp from server and send it to the player
			var playerID = player.GetPlayerID();
			XPManager.Instance.GetXPFromServer(playerID, __instance.name, "Pickable");
		}
	}
}