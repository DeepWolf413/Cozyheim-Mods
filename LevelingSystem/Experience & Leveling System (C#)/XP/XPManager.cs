using System;
using System.Collections.Generic;
using System.IO;
using Cozyheim.LevelingSystem.Constants;
using Jotunn.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cozyheim.LevelingSystem;

internal class XPManager : MonoBehaviour
{
	private static readonly string saveLevelString = "CozyLevel";
	private static readonly string saveXpString = "CozyXP";

	private static XPManager _instance;
	public static XpTable MiningXpTable { get; private set; }
	public static XpTable WoodcuttingXpTable { get; private set; }
	public static XpTable PickablesXpTable { get; private set; }
	public static XpTable CreaturesXpTable { get; private set; }
	public static LevelXpTable PlayerXpTable { get; private set; }

	private readonly List<MonsterXP> xpObjects = new();

	public static XPManager Instance
	{
		get
		{
			if (_instance == null) _instance = new GameObject("XPManager").AddComponent<XPManager>();
			return _instance;
		}
	}

	private void Awake()
	{
		var resourceAssembly = ReflectionHelper.GetCallingAssembly();
		MiningXpTable = new XpTable(resourceAssembly, Path.Combine(Main.ConfigFolder, "mining"), true);
		WoodcuttingXpTable = new XpTable(resourceAssembly, Path.Combine(Main.ConfigFolder, "woodcutting"), true);
		PickablesXpTable = new XpTable(resourceAssembly, Path.Combine(Main.ConfigFolder, "pickables"), true);
		CreaturesXpTable = new XpTable(resourceAssembly, Path.Combine(Main.ConfigFolder, "creatures"), false);
		PlayerXpTable = new LevelXpTable(Path.Combine(Main.ConfigFolder, "player"), "LevelingSystem.Resources.default_configs.player.xp_tables.default.json");
	}

	public void Init()
	{
		// Register RPC Methods
		ModRpcRegistry.Instance.AddRpc(RpcConstants.ServerSetLevel, ModRpcRegistry.RegistryEntry.RpcType.ServerOnly, RPC_ServerSetLevel);
		ModRpcRegistry.Instance.AddRpc(RpcConstants.ServerAddMonsterDamage, ModRpcRegistry.RegistryEntry.RpcType.ServerOnly, RPC_AddMonsterDamage);
		ModRpcRegistry.Instance.AddRpc(RpcConstants.ServerRewardXpMonster, ModRpcRegistry.RegistryEntry.RpcType.ServerOnly, RPC_RewardXPMonsters);
		ModRpcRegistry.Instance.AddRpc(RpcConstants.ServerGetXp, ModRpcRegistry.RegistryEntry.RpcType.ServerOnly, RPC_GetXPFromServer);
	}

	private static void RPC_ServerSetLevel(long senderId, ZPackage package)
	{
		
	}

	private static void RPC_AddMonsterDamage(long sender, ZPackage package)
	{
		var monsterID = package.ReadUInt();
		var playerID = package.ReadLong();
		var damage = package.ReadSingle();
		var playerName = package.ReadString();

		var obj = Instance.GetMonsterXP(monsterID);
		if (obj != null) {
			obj.AddDamage(playerID, damage, playerName);
			Jotunn.Logger.LogDebug($"Added {damage} damage inflicted by player '{playerName}'");
		}
		else {
			var newObj = Instance.CreateNewMonsterXP(monsterID);
			newObj.AddDamage(playerID, damage, playerName);
			Jotunn.Logger.LogDebug($"Added {damage} initial damage inflicted by player '{playerName}'");
		}
	}

	public void AddMonsterDamage(Character monster, Character player, float damage)
	{
		var monsterID = monster.GetZDOID().ID;
		var playerID = player.GetZDOID().UserID;
		var playerName = player.GetComponent<Player>().GetPlayerName();
		
		var newPackage = new ZPackage();
		newPackage.Write(monsterID);
		newPackage.Write(playerID);
		newPackage.Write(damage);
		newPackage.Write(playerName);
		
		Jotunn.Logger.LogDebug("Sending package to server");
		ModRpcRegistry.Instance.SendServerRpc(RpcConstants.ServerAddMonsterDamage, newPackage);
	}

	private MonsterXP CreateNewMonsterXP(uint monsterID)
	{
		var newObj = new MonsterXP(monsterID);
		xpObjects.Add(newObj);

		return newObj;
	}

	public void GetXPFromServer(long playerID, string itemName, string itemType, int xpMultiplier = 1)
	{
		Jotunn.Logger.LogDebug($"Attempting to get xp reward from server. [ItemName: {itemName}, ItemType: {itemType}, XpMultiplier: {xpMultiplier}]");
		var newPackage = new ZPackage();
		newPackage.Write(playerID);
		newPackage.Write(itemName);
		newPackage.Write(itemType);
		newPackage.Write(xpMultiplier);
		Jotunn.Logger.LogDebug("Sending package to server");
		ModRpcRegistry.Instance.SendServerRpc(RpcConstants.ServerGetXp, newPackage);
	}

	private static void RPC_GetXPFromServer(long sender, ZPackage package)
	{
		var playerID = package.ReadLong();
		var itemName = package.ReadString();
		var itemType = package.ReadString();
		var xpMultiplier = package.ReadInt();

		Jotunn.Logger.LogDebug("Received xp reward request from client");

		int xp;
		switch (itemType) {
			case "Woodcutting":
				xp = WoodcuttingXpTable.GetXp(itemName);
				break;
			case "Mining":
				xp = MiningXpTable.GetXp(itemName);
				break;
			case "Pickable":
				xp = PickablesXpTable.GetXp(itemName);
				break;
			default:
				return;
		}

		if (xp <= 0)
		{
			Jotunn.Logger.LogDebug($"Failed to find a valid xp reward for client. Player Id: {playerID} | xpMul: {xpMultiplier} | iType: {itemType} | iName: {itemName}");
			return;
		}
		
		RewardXP(sender, playerID, xp * xpMultiplier, itemType);
	}

	private static void RewardXP(long playerPeerId, long playerId, int xpAmount, string itemType)
	{
		if (!ZNet.instance.IsServer()) {
			return;
		}

		var baseXpSpreadMin = Mathf.Min(1 - Main.ModConfig.BaseXpSpreadMin.Value / 100f, 1f);
		var baseXpSpreadMax = Mathf.Max(1 + Main.ModConfig.BaseXpSpreadMax.Value / 100f, 1f);
		var xpMultiplier = Mathf.Max(0f, Main.ModConfig.AllXpMultiplier.Value / 100f);
		var restedMultiplier = Mathf.Max(0f, Main.ModConfig.RestedXpMultiplier.Value / 100f);

		var xp = (int)(xpAmount * xpMultiplier * Random.Range(baseXpSpreadMin, baseXpSpreadMax));
		var restedBonusXp = (int)(xp * restedMultiplier);

		var newPackage = new ZPackage();
		newPackage.Write(playerId);
		newPackage.Write(xp);
		newPackage.Write(itemType);
		newPackage.Write(restedBonusXp);
		
		Jotunn.Logger.LogDebug($"Rewarding player with {xp:N0} xp");
		ModRpcRegistry.Instance.SendTargetRpc(RpcConstants.ClientAddExperience, newPackage, playerPeerId);
	}

	private static void RPC_RewardXPMonsters(long sender, ZPackage package)
	{
		var monsterID = package.ReadUInt();
		var monsterLevel = package.ReadUInt();
		var monsterName = package.ReadString();

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
				var baseXpSpreadMin = Mathf.Min(1 - Main.ModConfig.BaseXpSpreadMin.Value / 100f, 1f);
				var baseXpSpreadMax = Mathf.Max(1 + Main.ModConfig.BaseXpSpreadMax.Value / 100f, 1f);
				var monsterLvlMultiplier = Mathf.Max(0f, Main.ModConfig.MonsterLvlXpMultiplier.Value / 100f);
				var xpMultiplier = Mathf.Max(0f, Main.ModConfig.AllXpMultiplier.Value / 100f);
				var restedMultiplier = Mathf.Max(0f, Main.ModConfig.RestedXpMultiplier.Value / 100f);

				var awardedXP = CreaturesXpTable.GetXp(monsterName) * xpPercentage * Random.Range(baseXpSpreadMin, baseXpSpreadMax) * xpMultiplier;
				var monsterLevelBonusXp = (monsterLevel - 1) * monsterLvlMultiplier * awardedXP;
				var restedBonusXp = awardedXP * restedMultiplier;

				newPackage.Write((int)awardedXP);
				newPackage.Write((int)monsterLevelBonusXp);
				newPackage.Write((int)restedBonusXp);
				newPackage.Write(damage.playerID);
				newPackage.Write(monsterName);

				Jotunn.Logger.LogDebug($"Rewarding player '{damage.playerName}' with {awardedXP:N0} xp. Calculation: {(xpPercentage * 100f):N1}% xp = [Awarded XP: {awardedXP:N0}, Level bonus: {monsterLevelBonusXp:N0}, Rested bonus: {restedBonusXp:N0}]");
				ModRpcRegistry.Instance.SendTargetRpc(RpcConstants.ClientAddExperienceMonster, newPackage, damage.playerID);
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
