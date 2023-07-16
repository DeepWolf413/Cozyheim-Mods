using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using ServerSync;
using UnityEngine;

namespace Cozyheim.MiningAOE {
    [BepInPlugin(GUID, modName, version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Main : BaseUnityPlugin {
        // Mod information
        internal const string modName = "MiningAOE";
        internal const string version = "0.0.1";
        internal const string GUID = "dk.thrakal." + modName;

        // Core objects that is required to patch and configure the mod
        private readonly Harmony harmony = new Harmony(GUID);
        internal static ConfigSync configSync = new ConfigSync(GUID) { DisplayName = modName, CurrentVersion = version, MinimumRequiredVersion = version };
        internal static ConfigFile configFile;

        void Awake()
        {
            configFile = Config;/*new ConfigFile(Config.ConfigFilePath, true);*/
            configFile.SaveOnConfigSet = true;

            ConfigSettings.Init();

            if(ConfigSettings.modEnabled.Value) {
                harmony.PatchAll();
                MineRock5_Patch mineRock5_Patch = new GameObject().AddComponent<MineRock5_Patch>();
                DontDestroyOnLoad(mineRock5_Patch.gameObject);
            }

            CommandManager.Instance.AddConsoleCommand(new ConsoleLog());
        }

        void OnDestroy() {
            harmony.UnpatchSelf();
        }
    }
}
