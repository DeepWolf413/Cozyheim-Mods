using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using Cozyheim.LevelingSystem.Commands;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace Cozyheim.LevelingSystem;

[BepInPlugin(Guid, ModName, Version)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency("dk.thrakal.DifficultyScaler", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
public class Main : BaseUnityPlugin
{
    public enum Position
    {
        Above,
        Below
    }

    // Mod information
    public const string ModName = "LevelingSystem";
    public const string Version = "0.5.19";
    public const string Guid = "dk.thrakal." + ModName;
    public const string AssetsPath = "Assets/_Leveling System/";

    private readonly Harmony _harmony = new (Guid);
    
    public static AssetBundle AssetBundle { get; private set; }
    public static bool IsDifficultyScalerModLoaded { get; private set; }
    public static bool IsJewelcraftingModLoaded { get; private set; }

    internal static ModConfig ModConfig { get; private set; }

    private void Awake()
    {
        IsDifficultyScalerModLoaded = CheckIfModIsLoaded("dk.thrakal.DifficultyScaler");
        IsJewelcraftingModLoaded = CheckIfModIsLoaded("org.bepinex.plugins.jewelcrafting");
        ModConfig = new(Config);

        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        // Asset Bundle loaded
        AssetBundle = AssetUtils.LoadAssetBundleFromResources("leveling_system");
        PrefabManager.OnVanillaPrefabsAvailable += LoadAssets;
        
        SkillConfig.Init();

        InitializeCommands();
        
        UIManager.Init();
        XPManager.Init();
    }
    
    
    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
    }

    private void InitializeCommands()
    {
        CommandManager.Instance.AddConsoleCommand(new SetLevelCommand());
        CommandManager.Instance.AddConsoleCommand(new LevelUpCommand());
    }

    private bool CheckIfModIsLoaded(string modGuid)
    {
        foreach (var plugin in Chainloader.PluginInfos) {
            var pluginData = plugin.Value.Metadata;
            if (pluginData.GUID.Equals(modGuid)) return true;
        }

        return false;
    }

    private void LoadAssets()
    {
        // Canvas UI with the XP Bar
        var levelSystem = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelingSystemUI.prefab");
        levelSystem.AddComponent<UIManager>();
        levelSystem.AddComponent<SkillManager>();
        PrefabManager.Instance.AddPrefab(levelSystem);

        var xpText = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/XPText.prefab");
        xpText.AddComponent<XPText>();
        PrefabManager.Instance.AddPrefab(xpText);

        var critDamageText = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/CritDamageText.prefab");
        critDamageText.AddComponent<CritTextAnim>();
        PrefabManager.Instance.AddPrefab(critDamageText);

        var levelUpEffect = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelUpEffectNew.prefab");
        PrefabManager.Instance.AddPrefab(levelUpEffect);

        var criticalHitEffect = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/CriticalHitEffect.prefab");
        PrefabManager.Instance.AddPrefab(criticalHitEffect);

        var skillUI = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/SkillUI.prefab");
        PrefabManager.Instance.AddPrefab(skillUI);

        var trainingDummy = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelingDummy.prefab");
        PieceManager.Instance.AddPiece(new CustomPiece(trainingDummy, PieceTables.Hammer, false));

        var trainingDummyStrawman =
            AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelingDummyStrawman.prefab");
        PieceManager.Instance.AddPiece(new CustomPiece(trainingDummyStrawman, PieceTables.Hammer, false));
        PrefabManager.OnVanillaPrefabsAvailable -= LoadAssets;
    }
}
