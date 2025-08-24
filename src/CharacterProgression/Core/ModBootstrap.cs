using System.Reflection;
using BepInEx;
using CharacterProgressionMod.Commands;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace CharacterProgressionMod.Core
{
    [BepInPlugin(ModInfo.Guid, ModInfo.ModName, ModInfo.Version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInDependency(ModDependencies.JewelcraftingModGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal sealed class ModBootstrap : BaseUnityPlugin
    {
        public const string ConfigFolder = "xp_tables";

        private readonly Harmony _harmony = new(ModInfo.Guid);

        private void Awake()
        {
            static void InitCommands()
            {
                CommandManager.Instance.AddConsoleCommand(new SetLevelCommand());
                CommandManager.Instance.AddConsoleCommand(new LevelUpCommand());
            }
            
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            ModDependencies.CheckForLoadedDependencies();
            InitCommands();
            
            var mod = new Mod(this);
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}