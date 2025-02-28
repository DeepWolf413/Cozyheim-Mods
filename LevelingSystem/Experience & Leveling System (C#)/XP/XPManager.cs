using System.Collections;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace Cozyheim.LevelingSystem;

internal class XPManager : MonoBehaviour
{
	private static readonly string saveLevelString = "CozyLevel";
	private static readonly string saveXpString = "CozyXP";

	// Network communication RPC
	public static CustomRPC rpc_AddMonsterDamage;
	public static CustomRPC rpc_RewardXPMonster;
	public static CustomRPC rpc_GetXP;

	private static XPManager _instance;

	private readonly List<MonsterXP> xpObjects = new();

	public static XPManager Instance
	{
		get
		{
			if (_instance == null) _instance = new GameObject("XPManager").AddComponent<XPManager>();
			return _instance;
		}
	}

	public static void Init()
	{
		// Register RPC Methods
		rpc_AddMonsterDamage = NetworkManager.Instance.AddRPC("AddMonsterDamage", RPC_AddMonsterDamage, RPC_AddMonsterDamage);
		rpc_RewardXPMonster = NetworkManager.Instance.AddRPC("RewardXPMonster", RPC_RewardXPMonsters, RPC_RewardXPMonsters);
		rpc_GetXP = NetworkManager.Instance.AddRPC("GetXP", RPC_GetXPFromServer, RPC_GetXPFromServer);

		XPTable.UpdateMiningXPTable();
		XPTable.UpdateMonsterXPTable();
		XPTable.UpdatePickableXPTable();
		XPTable.UpdatePlayerXPTable();
		XPTable.UpdateWoodcuttingXPTable();
	}

	private static IEnumerator RPC_AddMonsterDamage(long sender, ZPackage package)
	{
		if (!ZNet.instance.IsServer())
		{
			ConsoleLog.Print("Expected server instance but got a non-server instance. Rejecting RPC", LogType.Error);
			yield break;
		}

		var monsterID = package.ReadUInt();
		var playerID = package.ReadLong();
		var damage = package.ReadSingle();
		var playerName = package.ReadString();

		var obj = Instance.GetMonsterXP(monsterID);
		if (obj != null) {
			ConsoleLog.Print("Updated monster damage (Server)");
			obj.AddDamage(playerID, damage, playerName);
		}
		else {
			ConsoleLog.Print("Created new monster damage (Server)");
			var newObj = Instance.CreateNewMonsterXP(monsterID);
			newObj.AddDamage(playerID, damage, playerName);
		}

		yield return null;
	}

	public void AddMonsterDamage(Character monster, Character player, float damage)
	{
		var monsterID = monster.GetZDOID().ID;
		var playerID = player.GetComponent<Player>().GetPlayerID();
		var playerName = player.GetComponent<Player>().GetPlayerName();

		var newPackage = new ZPackage();
		newPackage.Write(monsterID);
		newPackage.Write(playerID);
		newPackage.Write(damage);
		newPackage.Write(playerName);

		ConsoleLog.Print("Sending damage to server RPC");
		rpc_AddMonsterDamage.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), newPackage);
	}

	private MonsterXP CreateNewMonsterXP(uint monsterID)
	{
		var newObj = new MonsterXP(monsterID);
		xpObjects.Add(newObj);

		return newObj;
	}

	public void GetXPFromServer(long playerID, string itemName, string itemType, int xpMultiplier = 1)
	{
		ConsoleLog.Print("Trying to get XP from server (" + itemName + " - " + itemType + " - " + xpMultiplier + ")");
		var newPackage = new ZPackage();
		newPackage.Write(playerID);
		newPackage.Write(itemName);
		newPackage.Write(itemType);
		newPackage.Write(xpMultiplier);
		rpc_GetXP.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), newPackage);
	}

	private static IEnumerator RPC_GetXPFromServer(long sender, ZPackage package)
	{
		if (!ZNet.instance.IsServer())
		{
			ConsoleLog.Print("Expected server instance but got a non-server instance. Rejecting RPC", LogType.Error);
			yield break;
		}

		var playerID = package.ReadLong();
		var itemName = package.ReadString();
		var itemType = package.ReadString();
		var xpMultiplier = package.ReadInt();

		ConsoleLog.Print("Server: Recieved GetXP Call (" + itemName + " - " + itemType + " - " + xpMultiplier + ")");

		int xp;
		switch (itemType) {
			case "Woodcutting":
				xp = XPTable.GetWoodcuttingXP(itemName);
				break;
			case "Mining":
				xp = XPTable.GetMiningXP(itemName);
				break;
			case "Pickable":
				xp = XPTable.GetPickableXP(itemName);
				break;
			default:
				yield break;
		}

		if (xp <= 0) {
			yield break;
		}

		ConsoleLog.Print("Server: Found XP = " + xp);
		//var playerPeerId = ZNet.instance.GetPeer(playerID).m_uid;
		RewardXP(sender, playerID, xp * xpMultiplier, itemType);
	}

	private static void RewardXP(long playerPeerId, long playerId, int xpAmount, string itemType)
	{
		if (!ZNet.instance.IsServer()) {
			return;
		}

		var baseXpSpreadMin = Mathf.Min(1 - Main.baseXpSpreadMin.Value / 100f, 1f);
		var baseXpSpreadMax = Mathf.Max(1 + Main.baseXpSpreadMax.Value / 100f, 1f);
		var xpMultiplier = Mathf.Max(0f, Main.allXPMultiplier.Value / 100f);
		var restedMultiplier = Mathf.Max(0f, Main.restedXPMultiplier.Value / 100f);

		var xp = (int)(xpAmount * xpMultiplier * Random.Range(baseXpSpreadMin, baseXpSpreadMax));
		var restedBonusXp = (int)(xp * restedMultiplier);

		var newPackage = new ZPackage();
		newPackage.Write(playerId);
		newPackage.Write(xp);
		newPackage.Write(itemType);
		newPackage.Write(restedBonusXp);
		
		ConsoleLog.Print("Server: Sending XP to Player (XP: " + xp);
		UIManager.rpc_AddExperience.SendPackage(playerPeerId, newPackage);
	}

	private static IEnumerator RPC_RewardXPMonsters(long sender, ZPackage package)
	{
		if (!ZNet.instance.IsServer()) yield break;

		var monsterID = package.ReadUInt();
		var monsterLevel = package.ReadUInt();
		var monsterName = package.ReadString();

		ConsoleLog.Print("Monster died (Server) - " + monsterName);

		var monsterObj = Instance.GetMonsterXP(monsterID);
		if (monsterObj != null) {
			var totalDamage = monsterObj.GetTotalDamageDealt();

			var dsHealthMultiplier = 0f;
			var dsDamageMultiplier = 0f;
			var dsBiomeMultiplier = 0f;
			var dsNightMultiplier = 0f;
			var dsBossKillMultiplier = 0f;
			var dsStarMultiplier = 0f;

			var dsFound = package.ReadBool();

			if (dsFound) {
				dsHealthMultiplier = package.ReadSingle();
				dsDamageMultiplier = package.ReadSingle();
				dsBiomeMultiplier = package.ReadSingle();
				dsNightMultiplier = package.ReadSingle();
				dsBossKillMultiplier = package.ReadSingle();
				dsStarMultiplier = package.ReadSingle();
			}

			// Find the correct monster in the list
			foreach (var damage in monsterObj.playerDamages) {
				var newPackage = new ZPackage();

				// Get the percentage of damage the player has dealt
				var xpPercentage = damage.playerTotalDamage / totalDamage;

				// Reward with xp based on monster type killed
				var baseXpSpreadMin = Mathf.Min(1 - Main.baseXpSpreadMin.Value / 100f, 1f);
				var baseXpSpreadMax = Mathf.Max(1 + Main.baseXpSpreadMax.Value / 100f, 1f);
				var monsterLvlMultiplier = Mathf.Max(0f, Main.monsterLvlXPMultiplier.Value / 100f);
				var xpMultiplier = Mathf.Max(0f, Main.allXPMultiplier.Value / 100f);
				var restedMultiplier = Mathf.Max(0f, Main.restedXPMultiplier.Value / 100f);

				var awardedXP = XPTable.GetMonsterXP(monsterName) * xpPercentage * Random.Range(baseXpSpreadMin, baseXpSpreadMax) * xpMultiplier;

				// Apply difficulty scaler xp
				if (dsFound && Main.modDifficultyScalerLoaded)
					if (Main.enableDifficultyScalerXP.Value) {
						var dsHealthBonus = dsHealthMultiplier * Main.difficultyScalerOverallHealthRatio.Value;
						var dsDamageBonus = dsDamageMultiplier * Main.difficultyScalerOverallDamageRatio.Value;
						var dsBiomeBonus = dsBiomeMultiplier * Main.difficultyScalerBiomeRatio.Value;
						var dsNightBonus = dsNightMultiplier * Main.difficultyScalerBossRatio.Value;
						var dsBossBonus = dsBossKillMultiplier * Main.difficultyScalerBossRatio.Value;
						var dsStarBonus = dsStarMultiplier * Main.difficultyScalerStarRatio.Value;

						var totalBonusMultiplier = 0f;

						ConsoleLog.Print("XP before scaling: " + awardedXP);

						if (Main.difficultyScalerOverallHealth.Value) totalBonusMultiplier += dsHealthBonus;

						if (Main.difficultyScalerOverallDamage.Value) totalBonusMultiplier += dsDamageBonus;

						if (Main.difficultyScalerBiome.Value) totalBonusMultiplier += dsBiomeBonus;

						if (Main.difficultyScalerNight.Value) totalBonusMultiplier += dsNightBonus;

						if (Main.difficultyScalerBoss.Value) totalBonusMultiplier += dsBossBonus;

						if (Main.difficultyScalerStar.Value) totalBonusMultiplier += dsStarBonus;

						awardedXP *= totalBonusMultiplier + 1f;

						ConsoleLog.Print($"XP scaled with {(totalBonusMultiplier * 100f).ToString("N0")}%: " + awardedXP);
					}

				var monsterLevelBonusXp = (monsterLevel - 1) * monsterLvlMultiplier * awardedXP;
				var restedBonusXp = awardedXP * restedMultiplier;

				newPackage.Write((int)awardedXP);
				newPackage.Write((int)monsterLevelBonusXp);
				newPackage.Write((int)restedBonusXp);
				newPackage.Write(damage.playerID);
				newPackage.Write(monsterName);


				ConsoleLog.Print("Sending " + (xpPercentage * 100f).ToString("N1") + "% xp to " + damage.playerName + ". (Awarded: " + (int)awardedXP + ", Level bonus: " + (int)monsterLevelBonusXp + ", Rested bonus: " + (int)restedBonusXp + ")");

				//var playerPeerId = ZNet.instance.GetPeer(sender);
				UIManager.rpc_AddExperienceMonster.SendPackage(sender, newPackage);
			}

			Instance.xpObjects.Remove(monsterObj);
		}
	}

	private MonsterXP GetMonsterXP(uint monsterID)
	{
		foreach (var obj in xpObjects)
			if (obj.monsterID == monsterID)
				return obj;

		return null;
	}

	public string GetAllMonsterXpString()
	{
		var response = "Total monsters: " + xpObjects.Count;
		foreach (var obj in xpObjects) response += "\n-> MonsterID: " + obj.monsterID;

		return response;
	}

	public void SetPlayerLevel(int level)
	{
		if (!Player.m_localPlayer || Player.m_localPlayer.m_customData == null) {
			return;
		}

		Player.m_localPlayer.m_customData[saveLevelString] = level.ToString();
	}

	public void SavePlayerLevel()
	{
		if (!Player.m_localPlayer || Player.m_localPlayer.m_customData == null || !UIManager.Instance) {
			return;
		}

		Player.m_localPlayer.m_customData[saveLevelString] = UIManager.Instance.playerLevel.ToString();
	}

	public void SetPlayerXP(int xp)
	{
		if (!Player.m_localPlayer || Player.m_localPlayer.m_customData == null) {
			return;
		}

		Player.m_localPlayer.m_customData[saveXpString] = xp.ToString();
	}

	public void SavePlayerXP()
	{
		if (!Player.m_localPlayer || Player.m_localPlayer.m_customData == null || !UIManager.Instance) {
			return;
		}

		Player.m_localPlayer.m_customData[saveXpString] = UIManager.Instance.playerXP.ToString();
	}

	public int GetPlayerLevel()
	{
		if (!Player.m_localPlayer || Player.m_localPlayer.m_customData == null) {
			return 1;
		}
		
		var value = 1;
		if (Player.m_localPlayer.m_customData.ContainsKey(saveLevelString)) {
			var savedString = Player.m_localPlayer.m_customData[saveLevelString];
			int.TryParse(savedString, out value);
		}

		return value;
	}

	public int GetPlayerXP()
	{
		if (!Player.m_localPlayer || Player.m_localPlayer.m_customData == null) {
			return 0;
		}
		
		var value = 0;
		if (Player.m_localPlayer.m_customData.ContainsKey(saveXpString)) {
			var savedString = Player.m_localPlayer.m_customData[saveXpString];
			int.TryParse(savedString, out value);
		}

		return value;
	}
}
