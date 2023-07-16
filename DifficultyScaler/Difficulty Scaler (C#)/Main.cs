using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using ServerSync;
using System.Collections.Generic;
using UnityEngine;
using Cozyheim.API;
using System.Reflection;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;

namespace Cozyheim.DifficultyScaler
{
    [BepInPlugin(GUID, modName, version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency("spawner_tweaks", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Main : BaseUnityPlugin
    {
        // Mod information
        internal const string modName = "DifficultyScaler";
        internal const string version = "0.1.4";
        internal const string GUID = "dk.thrakal." + modName;

        private static Dictionary<string, float> _monsterHealth;
        private static Dictionary<string, float> _monsterDamage;
        
        // Core objects that is required to patch and configure the mod
        private readonly Harmony harmony = new Harmony(GUID);
        internal static ConfigSync configSync = new ConfigSync(GUID) { DisplayName = modName, CurrentVersion = version, MinimumRequiredVersion = version };
        internal static ConfigFile configFile;

        // Config entries
        private ConfigEntry<int> configTest;
        
        internal static ConfigEntry<bool> modEnabled;
        internal static ConfigEntry<bool> debugEnabled;
        internal static ConfigEntry<LogType> debugLevel;

        internal static ConfigEntry<float> overallHealthMultipler;
        internal static ConfigEntry<float> overallDamageMultipler;

        internal static ConfigEntry<string> monsterHealthModifier;
        internal static ConfigEntry<string> monsterDamageModifier;


        private static ConfigEntry<bool> enableBossKillDifficulty;
        private static ConfigEntry<float> bossKillMultiplier;
        private static ConfigEntry<string> bossGlobalKeys;

        private static ConfigEntry<bool> enableBiomeDifficulty;
        private static ConfigEntry<float> meadowsMultiplier;
        private static ConfigEntry<float> blackForestMultiplier;
        private static ConfigEntry<float> swampMultiplier;
        private static ConfigEntry<float> mountainMultiplier;
        private static ConfigEntry<float> plainsMultiplier;
        private static ConfigEntry<float> mistlandsMultiplier;
        private static ConfigEntry<float> oceanMultiplier;
        private static ConfigEntry<float> deepNorthMultiplier;
        private static ConfigEntry<float> ashlandsMultiplier;

        private static ConfigEntry<bool> enableNightDifficulty;
        internal static ConfigEntry<float> nightMultiplier;

        private static ConfigEntry<bool> enableStarDifficulty;
        internal static ConfigEntry<float> starMultiplier;

        internal static bool IsSpawnerTweaksInstalled { get; private set; }

        void Awake()
        {
            configFile = Config;/*new ConfigFile(Config.ConfigFilePath, true, Info.Metadata)
            { SaveOnConfigSet = true };*/
            configFile.SaveOnConfigSet = true;

            // Assigning config entries
            modEnabled = CreateConfigEntry("00_General", "ModEnabled", true, "Enable this mod", false);
            debugEnabled = CreateConfigEntry("00_General", "DebugEnabled", false, "Display debug messages in the console", true);
            debugLevel = CreateConfigEntry("00_General", "DebugLevel", LogType.Info, "The level of debug messages to display in the console (if DebugEnabled is enabled).", true);

            overallHealthMultipler = CreateConfigEntry("01_Monsters", "overallHealthMultipler", 0f, "Increases the base health of all monsters in the game. (0 = No change, 1.5 = +150% health).", true);
            overallDamageMultipler = CreateConfigEntry("01_Monsters", "overallDamageMultipler", 0f, "Increases the base damage of all monsters in the game. (0 = No change, 1.5 = +150% damage).", true);
            monsterHealthModifier = CreateConfigEntry("01_Monsters", "monsterBaseHealthModifier", "", "Set the base health (60 = 60 base health) of individual monsters. Format must follow: Monstername:Health (example: Skeleton:60,Greyling:25)", true);
            monsterDamageModifier = CreateConfigEntry("01_Monsters", "monsterDamageModifier", "", "Set a damage multiplier (110 = +10% increased damage) of individual monsters. Format must follow: Monstername:Damage (example: Skeleton:110,Greyling:120)", true);


            enableBossKillDifficulty = CreateConfigEntry("02_Boss", "enableBossKillDifficulty", false, "Enables difficulty scaling after killing bosses", true);
            bossKillMultiplier = CreateConfigEntry("02_Boss", "bossKillMultiplier", 0.5f, "Increases the base health & damage of all monsters by this value, per boss you have killed. (0 = no scaling, 0.5 = +50% health/damage per boss)", true);
            bossGlobalKeys = CreateConfigEntry("02_Boss", "bossGlobalKeys", "defeated_eikthyr, defeated_gdking, defeated_bonemass, defeated_dragon, defeated_goblinking, defeated_queen", "Global keys to check against for registering boss kills. Add custom boss keys to this list if you are using custom bosses.", true);


            enableBiomeDifficulty = CreateConfigEntry("03_Biomes", "enableBiomeDifficulty", false, "Enables difficulty scaling for biomes", true);
            meadowsMultiplier = CreateConfigEntry("03_Biomes", "meadowsMultiplier", 0f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            blackForestMultiplier = CreateConfigEntry("03_Biomes", "blackForestMultiplier", 0.5f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            swampMultiplier = CreateConfigEntry("03_Biomes", "swampMultiplier", 1f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            oceanMultiplier = CreateConfigEntry("03_Biomes", "oceanMultiplier", 1.25f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            mountainMultiplier = CreateConfigEntry("03_Biomes", "mountainMultiplier", 1.5f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            plainsMultiplier = CreateConfigEntry("03_Biomes", "plainsMultiplier", 2f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            mistlandsMultiplier = CreateConfigEntry("03_Biomes", "mistlandsMultiplier", 2.5f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            deepNorthMultiplier = CreateConfigEntry("03_Biomes", "deepNorthMultiplier", 3f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);
            ashlandsMultiplier = CreateConfigEntry("03_Biomes", "ashlandsMultiplier", 4f, "Increases the base health & damage of all monsters in this Biome (0 = no scaling, 0.5 = +50% health/damage)", true);

            enableNightDifficulty = CreateConfigEntry("04_Night", "enableNightDifficulty", false, "Enables difficulty scaling during night", true);
            nightMultiplier = CreateConfigEntry("04_Night", "nightMultiplier", 1f, "Increases the base health & damage of all monsters by this value during night. (0 = no scaling, 0.5 = +50% health/damage)", true);

            enableStarDifficulty = CreateConfigEntry("05_MonsterStars", "enableStarDifficulty", false, "Enables difficulty scaling for starred monsters. (Overrides vanilla health scaling!)", true);
            starMultiplier = CreateConfigEntry("05_MonsterStars", "starMultiplier", 1f, "Increases the base health & damage of all monsters by this value for each star they have. Vanilla value is 1. (0 = no scaling, 0.5 = +50% health/damage)", true);

            IsSpawnerTweaksInstalled = IsModInstalled("spawner_tweaks");
            
            if(!modEnabled.Value) {
                return;
            }

            harmony.PatchAll();
            CommandManager.Instance.AddConsoleCommand(new ConsoleLog());
            PrefabManager.OnVanillaPrefabsAvailable += CreateMonsterHealthDictionary;
        }

        public static bool IsModInstalled(string modGuid) => Chainloader.PluginInfos.Select(plugin => plugin.Value.Metadata).Any(metadata => metadata.GUID.Equals(modGuid));

        public static float GetBiomeMultiplier(Heightmap.Biome biome)
        {
            switch(biome) {
                case Heightmap.Biome.Meadows:
                    return meadowsMultiplier.Value;
                case Heightmap.Biome.BlackForest:
                    return blackForestMultiplier.Value;
                case Heightmap.Biome.Swamp:
                    return swampMultiplier.Value;
                case Heightmap.Biome.Mountain:
                    return mountainMultiplier.Value;
                case Heightmap.Biome.Plains:
                    return plainsMultiplier.Value;
                case Heightmap.Biome.AshLands:
                    return ashlandsMultiplier.Value;
                case Heightmap.Biome.DeepNorth:
                    return deepNorthMultiplier.Value;
                case Heightmap.Biome.Mistlands:
                    return mistlandsMultiplier.Value;
                case Heightmap.Biome.Ocean:
                    return oceanMultiplier.Value;
                default:
                    return 0.0f;
            }
        }

        public static bool IsMultiplierEnabled(DifficultyScalerMultiplier multiplierType)
        {
            switch (multiplierType) {
                case DifficultyScalerMultiplier.DamageMultiplier:
                    return true;
                case DifficultyScalerMultiplier.HealthMultiplier:
                    return true;
                case DifficultyScalerMultiplier.NightMultiplier:
                    return enableNightDifficulty.Value;
                case DifficultyScalerMultiplier.BossKillMultiplier:
                    return enableBossKillDifficulty.Value;
                case DifficultyScalerMultiplier.StarMultiplier:
                    return enableStarDifficulty.Value;
                case DifficultyScalerMultiplier.BiomeMultiplier:
                    return enableBiomeDifficulty.Value;
                default:
                    ConsoleLog.Print($"({nameof(IsMultiplierEnabled)}) Multiplier type {multiplierType} is not handled.", LogType.Warning);
                    return false;
            }
        }

        public static float GetCalculatedBossKillMultiplier()
        {
            string[] split = Main.bossGlobalKeys.Value.Split(',');
            int counter = 0;
            foreach(string key in split) {
                if(key != "") {
                    if(ZoneSystem.instance.GetGlobalKey(key.Trim())) {
                        counter++;
                    }
                }
            }
            
            return bossKillMultiplier.Value * counter;
        }
        
        public static bool TryGetMonsterBaseHealthOverride(string monsterName, out float baseHealthOverrideValue) =>
            _monsterHealth.TryGetValue(monsterName, out baseHealthOverrideValue);
        
        public static bool TryGetMonsterDamage(string monsterName, out float monsterDamageValue) =>
            _monsterDamage.TryGetValue(monsterName, out monsterDamageValue);

        public static void CreateMonsterHealthDictionary()
        {
            _monsterHealth = new Dictionary<string, float>();
            ConsoleLog.Print("-- Changing base HEALTH for monsters: --", LogType.Message);

            string[] monsterArray = monsterHealthModifier.Value.Split(',');
            for (int i = 0; i < monsterArray.Length; i++)
            {
                string[] monsterData = monsterArray[i].Split(':');
                if (monsterData.Length >= 2)
                {
                    if (float.TryParse(monsterData[1], out float health))
                    {
                        float originalHealth = PrefabManager.Instance.GetPrefab(monsterData[0]).GetComponent<Humanoid>().m_health;
                        _monsterHealth.Add(monsterData[0].Trim() + "(Clone)", health);
                        ConsoleLog.Print(monsterData[0].Trim() + ": (" + originalHealth + " -> " + health + " HP)");
                    }
                }
            }

            CreateMonsterDamageDictionary();
            PrefabManager.OnVanillaPrefabsAvailable -= CreateMonsterHealthDictionary;
        }

        public static void CreateMonsterDamageDictionary()
        {
            _monsterDamage = new Dictionary<string, float>();
            ConsoleLog.Print("-- Changing base DAMAGE for monsters: --", LogType.Message);

            string[] monsterArray = monsterDamageModifier.Value.Split(',');
            for (int i = 0; i < monsterArray.Length; i++)
            {
                string[] monsterData = monsterArray[i].Split(':');
                if (monsterData.Length >= 2)
                {
                    if (float.TryParse(monsterData[1], out float damage))
                    {
                        ConsoleLog.Print(monsterData[0].Trim() + ": " + damage + "% damage");
                        damage /= 100f;
                        _monsterDamage.Add(monsterData[0].Trim() + "(Clone)", damage);
                    }
                }
            }

            ConsoleLog.Print("-- Adjusting ALL monster's base damage and health: --", LogType.Message);
            ConsoleLog.Print("Base health set to: +" + (overallHealthMultipler.Value * 100).ToString("N0") + "%");
            ConsoleLog.Print("Base damage set to: +" + (overallDamageMultipler.Value * 100).ToString("N0") + "%");
        }

        void OnDestroy()
        {
            harmony.UnpatchSelf();
        }

        #region CreateConfigEntry Wrapper
        ConfigEntry<T> CreateConfigEntry<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> CreateConfigEntry<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => CreateConfigEntry(group, name, value, new ConfigDescription(description), synchronizedSetting);
        #endregion
    }
}
