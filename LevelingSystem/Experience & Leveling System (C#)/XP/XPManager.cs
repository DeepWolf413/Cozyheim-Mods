using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cozyheim.LevelingSystem
{
    internal class XPManager : MonoBehaviour
    {
        private static string saveLevelString = "CozyLevel";
        private static string saveXpString = "CozyXP";

        private List<MonsterXP> xpObjects = new List<MonsterXP>();

        // Network communication RPC
        public static CustomRPC rpc_AddMonsterDamage = null;
        public static CustomRPC rpc_RewardXPMonster = null;
        public static CustomRPC rpc_RewardXP = null;
        public static CustomRPC rpc_GetXP = null;

        private static XPManager _instance;
        public static XPManager Instance {
            get {
                if (_instance == null)
                {
                    _instance = new GameObject("XPManager").AddComponent<XPManager>();
                }
                return _instance;
            }
        }

        public static void Init()
        {
            // Register RPC Methods
            rpc_AddMonsterDamage = NetworkManager.Instance.AddRPC("AddMonsterDamage", RPC_AddMonsterDamage, RPC_AddMonsterDamage);
            rpc_RewardXPMonster = NetworkManager.Instance.AddRPC("RewardXPMonster", RPC_RewardXPMonsters, RPC_RewardXPMonsters);
            rpc_RewardXP = NetworkManager.Instance.AddRPC("RewardXP", RPC_RewardXP, RPC_RewardXP);
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
                yield break;
            }

            uint monsterID = package.ReadUInt();
            long playerID = package.ReadLong();
            float damage = package.ReadSingle();
            string playerName = package.ReadString();

            MonsterXP obj = Instance.GetMonsterXP(monsterID);
            if (obj != null)
            {
                ConsoleLog.Print("Updated monster damage (Server)");
                obj.AddDamage(playerID, damage, playerName);
            }
            else
            {
                ConsoleLog.Print("Created new monster damage (Server)");
                MonsterXP newObj = Instance.CreateNewMonsterXP(monsterID);
                newObj.AddDamage(playerID, damage, playerName);
            }

            yield return null;
        }

        public void AddMonsterDamage(Character monster, Character player, float damage)
        {
            uint monsterID = monster.GetZDOID().ID;
            long playerID = player.GetComponent<Player>().GetPlayerID();
            string playerName = player.GetComponent<Player>().GetPlayerName();

            ZPackage newPackage = new ZPackage();
            newPackage.Write(monsterID);
            newPackage.Write(playerID);
            newPackage.Write(damage);
            newPackage.Write(playerName);

            ConsoleLog.Print("Sending damage to server RPC");
            rpc_AddMonsterDamage.SendPackage(ZRoutedRpc.Everybody, newPackage);
        }

        private MonsterXP CreateNewMonsterXP(uint monsterID)
        {
            MonsterXP newObj = new MonsterXP(monsterID);
            xpObjects.Add(newObj);

            return newObj;
        }

        public void GetXPFromServer(long playerID, string itemName, string itemType, int xpMultiplier = 1) {
            ConsoleLog.Print("Trying to get XP from server (" + itemName + " - " + itemType + " - " + xpMultiplier + ")");
            ZPackage newPackage = new ZPackage();
            newPackage.Write(playerID);
            newPackage.Write(itemName);
            newPackage.Write(itemType);
            newPackage.Write(xpMultiplier);
            rpc_GetXP.SendPackage(ZRoutedRpc.Everybody, newPackage);
        }

        private static IEnumerator RPC_GetXPFromServer(long sender, ZPackage package) {
            if (!ZNet.instance.IsServer()) {
                yield break;
            }

            long playerID = package.ReadLong();
            string itemName = package.ReadString();
            string itemType = package.ReadString();
            int xpMultiplier = package.ReadInt();

            ConsoleLog.Print("Server: Recieved GetXP Call (" + itemName + " - " + itemType + " - " + xpMultiplier + ")");

            int xp;
            switch(itemType) {
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

            if(xp <= 0) {
                yield break;
            }

            ConsoleLog.Print("Server: Found XP = " + xp);

            ZPackage newPackage = new ZPackage();
            newPackage.Write(playerID);
            newPackage.Write(xp * xpMultiplier);

            rpc_RewardXP.SendPackage(ZRoutedRpc.Everybody, newPackage);
        }

        private static IEnumerator RPC_RewardXP(long sender, ZPackage package)
        {
            if (!ZNet.instance.IsServer())
            {
                yield break;
            }

            ZPackage newPackage = new ZPackage();
            long playerID = package.ReadLong();
            int xpAmount = package.ReadInt();

            float baseXpSpreadMin = Mathf.Min(1 - (Main.baseXpSpreadMin.Value / 100f), 1f);
            float baseXpSpreadMax = Mathf.Max(1 + (Main.baseXpSpreadMax.Value / 100f), 1f);
            float xpMultiplier = Mathf.Max(0f, Main.allXPMultiplier.Value / 100f);

            int xp = (int)(xpAmount * xpMultiplier * UnityEngine.Random.Range(baseXpSpreadMin, baseXpSpreadMax));

            newPackage.Write(playerID);
            newPackage.Write(xp);

            ConsoleLog.Print("Server: Sending XP to Player (XP: " + xp);

            UIManager.rpc_AddExperience.SendPackage(ZRoutedRpc.Everybody, newPackage);
        } 

        private static IEnumerator RPC_RewardXPMonsters(long sender, ZPackage package)
        {
            if (!ZNet.instance.IsServer())
            {
                yield break;
            }

            uint monsterID = package.ReadUInt();
            uint monsterLevel = package.ReadUInt();
            string monsterName = package.ReadString();

            ConsoleLog.Print("Monster died (Server) - " + monsterName);

            MonsterXP monsterObj = Instance.GetMonsterXP(monsterID);
            if (monsterObj != null)
            {
                float totalDamage = monsterObj.GetTotalDamageDealt();

                float dsHealthMultiplier = 0f;
                float dsDamageMultiplier = 0f;
                float dsBiomeMultiplier = 0f;
                float dsNightMultiplier = 0f;
                float dsBossKillMultiplier = 0f;
                float dsStarMultiplier = 0f;

                bool dsFound = package.ReadBool();

                if(dsFound) {
                    dsHealthMultiplier = package.ReadSingle();
                    dsDamageMultiplier = package.ReadSingle();
                    dsBiomeMultiplier = package.ReadSingle();
                    dsNightMultiplier = package.ReadSingle();
                    dsBossKillMultiplier = package.ReadSingle();
                    dsStarMultiplier = package.ReadSingle();
                }

                // Find the correct monster in the list
                foreach (PlayerDamage damage in monsterObj.playerDamages)
                {
                    ZPackage newPackage = new ZPackage();

                    // Get the percentage of damage the player has dealt
                    float xpPercentage = damage.playerTotalDamage / totalDamage;

                    // Reward with xp based on monster type killed
                    float baseXpSpreadMin = Mathf.Min(1 - (Main.baseXpSpreadMin.Value / 100f), 1f);
                    float baseXpSpreadMax = Mathf.Max(1 + (Main.baseXpSpreadMax.Value / 100f), 1f);
                    float monsterLvlMultiplier = Mathf.Max(0f, Main.monsterLvlXPMultiplier.Value / 100f);
                    float xpMultiplier = Mathf.Max(0f, Main.allXPMultiplier.Value / 100f);
                    float restedMultiplier = Mathf.Max(0f, Main.restedXPMultiplier.Value / 100f);

                    float awardedXP = XPTable.GetMonsterXP(monsterName) * xpPercentage * UnityEngine.Random.Range(baseXpSpreadMin, baseXpSpreadMax) * xpMultiplier;

                    // Apply difficulty scaler xp
                    if(dsFound && Main.modDifficultyScalerLoaded) {
                        if(Main.enableDifficultyScalerXP.Value) {
                            float dsHealthBonus = dsHealthMultiplier * Main.difficultyScalerOverallHealthRatio.Value;
                            float dsDamageBonus = dsDamageMultiplier * Main.difficultyScalerOverallDamageRatio.Value;
                            float dsBiomeBonus = dsBiomeMultiplier * Main.difficultyScalerBiomeRatio.Value;
                            float dsNightBonus = dsNightMultiplier * Main.difficultyScalerBossRatio.Value;
                            float dsBossBonus = dsBossKillMultiplier * Main.difficultyScalerBossRatio.Value;
                            float dsStarBonus = dsStarMultiplier * Main.difficultyScalerStarRatio.Value;

                            float totalBonusMultiplier = 0f;

                            ConsoleLog.Print("XP before scaling: " + awardedXP);

                            if(Main.difficultyScalerOverallHealth.Value) {
                                totalBonusMultiplier += dsHealthBonus;
                            }

                            if(Main.difficultyScalerOverallDamage.Value) {
                                totalBonusMultiplier += dsDamageBonus;
                            }

                            if(Main.difficultyScalerBiome.Value) {
                                totalBonusMultiplier += dsBiomeBonus;
                            }

                            if(Main.difficultyScalerNight.Value) {
                                totalBonusMultiplier += dsNightBonus;
                            }

                            if(Main.difficultyScalerBoss.Value) {
                                totalBonusMultiplier += dsBossBonus;
                            }

                            if(Main.difficultyScalerStar.Value) {
                                totalBonusMultiplier += dsStarBonus;
                            }

                            awardedXP *= totalBonusMultiplier + 1f;

                            ConsoleLog.Print($"XP scaled with { (totalBonusMultiplier * 100f).ToString("N0") }%: " + awardedXP);
                        }
                    }

                    float monsterLevelBonusXp = (monsterLevel - 1) * monsterLvlMultiplier * awardedXP;
                    float restedBonusXp = awardedXP * restedMultiplier;

                    newPackage.Write((int)awardedXP);
                    newPackage.Write((int)monsterLevelBonusXp);
                    newPackage.Write((int)restedBonusXp);
                    newPackage.Write(damage.playerID);
                    newPackage.Write(monsterName);


                    ConsoleLog.Print("Sending " + (xpPercentage * 100f).ToString("N1") + "% xp to " + damage.playerName + ". (Awarded: " + (int)awardedXP + ", Level bonus: " + (int)monsterLevelBonusXp + ", Rested bonus: " + (int)restedBonusXp + ")");

                    UIManager.rpc_AddExperienceMonster.SendPackage(ZRoutedRpc.Everybody, newPackage);
                }

                Instance.xpObjects.Remove(monsterObj);
            }
        }

        public void RewardXP(Character monster)
        {
            ConsoleLog.Print("Monster died (Client)");

            ZPackage newPackage = new ZPackage();

            newPackage.Write(monster.GetInstanceID());
            newPackage.Write(monster.GetLevel());
            newPackage.Write(monster.name);

            rpc_RewardXPMonster.SendPackage(ZRoutedRpc.Everybody, newPackage);
        }

        private MonsterXP GetMonsterXP(uint monsterID)
        {
            foreach (MonsterXP obj in xpObjects)
            {
                if (obj.monsterID == monsterID)
                {
                    return obj;
                }
            }

            return null;
        }

        public string GetAllMonsterXpString()
        {
            string response = "Total monsters: " + xpObjects.Count;
            foreach (MonsterXP obj in xpObjects)
            {
                response += "\n-> MonsterID: " + obj.monsterID;
            }

            return response;
        }

        public void SetPlayerLevel(int level)
        {
            Player.m_localPlayer.m_customData[saveLevelString] = level.ToString();
        }

        public void SavePlayerLevel()
        {
            Player.m_localPlayer.m_customData[saveLevelString] = UIManager.Instance.playerLevel.ToString();
        }

        public void SetPlayerXP(int xp)
        {
            Player.m_localPlayer.m_customData[saveXpString] = xp.ToString();
        }

        public void SavePlayerXP()
        {
            Player.m_localPlayer.m_customData[saveXpString] = UIManager.Instance.playerXP.ToString();
        }

        public int GetPlayerLevel()
        {
            int value = 1;
            if(Player.m_localPlayer.m_customData.ContainsKey(saveLevelString))
            {
                string savedString = Player.m_localPlayer.m_customData[saveLevelString];
                int.TryParse(savedString, out value);
            }

            return value;
        }

        public int GetPlayerXP()
        {
            int value = 0;
            if (Player.m_localPlayer.m_customData.ContainsKey(saveXpString))
            {
                string savedString = Player.m_localPlayer.m_customData[saveXpString];
                int.TryParse(savedString, out value);
            }

            return value;
        }

    }
}
