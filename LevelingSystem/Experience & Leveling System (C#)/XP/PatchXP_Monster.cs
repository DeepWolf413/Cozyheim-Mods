using Cozyheim.LevelingSystem.Constants;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace Cozyheim.LevelingSystem;

internal class PatchXP_Monster : MonoBehaviour
{
	[HarmonyPatch]
	private class PatchClass
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(Character), "Damage")]
		private static void Character_Damage_Prefix(Character __instance, ref HitData hit, ZNetView ___m_nview)
		{
			if (!___m_nview.IsValid())
			{
				Jotunn.Logger.LogError("ZNetView not valid!");
				return;
			}

			if (hit == null) {
				Jotunn.Logger.LogError("No HitData found!");
				return;
			}
			
			var target = __instance;
			var attacker = hit.GetAttacker();
			var totalDamage = hit.GetTotalDamage();

			if (target == null) {
				Jotunn.Logger.LogDebug("No target found!");
				return;
			}

			if (!CanTargetAwardXP(target)) {
				Jotunn.Logger.LogDebug("Target not a monster!");
				return;
			}

			if (attacker == null) {
				Jotunn.Logger.LogDebug("No attacker found!");
				return;
			}

			if (!attacker.IsPlayer()) {
				Jotunn.Logger.LogDebug("Attacker not a player!");
				return;
			}

			if (Player.m_localPlayer == null) {
				Jotunn.Logger.LogDebug("No local player found!");
				return;
			}

			if (totalDamage <= 0f) {
				Jotunn.Logger.LogDebug("Total damage is less than 0!");
				return;
			}

			var player = attacker.GetComponent<Player>();
			if (player != Player.m_localPlayer) {
				// Abort if damage is self-inflicted.
				return;
			}

			Jotunn.Logger.LogDebug($"Target: {target.name} | Attacker: {player.GetPlayerName()} | TotalDamage: {totalDamage}");
			XPManager.Instance.AddMonsterDamage(target, attacker, totalDamage);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(Character), "OnDeath")]
		private static void Character_OnDeath_Prefix(Character __instance)
		{
			if (Player.m_localPlayer == null) return;

			if (CanTargetAwardXP(__instance) && Player.m_localPlayer != null) {
				var newPackage = new ZPackage();

				newPackage.Write(__instance.GetZDOID().ID);
				newPackage.Write(__instance.GetLevel());
				newPackage.Write(__instance.name);

				ModRpcRegistry.Instance.SendServerRpc(RpcConstants.ServerRewardXpMonster, newPackage);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "Start")]
		private static void Player_Start_Postfix(ref ZNetView ___m_nview)
		{
			if (ZNet.instance != null && Player.m_localPlayer != null)
				if (UIManager.Instance == null)
					Instantiate(PrefabManager.Instance.GetPrefab("LevelingSystemUI"));
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(Game), "Logout")]
		private static void Game_Logout_Prefix()
		{
			if (UIManager.Instance != null) UIManager.Instance.DestroySelf();
		}

		private static bool CanTargetAwardXP(Character target)
		{
			Character.Faction[] allowedFactions =
			{ Character.Faction.ForestMonsters,
			  Character.Faction.SeaMonsters,
			  Character.Faction.MountainMonsters,
			  Character.Faction.PlainsMonsters,
			  Character.Faction.MistlandsMonsters,
			  Character.Faction.Dverger,
			  Character.Faction.Undead,
			  Character.Faction.Demon,
			  Character.Faction.AnimalsVeg,
			  Character.Faction.Boss };

			foreach (var faction in allowedFactions)
				if (target.GetFaction() == faction)
					return true;

			return false;
		}
	}
}
