using System.Reflection;
using BepInEx;
using CharacterProgressionMod.Commands;
using CharacterProgressionMod.Patches;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace CharacterProgressionMod.Core
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.ModName, PluginInfo.Version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency(PluginDependencyFinder.Guids.SmoothbrainsJewelcrafting, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal sealed class PluginInitializer : BaseUnityPlugin
    {
        private void Awake()
        {
            static void InitCommands()
            {
                CommandManager.Instance.AddConsoleCommand(new SetLevelCommand());
                CommandManager.Instance.AddConsoleCommand(new LevelUpCommand());
            }
            
            InitCommands();
            
            var config = new PluginConfig(Config);
            var resources = new ModResources(config);
            Patcher.PatchAll(config, resources);
        }

        private void OnDestroy()
        {
            Patcher.Unpatch();
        }
    }
}