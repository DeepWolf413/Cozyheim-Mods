using Cozyheim.DifficultyScaler;
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
			if (!___m_nview.IsValid()) {
				ConsoleLog.Print("Damage: ZNetView not valid!", LogType.Error);
				return;
			}

			if (hit == null) {
				ConsoleLog.Print("Damage: No HitData found!", LogType.Error);
				return;
			}

			if (__instance == null) {
				ConsoleLog.Print("Damage: No Character found!", LogType.Error);
				return;
			}

			var target = __instance;
			var attacker = hit.GetAttacker();
			var totalDamage = hit.GetTotalDamage();

			if (target == null) {
				ConsoleLog.Print("Damage: No target found!", LogType.Error);
				return;
			}

			if (!CanTargetAwardXP(target)) {
				ConsoleLog.Print("Damage: Target not a Monster!", LogType.Error);
				return;
			}

			if (attacker == null) {
				ConsoleLog.Print("Damage: No attacker found!", LogType.Error);
				return;
			}

			if (!attacker.IsPlayer()) {
				ConsoleLog.Print("Damage: Attacker not a Player!", LogType.Error);
				return;
			}

			if (Player.m_localPlayer == null) {
				ConsoleLog.Print("Damage: No local player found!", LogType.Error);
				return;
			}

			if (totalDamage <= 0f) {
				ConsoleLog.Print("Damage: Total damage is less than 0!", LogType.Error);
				return;
			}

			var player = attacker.GetComponent<Player>();
			if (player != Player.m_localPlayer) {
				ConsoleLog.Print("Damage: The attacker is not you!", LogType.Error);
				return;
			}

			if (Main.debugMonsterInternalName.Value) {
				var monsterPrefabName = target.name.Replace("(Clone)", "");
				ConsoleLog.PrintOverrideDebugMode("Monster Internal ID: " + monsterPrefabName);
			}

			ConsoleLog.Print("Damage: Success! (Target = " + target.name + ", Attacker = " + player.GetPlayerName() + ", Damage: " + totalDamage.ToString("N1") + ")", LogType.Message);
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

				var comp = __instance.gameObject.GetComponent<DifficultyScalerBase>();
				var dsFound = comp != null;
				newPackage.Write(dsFound);

				ConsoleLog.Print(__instance.name + ": Found DS = " + dsFound);

				if (comp != null) {
					newPackage.Write(comp.GetMultiplier(DifficultyScalerMultiplier.HealthMultiplier));
					newPackage.Write(comp.GetMultiplier(DifficultyScalerMultiplier.DamageMultiplier));
					newPackage.Write(comp.GetMultiplier(DifficultyScalerMultiplier.BiomeMultiplier));
					newPackage.Write(comp.GetMultiplier(DifficultyScalerMultiplier.NightMultiplier));
					newPackage.Write(comp.GetMultiplier(DifficultyScalerMultiplier.BossKillMultiplier));
					newPackage.Write(comp.GetMultiplier(DifficultyScalerMultiplier.StarMultiplier));
				}

				XPManager.rpc_RewardXPMonster.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), newPackage);
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
