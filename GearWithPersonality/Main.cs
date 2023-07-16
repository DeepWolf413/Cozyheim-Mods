using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using ServerSync;
using UnityEngine;

namespace Cozyheim.GearWithPersonality {
    [BepInPlugin(GUID, modName, version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
//    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class Main : BaseUnityPlugin {
        // Mod information
        internal const string modName = "GearWithPersonality";
        internal const string version = "0.0.1";
        internal const string GUID = "dk.thrakal." + modName;

        // Core objects that is required to patch and configure the mod
        private readonly Harmony harmony = new Harmony(GUID);
        internal static ConfigSync configSync = new ConfigSync(GUID) { DisplayName = modName, CurrentVersion = version, MinimumRequiredVersion = version };
        internal static ConfigFile configFile;

        void Awake() {
            harmony.PatchAll();
            configFile = new ConfigFile(Config.ConfigFilePath, true);
            configFile.SaveOnConfigSet = true;

            ConfigSettings.Init();

            CommandManager.Instance.AddConsoleCommand(new ConsoleLog());
        }


        void Update() {
            if(Input.GetKey(KeyCode.RightControl)) {
                if(Player.m_localPlayer.GetInventory().GetEquippedItems().Count > 0) {
                    if(Input.GetKeyDown(KeyCode.P)) {
                        ItemDrop.ItemData firstItem = Player.m_localPlayer.GetInventory().GetEquippedItems()[0];
                        ItemPersonality.PrintAllPersonalityValues(firstItem);
                    }
                    if(Input.GetKeyDown(KeyCode.R)) {
                        ItemDrop.ItemData firstItem = Player.m_localPlayer.GetInventory().GetEquippedItems()[0];
                        ItemPersonality.RemoveAllPersonalities(firstItem);
                    }
                    if(Input.GetKeyDown(KeyCode.G)) {
                        ItemDrop.ItemData firstItem = Player.m_localPlayer.GetInventory().GetEquippedItems()[0];
                        float bonus = PersonalityTraits.GetWeaponTraitBonus(firstItem, Traits.TraitType.Slayer, "Neck");
                        if(bonus > 0) {
                            ConsoleLog.Print("Trait bonus: " + bonus);
                        } else {
                            ConsoleLog.Print("No bonus found");
                        }
                    }
                } else {
                    ConsoleLog.Print("No items equipped", LogType.Warning);
                }
            }
        }




        void OnDestroy() {
            harmony.UnpatchSelf();
        }
    }
}
