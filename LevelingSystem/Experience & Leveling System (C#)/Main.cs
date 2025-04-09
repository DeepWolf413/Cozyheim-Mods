using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Cozyheim.LevelingSystem.Commands;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Cozyheim.LevelingSystem;

[BepInPlugin(GUID, modName, version)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency("dk.thrakal.DifficultyScaler", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
internal class Main : BaseUnityPlugin
{
    public enum Position
    {
        Above,
        Below
    }

    // Mod information
    internal const string modName = "LevelingSystem";
    internal const string version = "0.5.19";
    internal const string GUID = "dk.thrakal." + modName;

    internal static ConfigFile configFile;
    private readonly Harmony harmony = new (GUID);

    // Asset bundles
    internal static string assetsPath = "Assets/_Leveling System/";
    internal static AssetBundle assetBundle;

    // Check for other mods loaded
    internal static bool modDifficultyScalerLoaded;
    internal static bool modJewelcraftingLoaded;

    // Config entries
    // -----------
    // XP Bar
    internal static ConfigEntry<bool> showLevel;
    internal static ConfigEntry<bool> showXp;
    internal static ConfigEntry<bool> showRequiredXp;
    internal static ConfigEntry<bool> showPercentageXP;
    internal static ConfigEntry<float> xpBarSize;
    internal static ConfigEntry<Vector2> xpBarPosition;
    internal static ConfigEntry<Position> xpBarLevelTextPosition;

    // Levels
    internal static ConfigEntry<float> pointsPerLevel;

    // Skills Menu
    internal static ConfigEntry<bool> showScrollbar;
    internal static ConfigEntry<KeyCode> addMaxPointsKey;
    internal static ConfigEntry<KeyCode> addMultiplePointsKey;
    internal static ConfigEntry<int> addMultiplePointsAmount;

    // VFX
    internal static ConfigEntry<bool> levelUpVFX;
    internal static ConfigEntry<bool> criticalHitVFX;
    internal static ConfigEntry<bool> criticalHitShake;
    internal static ConfigEntry<float> criticalHitShakeIntensity;

    // XP Text
    internal static ConfigEntry<bool> displayXPInCorner;
    internal static ConfigEntry<bool> displayXPFloatingText;
    internal static ConfigEntry<bool> displayWoodcuttingXPText;
    internal static ConfigEntry<bool> displayMiningXPText;
    internal static ConfigEntry<bool> displayPickupXPText;
    internal static ConfigEntry<bool> displayMonsterXPText;
    internal static ConfigEntry<float> xpFontSize;

    // XP Table
    internal static ConfigEntry<string> monsterXpTable;
    internal static ConfigEntry<string> playerXpTable;

    internal static ConfigEntry<bool> pickableXpEnabled;
    internal static ConfigEntry<string> pickableXpTable;
    internal static ConfigEntry<bool> miningXpEnabled;
    internal static ConfigEntry<string> miningXpTable;
    internal static ConfigEntry<bool> woodcuttingXpEnabled;
    internal static ConfigEntry<string> woodcuttingXpTable;

    // XP Multipliers
    internal static ConfigEntry<float> allXPMultiplier;
    internal static ConfigEntry<float> monsterLvlXPMultiplier;
    internal static ConfigEntry<float> restedXPMultiplier;
    internal static ConfigEntry<float> baseXpSpreadMin;
    internal static ConfigEntry<float> baseXpSpreadMax;

    internal static ConfigEntry<bool> enableDifficultyScalerXP;
    internal static ConfigEntry<bool> difficultyScalerOverallHealth;
    internal static ConfigEntry<float> difficultyScalerOverallHealthRatio;
    internal static ConfigEntry<bool> difficultyScalerOverallDamage;
    internal static ConfigEntry<float> difficultyScalerOverallDamageRatio;
    internal static ConfigEntry<bool> difficultyScalerBiome;
    internal static ConfigEntry<float> difficultyScalerBiomeRatio;
    internal static ConfigEntry<bool> difficultyScalerBoss;
    internal static ConfigEntry<float> difficultyScalerBossRatio;
    internal static ConfigEntry<bool> difficultyScalerNight;
    internal static ConfigEntry<float> difficultyScalerNightRatio;
    internal static ConfigEntry<bool> difficultyScalerStar;
    internal static ConfigEntry<float> difficultyScalerStarRatio;

    private void Awake()
    {
        modDifficultyScalerLoaded = CheckIfModIsLoaded("dk.thrakal.DifficultyScaler");
        modJewelcraftingLoaded = CheckIfModIsLoaded("org.bepinex.plugins.jewelcrafting");
        configFile = Config;
        configFile.SaveOnConfigSet = true;

        harmony.PatchAll(Assembly.GetExecutingAssembly());

        // Asset Bundle loaded
        assetBundle = AssetUtils.LoadAssetBundleFromResources("leveling_system");
        PrefabManager.OnVanillaPrefabsAvailable += LoadAssets;
        
        // Assigning config entries
        // XP Bar
        showLevel = CreateConfigEntry("XP Bar", "showLevel", true, "Display Level text", false);
        showXp = CreateConfigEntry("XP Bar", "showXp", true, "Display XP text", false);
        showRequiredXp = CreateConfigEntry("XP Bar", "showRequiredXp", true,
            "Display XP required for next level. (ShowXP must be true) ", false);
        showPercentageXP = CreateConfigEntry("XP Bar", "showPercentageXP", true, "Display XP required for next level.",
            false);
        xpBarSize = CreateConfigEntry("XP Bar", "xpBarSize", 100f,
            "The width in percentage (%) of the default xp bar width. (100 = default size, 50 = half the size)", false);
        xpBarPosition = CreateConfigEntry("XP Bar", "xpBarPosition", new Vector2(0f, 0f),
            "The offset position in (x,y) coordinates, from its default position. (x: 0.0 = center of screen, y: 0.0 = bottom of screen, y: 950.0 = top of screen)",
            false);
        xpBarLevelTextPosition = CreateConfigEntry("XP Bar", "xpBarLevelTextPosition", Position.Above,
            "The position of the level text, relative to the xp bar.", false);

        // Levels
        pointsPerLevel = CreateConfigEntry("Levels", "pointsPerLevel", 1f,
            "[ServerSync] The amount of skill points gained per level", true);

        // Skills Menu
        showScrollbar = CreateConfigEntry("Skills Menu", "showScrollbar", true,
            "Display the scroll bar. (Setting to false only disables the graphics, you can still keep scrolling)",
            false);
        addMaxPointsKey = CreateConfigEntry("Skills Menu", "addMaxPointsKey", KeyCode.LeftControl,
            "By holding down this key, you will use as many points as you can on the skill.", false);
        addMultiplePointsKey = CreateConfigEntry("Skills Menu", "addMultiplePointsKey", KeyCode.LeftShift,
            "By holding down this key, you will use 'addMultiplePointsAmount' points on each click.", false);
        addMultiplePointsAmount = CreateConfigEntry("Skills Menu", "addMultiplePointsAmount", 10,
            "The amount of points used when holding down the 'addMultiplePointsKey' key", false);

        // VFX
        levelUpVFX = CreateConfigEntry("VFX", "levelUpVFX", true, "Display visual effects when leveling up", false);
        criticalHitVFX = CreateConfigEntry("VFX", "criticalHitVFX", true,
            "Display visual effects when dealing a critical hit", false);
        criticalHitShake = CreateConfigEntry("VFX", "criticalHitShake", true,
            "Shake the camera when dealing a critical hit", false);
        criticalHitShakeIntensity =
            CreateConfigEntry("VFX", "criticalHitShakeIntensity", 2f, "Intensity of the camera shake", false);

        // XP Text
        displayXPInCorner = CreateConfigEntry("XP Text", "displayXPInCorner", true,
            "Display XP gained in top left corner", false);
        displayXPFloatingText = CreateConfigEntry("XP Text", "displayXPFloatingText", true,
            "Display XP gained as floating text", false);
        displayWoodcuttingXPText = CreateConfigEntry("XP Text", "displayWoodcuttingXPText", true,
            "Display woodcutting XP gained as floating text", false);
        displayMiningXPText = CreateConfigEntry("XP Text", "displayMiningXPText", true,
            "Display mining XP gained as floating text", false);
        displayPickupXPText = CreateConfigEntry("XP Text", "displayPickupXPText", true,
            "Display pickup XP gained as floating text", false);
        displayMonsterXPText = CreateConfigEntry("XP Text", "displayMonsterXPText", true,
            "Display monster XP gained as floating text", false);
        xpFontSize = CreateConfigEntry("XP Text", "xpFontSize", 100f,
            "The size  (in percentage) of the floating xp text. (100 = 100%, 50 = 50% etc.)", false);

        // XP Multipliers
        allXPMultiplier = CreateConfigEntry("XP Multipliers", "XPMultipliers", 100f,
            "[ServerSync] XP gained (in percentage) compared to the Monster XP Table. (100 = Same as XP table, 150 = +50%, 70 = -30%)",
            true);
        monsterLvlXPMultiplier = CreateConfigEntry("XP Multipliers", "monsterLvlXPMultiplier", 50f,
            "[ServerSync] Bonus XP gained per monster level. (0 = No Bonus, 50 = +50% per level)", true);
        restedXPMultiplier = CreateConfigEntry("XP Multipliers", "restedXPMultiplier", 30f,
            "[ServerSync] Bonus XP gained while rested. (0 = No Bonus, 30 = +30%)", true);
        baseXpSpreadMin = CreateConfigEntry("XP Multipliers", "baseXpSpreadMin", 5f,
            "[ServerSync] Base XP spread, Minimum. (0 = Same as XP table, 5 = -5% from XP table) Used to ensure that the same monster don't reward the exact same amount of XP every time.",
            true);
        baseXpSpreadMax = CreateConfigEntry("XP Multipliers", "baseXpSpreadMax", 5f,
            "[ServerSync] Base XP spread, Maximum. (0 = Same as XP table, 5 = +5% from XP table) Used to ensure that the same monster don't reward the exact same amount of XP every time.",
            true);

        // Difficulty Scaler integration
        if (modDifficultyScalerLoaded) {
            enableDifficultyScalerXP = CreateConfigEntry("Difficulty Scaler", "enableDifficultyScalerXP", false,
                "[ServerSync] Enable Difficulty Scaler XP integration (Requires the Difficulty Scaler mod is installed)",
                true);

            difficultyScalerOverallHealth = CreateConfigEntry("Difficulty Scaler", "difficultyScalerOverallHealth",
                true, "[ServerSync] Use Difficulty Scaler's overall health difficulty multiplier", true);
            difficultyScalerOverallHealthRatio = CreateConfigEntry("Difficulty Scaler",
                "difficultyScalerOverallHealthRatio", 0.5f,
                "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling",
                true);

            difficultyScalerOverallDamage = CreateConfigEntry("Difficulty Scaler", "difficultyScalerOverallDamage",
                true, "[ServerSync] Use Difficulty Scaler's overall damage difficulty multiplier", true);
            difficultyScalerOverallDamageRatio = CreateConfigEntry("Difficulty Scaler",
                "difficultyScalerOverallDamageRatio", 0.5f,
                "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling",
                true);

            difficultyScalerBiome = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBiome", true,
                "[ServerSync] Use Difficulty Scaler's biome difficulty multiplier", true);
            difficultyScalerBiomeRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBiomeRatio", 1f,
                "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling",
                true);

            difficultyScalerBoss = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBoss", true,
                "[ServerSync] Use Difficulty Scaler's boss difficulty multiplier", true);
            difficultyScalerBossRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBossRatio", 1f,
                "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling",
                true);

            difficultyScalerNight = CreateConfigEntry("Difficulty Scaler", "difficultyScalerNight", true,
                "[ServerSync] Use Difficulty Scaler' night difficulty multiplier", true);
            difficultyScalerNightRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerNightRatio", 1f,
                "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling",
                true);

            difficultyScalerStar = CreateConfigEntry("Difficulty Scaler", "difficultyScalerStar", true,
                "[ServerSync] Use Difficulty Scaler's star difficulty multiplier", true);
            difficultyScalerStarRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerStarRatio", 1f,
                "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling",
                true);
        }

        SkillConfig.Init();

        // Generate config entries for XP Tables
        // Pickables
        pickableXpEnabled = CreateConfigEntry("XP Table", "pickableXpEnabled", true,
            "[ServerSync] Gain XP when interacting with Pickables", true);

        // Mining
        miningXpEnabled = CreateConfigEntry("XP Table", "miningXpEnabled", true, "[ServerSync] Gain XP when mining",
            true);

        // Woodcutting
        woodcuttingXpEnabled = CreateConfigEntry("XP Table", "woodcuttingXpEnabled", true,
            "[ServerSync] Gain XP when chopping trees", true);

        InitializeCommands();
        
        UIManager.Init();
        XPManager.Init();
    }
    
    
    private void OnDestroy()
    {
        harmony.UnpatchSelf();
    }

    private void InitializeCommands()
    {
        CommandManager.Instance.AddConsoleCommand(new SetLevelCommand());
        CommandManager.Instance.AddConsoleCommand(new LevelUpCommand());
    }

    private bool CheckIfModIsLoaded(string modGUID)
    {
        foreach (var plugin in Chainloader.PluginInfos) {
            var pluginData = plugin.Value.Metadata;
            if (pluginData.GUID.Equals(modGUID)) return true;
        }

        return false;
    }

    private void LoadAssets()
    {
        // Canvas UI with the XP Bar
        var levelSystem = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/LevelingSystemUI.prefab");
        levelSystem.AddComponent<UIManager>();
        levelSystem.AddComponent<SkillManager>();
        PrefabManager.Instance.AddPrefab(levelSystem);

        var xpText = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/XPText.prefab");
        xpText.AddComponent<XPText>();
        PrefabManager.Instance.AddPrefab(xpText);

        var critDamageText = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/CritDamageText.prefab");
        critDamageText.AddComponent<CritTextAnim>();
        PrefabManager.Instance.AddPrefab(critDamageText);

        var levelUpEffect = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/LevelUpEffectNew.prefab");
        PrefabManager.Instance.AddPrefab(levelUpEffect);

        var criticalHitEffect = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/CriticalHitEffect.prefab");
        PrefabManager.Instance.AddPrefab(criticalHitEffect);

        var skillUI = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/SkillUI.prefab");
        PrefabManager.Instance.AddPrefab(skillUI);

        var trainingDummy = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/LevelingDummy.prefab");
        PieceManager.Instance.AddPiece(new CustomPiece(trainingDummy, PieceTables.Hammer, false));

        var trainingDummyStrawman =
            assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/LevelingDummyStrawman.prefab");
        PieceManager.Instance.AddPiece(new CustomPiece(trainingDummyStrawman, PieceTables.Hammer, false));
        PrefabManager.OnVanillaPrefabsAvailable -= LoadAssets;
    }

    #region CreateConfigEntry Wrapper

    public static ConfigEntry<T> CreateConfigEntry<T>(string group, string name, T value, string description, bool requiresAdminToChange = false)
    {
        var configAttributes = new ConfigurationManagerAttributes
            { IsAdminOnly = requiresAdminToChange };

        var configEntry = configFile.Bind(group, name, value, new ConfigDescription(description, null, configAttributes));
        return configEntry;
    }

    #endregion
}
