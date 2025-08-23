using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace CharacterProgressionMod.Core
{
    public sealed class ModConfig
    {
        private const string CustomFolder = "custom";
        private const string CategoriesFolderName = "categories";

        private readonly ConfigFile _configFile;

        public ModConfig(ConfigFile modConfigFile)
        {
            _configFile = modConfigFile;
            _configFile.SaveOnConfigSet = true;

            #region Initialize config entries

            // XP Bar
            ShowLevel = CreateConfigEntry("XP Bar", "showLevel", true, "Display Level text");
            ShowXp = CreateConfigEntry("XP Bar", "showXp", true, "Display XP text");
            ShowRequiredXp = CreateConfigEntry("XP Bar", "showRequiredXp", true,
                                               "Display XP required for next level. (ShowXP must be true) ");
            ShowPercentageXp =
                CreateConfigEntry("XP Bar", "showPercentageXP", true, "Display XP required for next level.");
            XpBarSize = CreateConfigEntry("XP Bar", "xpBarSize", 100f,
                                          "The width in percentage (%) of the default xp bar width. (100 = default size, 50 = half the size)");
            XpBarPosition = CreateConfigEntry("XP Bar", "xpBarPosition", new Vector2(0f, 0f),
                                              "The offset position in (x,y) coordinates, from its default position. (x: 0.0 = center of screen, y: 0.0 = bottom of screen, y: 950.0 = top of screen)");

            // Levels
            PointsPerLevel = CreateConfigEntry("Levels", "pointsPerLevel", 1f,
                                               "[ServerSync] The amount of skill points gained per level", true);

            // Skills Menu
            ShowScrollbar = CreateConfigEntry("Skills Menu", "showScrollbar", true,
                                              "Display the scroll bar. (Setting to false only disables the graphics, you can still keep scrolling)");
            AddMaxPointsKey = CreateConfigEntry("Skills Menu", "addMaxPointsKey", KeyCode.LeftControl,
                                                "By holding down this key, you will use as many points as you can on the skill.");
            AddMultiplePointsKey = CreateConfigEntry("Skills Menu", "addMultiplePointsKey", KeyCode.LeftShift,
                                                     "By holding down this key, you will use 'addMultiplePointsAmount' points on each click.");
            AddMultiplePointsAmount = CreateConfigEntry("Skills Menu", "addMultiplePointsAmount", 10,
                                                        "The amount of points used when holding down the 'addMultiplePointsKey' key");

            // VFX
            LevelUpVFX = CreateConfigEntry("VFX", "levelUpVFX", true, "Display visual effects when leveling up");
            CriticalHitVFX = CreateConfigEntry("VFX", "criticalHitVFX", true,
                                               "Display visual effects when dealing a critical hit");
            CriticalHitShake = CreateConfigEntry("VFX", "criticalHitShake", true,
                                                 "Shake the camera when dealing a critical hit");
            CriticalHitShakeIntensity =
                CreateConfigEntry("VFX", "criticalHitShakeIntensity", 2f, "Intensity of the camera shake");

            // XP Text
            DisplayXpInCorner = CreateConfigEntry("XP Text", "displayXPInCorner", true,
                                                  "Display XP gained in top left corner");
            DisplayXpFloatingText = CreateConfigEntry("XP Text", "displayXPFloatingText", true,
                                                      "Display XP gained as floating text");
            DisplayWoodcuttingXpText = CreateConfigEntry("XP Text", "displayWoodcuttingXPText", true,
                                                         "Display woodcutting XP gained as floating text");
            DisplayMiningXpText = CreateConfigEntry("XP Text", "displayMiningXPText", true,
                                                    "Display mining XP gained as floating text");
            DisplayPickupXpText = CreateConfigEntry("XP Text", "displayPickupXPText", true,
                                                    "Display pickup XP gained as floating text");
            DisplayMonsterXpText = CreateConfigEntry("XP Text", "displayMonsterXPText", true,
                                                     "Display monster XP gained as floating text");
            XpFontSize = CreateConfigEntry("XP Text", "xpFontSize", 100f,
                                           "The size  (in percentage) of the floating xp text. (100 = 100%, 50 = 50% etc.)");

            // XP Multipliers
            AllXpMultiplier = CreateConfigEntry("XP Multipliers", "XPMultipliers", 100f,
                                                "[ServerSync] XP gained (in percentage) compared to the Monster XP Table. (100 = Same as XP table, 150 = +50%, 70 = -30%)",
                                                true);
            MonsterLvlXpMultiplier = CreateConfigEntry("XP Multipliers", "monsterLvlXPMultiplier", 50f,
                                                       "[ServerSync] Bonus XP gained per monster level. (0 = No Bonus, 50 = +50% per level)",
                                                       true);
            RestedXpMultiplier = CreateConfigEntry("XP Multipliers", "restedXPMultiplier", 30f,
                                                   "[ServerSync] Bonus XP gained while rested. (0 = No Bonus, 30 = +30%)",
                                                   true);
            BaseXpSpreadMin = CreateConfigEntry("XP Multipliers", "baseXpSpreadMin", 5f,
                                                "[ServerSync] Base XP spread, Minimum. (0 = Same as XP table, 5 = -5% from XP table) Used to ensure that the same monster don't reward the exact same amount of XP every time.",
                                                true);
            BaseXpSpreadMax = CreateConfigEntry("XP Multipliers", "baseXpSpreadMax", 5f,
                                                "[ServerSync] Base XP spread, Maximum. (0 = Same as XP table, 5 = +5% from XP table) Used to ensure that the same monster don't reward the exact same amount of XP every time.",
                                                true);

            // Generate config entries for XP Tables
            // Pickables
            PickableXpEnabled = CreateConfigEntry("XP Table", "pickableXpEnabled", true,
                                                  "[ServerSync] Gain XP when interacting with Pickables", true);

            // Mining
            MiningXpEnabled = CreateConfigEntry("XP Table", "miningXpEnabled", true, "[ServerSync] Gain XP when mining",
                                                true);

            // Woodcutting
            WoodcuttingXpEnabled = CreateConfigEntry("XP Table", "woodcuttingXpEnabled", true,
                                                     "[ServerSync] Gain XP when chopping trees", true);

            #endregion
        }

        public static string CustomMiningDirectory => Path.Combine(CustomFolder, "mining");

        public static string CustomMiningCategoriesDirectory =>
            Path.Combine(CustomFolder, "mining", CategoriesFolderName);

        public static string CustomWoodcuttingDirectory => Path.Combine(CustomFolder, "woodcutting");

        public static string CustomWoodcuttingCategoriesDirectory =>
            Path.Combine(CustomFolder, "woodcutting", CategoriesFolderName);

        public static string CustomCreaturesDirectory => Path.Combine(CustomFolder, "creatures");
        public static string CustomPickablesDirectory => Path.Combine(CustomFolder, "pickables");

        public static string CustomPickablesCategoriesDirectory =>
            Path.Combine(CustomFolder, "pickables", CategoriesFolderName);

        public static string CustomPlayerDirectory => Path.Combine(CustomFolder, "player");

        public ConfigEntry<T> CreateConfigEntry<T>(string group, string name, T value, string description,
                                                   bool requiresAdminToChange = false)
        {
            var configAttributes = new ConfigurationManagerAttributes
                { IsAdminOnly = requiresAdminToChange };

            var configEntry =
                _configFile.Bind(group, name, value, new ConfigDescription(description, null, configAttributes));
            return configEntry;
        }

        #region Properties

        public ConfigEntry<float> DifficultyScalerStarRatio { get; }
        public ConfigEntry<bool> DisplayMiningXpText { get; }
        public ConfigEntry<bool> DisplayMonsterXpText { get; }
        public ConfigEntry<bool> DisplayPickupXpText { get; }
        public ConfigEntry<bool> DisplayWoodcuttingXpText { get; }
        public ConfigEntry<bool> DisplayXpFloatingText { get; }

        // XP Text
        public ConfigEntry<bool> DisplayXpInCorner { get; }

        public ConfigEntry<bool> EnableDifficultyScalerXp { get; }

        // VFX
        public ConfigEntry<bool> LevelUpVFX { get; }
        public ConfigEntry<bool> MiningXpEnabled { get; }
        public ConfigEntry<float> MonsterLvlXpMultiplier { get; }

        public ConfigEntry<bool> PickableXpEnabled { get; }

        public ConfigEntry<float> RestedXpMultiplier { get; }

        // Skills Menu
        public ConfigEntry<bool> ShowScrollbar { get; }
        public ConfigEntry<bool> WoodcuttingXpEnabled { get; }
        public ConfigEntry<float> XpFontSize { get; }
        public ConfigEntry<KeyCode> AddMaxPointsKey { get; }

        // XP Multipliers
        public ConfigEntry<float> AllXpMultiplier { get; }
        public ConfigEntry<float> BaseXpSpreadMax { get; }
        public ConfigEntry<float> BaseXpSpreadMin { get; }
        public ConfigEntry<bool> CriticalHitShake { get; }
        public ConfigEntry<float> CriticalHitShakeIntensity { get; }
        public ConfigEntry<bool> CriticalHitVFX { get; }
        public ConfigEntry<bool> DifficultyScalerBiome { get; }
        public ConfigEntry<float> DifficultyScalerBiomeRatio { get; }
        public ConfigEntry<bool> DifficultyScalerBoss { get; }
        public ConfigEntry<float> DifficultyScalerBossRatio { get; }
        public ConfigEntry<bool> DifficultyScalerNight { get; }
        public ConfigEntry<bool> DifficultyScalerOverallDamage { get; }
        public ConfigEntry<float> DifficultyScalerOverallDamageRatio { get; }
        public ConfigEntry<bool> DifficultyScalerStar { get; }
        public ConfigEntry<bool> DifficultyScalerOverallHealth { get; }
        public ConfigEntry<float> DifficultyScalerOverallHealthRatio { get; }

        // Config entries
        // -----------
        // XP Bar
        public ConfigEntry<bool> ShowLevel { get; }
        public ConfigEntry<bool> ShowXp { get; }
        public ConfigEntry<bool> ShowRequiredXp { get; }
        public ConfigEntry<bool> ShowPercentageXp { get; }
        public ConfigEntry<float> XpBarSize { get; }
        public ConfigEntry<Vector2> XpBarPosition { get; }

        // Levels
        public ConfigEntry<float> PointsPerLevel { get; }

        public ConfigEntry<float> DifficultyScalerNightRatio { get; }
        public ConfigEntry<KeyCode> AddMultiplePointsKey { get; }
        public ConfigEntry<int> AddMultiplePointsAmount { get; }

        #endregion
    }
}