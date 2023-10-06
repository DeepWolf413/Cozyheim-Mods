using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using ServerSync;
using UnityEngine;

namespace Cozyheim.LevelingSystem;

[BepInPlugin(GUID, modName, version)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency("randyknapp.mods.auga", BepInDependency.DependencyFlags.SoftDependency)]
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
	internal const string version = "0.5.9";
	internal const string GUID = "dk.thrakal." + modName;

	internal static ConfigSync configSync = new(GUID)
	{ DisplayName = modName, CurrentVersion = version, MinimumRequiredVersion = version };

	internal static ConfigFile configFile;

	// Asset bundles
	internal static string assetsPath = "Assets/_Leveling System/";
	internal static AssetBundle assetBundle;

	// Check for other mods loaded
	internal static bool modAugaLoaded;
	internal static bool modDifficultyScalerLoaded;
	internal static bool modJewelcraftingLoaded;

	// Config entries
	// -----------

	// General
	internal static ConfigEntry<bool> modEnabled;
	internal static ConfigEntry<bool> debugEnabled;
	internal static ConfigEntry<bool> debugMonsterInternalName;

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


	// Auga integration
	internal static ConfigEntry<bool> useAugaBuildMenuUI;

	internal static ConfigEntry<int> nexusID;

	// Core objects that is required to patch and configure the mod
	private readonly Harmony harmony = new(GUID);

	private void Awake()
	{
		AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

		modAugaLoaded = CheckIfModIsLoaded("randyknapp.mods.auga");
		modDifficultyScalerLoaded = CheckIfModIsLoaded("dk.thrakal.DifficultyScaler");
		modJewelcraftingLoaded = CheckIfModIsLoaded("org.bepinex.plugins.jewelcrafting");
		harmony.PatchAll();
		configFile = Config; /*new ConfigFile(Config.ConfigFilePath, true, Info.Metadata)
            { SaveOnConfigSet = true };*/
		configFile.SaveOnConfigSet = true;

		// Asset Bundle loaded
		assetBundle = GetAssetBundleFromResources("leveling_system");
		PrefabManager.OnVanillaPrefabsAvailable += LoadAssets;

		// Assigning config entries
		modEnabled = CreateConfigEntry("General", "modEnabled", true, "[ServerSync] Enable this mod", true, true);
		debugEnabled = CreateConfigEntry("General", "debugEnabled", false, "Display debug messages in the console", false);
		debugMonsterInternalName = CreateConfigEntry("General", "debugMonsterInternalName", false, "Display the internal ID (prefab name) of monsters in the console, when you hit them", false);

		// Nexus ID
		nexusID = Config.Bind("General", "NexusID", 2282, "Nexus mod ID for updates");

		// XP Bar
		showLevel = CreateConfigEntry("XP Bar", "showLevel", true, "Display Level text", false);
		showXp = CreateConfigEntry("XP Bar", "showXp", true, "Display XP text", false);
		showRequiredXp = CreateConfigEntry("XP Bar", "showRequiredXp", true, "Display XP required for next level. (ShowXP must be true) ", false);
		showPercentageXP = CreateConfigEntry("XP Bar", "showPercentageXP", true, "Display XP required for next level.", false);
		xpBarSize = CreateConfigEntry("XP Bar", "xpBarSize", 100f, "The width in percentage (%) of the default xp bar width. (100 = default size, 50 = half the size)", false);
		xpBarPosition = CreateConfigEntry("XP Bar", "xpBarPosition", new Vector2(0f, 0f), "The offset position in (x,y) coordinates, from its default position. (x: 0.0 = center of screen, y: 0.0 = bottom of screen, y: 950.0 = top of screen)", false);
		xpBarLevelTextPosition = CreateConfigEntry("XP Bar", "xpBarLevelTextPosition", Position.Above, "The position of the level text, relative to the xp bar.", false);

		// Levels
		pointsPerLevel = CreateConfigEntry("Levels", "pointsPerLevel", 1f, "[ServerSync] The amount of skill points gained per level", true, true);

		// Skills Menu
		showScrollbar = CreateConfigEntry("Skills Menu", "showScrollbar", true, "Display the scroll bar. (Setting to false only disables the graphics, you can still keep scrolling)", false);
		addMaxPointsKey = CreateConfigEntry("Skills Menu", "addMaxPointsKey", KeyCode.LeftControl, "By holding down this key, you will use as many points as you can on the skill.", false);
		addMultiplePointsKey = CreateConfigEntry("Skills Menu", "addMultiplePointsKey", KeyCode.LeftShift, "By holding down this key, you will use 'addMultiplePointsAmount' points on each click.", false);
		addMultiplePointsAmount = CreateConfigEntry("Skills Menu", "addMultiplePointsAmount", 10, "The amount of points used when holding down the 'addMultiplePointsKey' key", false);

		// VFX
		levelUpVFX = CreateConfigEntry("VFX", "levelUpVFX", true, "Display visual effects when leveling up", false);
		criticalHitVFX = CreateConfigEntry("VFX", "criticalHitVFX", true, "Display visual effects when dealing a critical hit", false);
		criticalHitShake = CreateConfigEntry("VFX", "criticalHitShake", true, "Shake the camera when dealing a critical hit", false);
		criticalHitShakeIntensity = CreateConfigEntry("VFX", "criticalHitShakeIntensity", 2f, "Intensity of the camera shake", false);

		// XP Text
		displayXPInCorner = CreateConfigEntry("XP Text", "displayXPInCorner", true, "Display XP gained in top left corner", false);
		displayXPFloatingText = CreateConfigEntry("XP Text", "displayXPFloatingText", true, "Display XP gained as floating text", false);
		displayWoodcuttingXPText = CreateConfigEntry("XP Text", "displayWoodcuttingXPText", true, "Display woodcutting XP gained as floating text", false);
		displayMiningXPText = CreateConfigEntry("XP Text", "displayMiningXPText", true, "Display mining XP gained as floating text", false);
		displayPickupXPText = CreateConfigEntry("XP Text", "displayPickupXPText", true, "Display pickup XP gained as floating text", false);
		displayMonsterXPText = CreateConfigEntry("XP Text", "displayMonsterXPText", true, "Display monster XP gained as floating text", false);
		xpFontSize = CreateConfigEntry("XP Text", "xpFontSize", 100f, "The size  (in percentage) of the floating xp text. (100 = 100%, 50 = 50% etc.)", false);

		// XP Multipliers
		allXPMultiplier = CreateConfigEntry("XP Multipliers", "XPMultipliers", 100f, "[ServerSync] XP gained (in percentage) compared to the Monster XP Table. (100 = Same as XP table, 150 = +50%, 70 = -30%)", true, true);
		monsterLvlXPMultiplier = CreateConfigEntry("XP Multipliers", "monsterLvlXPMultiplier", 50f, "[ServerSync] Bonus XP gained per monster level. (0 = No Bonus, 50 = +50% per level)", true, true);
		restedXPMultiplier = CreateConfigEntry("XP Multipliers", "restedXPMultiplier", 30f, "[ServerSync] Bonus XP gained while rested. (0 = No Bonus, 30 = +30%)", true, true);
		baseXpSpreadMin = CreateConfigEntry("XP Multipliers", "baseXpSpreadMin", 5f, "[ServerSync] Base XP spread, Minimum. (0 = Same as XP table, 5 = -5% from XP table) Used to ensure that the same monster don't reward the exact same amount of XP every time.", true, true);
		baseXpSpreadMax = CreateConfigEntry("XP Multipliers", "baseXpSpreadMax", 5f, "[ServerSync] Base XP spread, Maximum. (0 = Same as XP table, 5 = +5% from XP table) Used to ensure that the same monster don't reward the exact same amount of XP every time.", true, true);

		// Auga integration
		useAugaBuildMenuUI = CreateConfigEntry("Auga Compatibility", "useAugaBuildMenuUI", true, "Using the Auga build menu HUD. Fixes compatibility issues. MUST be the same value as inthe Auga config. (Only required if you have Auga installed)", false);

		// Difficulty Scaler integration
		if (modDifficultyScalerLoaded) {
			enableDifficultyScalerXP = CreateConfigEntry("Difficulty Scaler", "enableDifficultyScalerXP", false, "[ServerSync] Enable Difficulty Scaler XP integration (Requires the Difficulty Scaler mod is installed)", true, true);

			difficultyScalerOverallHealth = CreateConfigEntry("Difficulty Scaler", "difficultyScalerOverallHealth", true, "[ServerSync] Use Difficulty Scaler's overall health difficulty multiplier", true, true);
			difficultyScalerOverallHealthRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerOverallHealthRatio", 0.5f, "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling", true, true);

			difficultyScalerOverallDamage = CreateConfigEntry("Difficulty Scaler", "difficultyScalerOverallDamage", true, "[ServerSync] Use Difficulty Scaler's overall damage difficulty multiplier", true, true);
			difficultyScalerOverallDamageRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerOverallDamageRatio", 0.5f, "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling", true, true);

			difficultyScalerBiome = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBiome", true, "[ServerSync] Use Difficulty Scaler's biome difficulty multiplier", true, true);
			difficultyScalerBiomeRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBiomeRatio", 1f, "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling", true, true);

			difficultyScalerBoss = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBoss", true, "[ServerSync] Use Difficulty Scaler's boss difficulty multiplier", true, true);
			difficultyScalerBossRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerBossRatio", 1f, "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling", true, true);

			difficultyScalerNight = CreateConfigEntry("Difficulty Scaler", "difficultyScalerNight", true, "[ServerSync] Use Difficulty Scaler' night difficulty multiplier", true, true);
			difficultyScalerNightRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerNightRatio", 1f, "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling", true, true);

			difficultyScalerStar = CreateConfigEntry("Difficulty Scaler", "difficultyScalerStar", true, "[ServerSync] Use Difficulty Scaler's star difficulty multiplier", true, true);
			difficultyScalerStarRatio = CreateConfigEntry("Difficulty Scaler", "difficultyScalerStarRatio", 1f, "[ServerSync] The ratio of the scaling multiplier that is applied as XP. (1 = the same as difficulty scaler, 0.5 = 50% of the scaling, 2 = 200% of the scaling", true, true);
		}

		SkillConfig.Init();

		// Generate config entries for XP Tables

		// Player
		playerXpTable = CreateConfigEntry("XP Table", "playerXpTable", "", "(Obsolete! - Change the JSON file in the config folder instead) The xp needed for each level. To reach a higher max level, simply add more values to the table. (Changes requires to reload the config file, which can be done in two ways. 1. Restart the server.  -  2. Admins can open the console in-game and type LevelingSystem ReloadConfig)");

		// Monsters
		monsterXpTable = CreateConfigEntry("XP Table", "monsterXpTable", "", "(Obsolete! - Change the JSON file in the config folder instead) The base xp of monsters. (Changes requires to realod the config file)");

		// Pickables
		pickableXpEnabled = CreateConfigEntry("XP Table", "pickableXpEnabled", true, "[ServerSync] Gain XP when interacting with Pickables", true, true);
		pickableXpTable = CreateConfigEntry("XP Table", "pickableXpTable", "", "(Obsolete! - Change the JSON file in the config folder instead) The base xp of pickables. (Changes requires to reload the config file)");

		// Mining
		miningXpEnabled = CreateConfigEntry("XP Table", "miningXpEnabled", true, "[ServerSync] Gain XP when mining", true, true);
		miningXpTable = CreateConfigEntry("XP Table", "miningXpTable", "", "(Obsolete! - Change the JSON file in the config folder instead) The base xp for mining. (Changes requires to reload the config file)");

		// Woodcutting
		woodcuttingXpEnabled = CreateConfigEntry("XP Table", "woodcuttingXpEnabled", true, "[ServerSync] Gain XP when chopping trees", true, true);
		woodcuttingXpTable = CreateConfigEntry("XP Table", "woodcuttingXpTable", "", "(Obsolete! - Change the JSON file in the config folder instead) The base xp for woodcutting. (Changes requires to reload the config file)");


		CommandManager.Instance.AddConsoleCommand(new ConsoleLog());
		ConsoleLog.Init();

		NetworkHandler.Init();
		UIManager.Init();
		XPManager.Init();

		ConsoleLog.Print("Auga loaded: " + modAugaLoaded, LogType.Warning);
	}

	private void OnDestroy() { harmony.UnpatchSelf(); }

	private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		var baseResourceName = Assembly.GetExecutingAssembly().GetName().Name + "." + new AssemblyName(args.Name).Name;
		byte[] assemblyData = null;
		byte[] symbolsData = null;
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(baseResourceName + ".dll")) {
			if (stream == null)
				return null;

			assemblyData = new byte[stream.Length];
			stream.Read(assemblyData, 0, assemblyData.Length);
		}

		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(baseResourceName + ".pdb")) {
			if (stream != null) {
				symbolsData = new byte[stream.Length];
				stream.Read(symbolsData, 0, symbolsData.Length);
			}
		}

		var assembly = Assembly.Load(assemblyData, symbolsData);
		ConsoleLog.Print("Assembly loaded: " + (assembly == null));

		return assembly;
	}

	private string AddNewEntriesToXPTable(Dictionary<string, int> xpTable, string configEntry)
	{
		var configXPTable = new Dictionary<string, int>();

		var entries = configEntry.Split(',');
		foreach (var entry in entries) {
			var entryData = entry.Split(':');
			if (entryData.Length == 2) {
				var key = entryData[0].Trim();
				var value = 0;
				if (int.TryParse(entryData[1].Trim(), out value)) configXPTable.Add(key, value);
			}
		}

		foreach (var kvp in xpTable)
			if (!configXPTable.ContainsKey(kvp.Key))
				configXPTable.Add(kvp.Key, kvp.Value);

		return GenerateXPTableString(configXPTable);
	}

	private string GenerateXPTableString(Dictionary<string, int> xpTable)
	{
		var counter = 0;
		var returnValue = "";
		foreach (var kvp in xpTable) {
			returnValue += counter != 0 ? ", " : "";
			returnValue += kvp.Key + ":" + kvp.Value;
			counter++;
		}

		return returnValue;
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
		PieceManager.Instance.AddPiece(new CustomPiece(trainingDummy, "Hammer", false));

		var trainingDummyStrawman = assetBundle.LoadAsset<GameObject>(assetsPath + "Prefabs/LevelingDummyStrawman.prefab");
		PieceManager.Instance.AddPiece(new CustomPiece(trainingDummyStrawman, "Hammer", false));

		PrefabManager.OnVanillaPrefabsAvailable -= LoadAssets;
	}

	public static AssetBundle GetAssetBundleFromResources(string fileName)
	{
		var execAssembly = Assembly.GetExecutingAssembly();

		var resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));

		using (var stream = execAssembly.GetManifestResourceStream(resourceName)) {
			return AssetBundle.LoadFromStream(stream);
		}
	}

	public static Sprite GetSpriteFromResources(string filePath)
	{
		Texture2D texture = null;
		byte[] data;

		data = File.ReadAllBytes(filePath);
		texture = new Texture2D(2, 2);
		texture.SetPixelData(data, 0);

		texture.LoadImage(data);

		var newSprite = Sprite.Create(texture, new Rect(0.5f, 0.5f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);

		return newSprite;
	}


	#region CreateConfigEntry Wrapper

	public static ConfigEntry<T> CreateConfigEntry<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
	{
		var configEntry = configFile.Bind(group, name, value, description);

		var syncedConfigEntry = configSync.AddConfigEntry(configEntry);
		syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

		return configEntry;
	}

	public static ConfigEntry<T> CreateConfigEntry<T>(string group, string name, T value, string description, bool synchronizedSetting = true, bool requiresAdminToChange = false)
	{
		var configAttributes = new ConfigurationManagerAttributes
		{ IsAdminOnly = requiresAdminToChange };

		return CreateConfigEntry(group, name, value, new ConfigDescription(description, null, configAttributes), synchronizedSetting);
	}

	#endregion
}