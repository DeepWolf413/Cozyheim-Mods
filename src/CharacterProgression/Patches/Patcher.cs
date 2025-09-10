using System.Reflection;
using HarmonyLib;
using CharacterProgressionMod.Core;

namespace CharacterProgressionMod.Patches
{
    [HarmonyPatch]
    internal static partial class Patcher
    {
        private static readonly Harmony Harmony = new(PluginInfo.Guid);
        private static PluginConfig _config;
        private static ModResources _resources;
        
        public static void PatchAll(PluginConfig config, ModResources resources)
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            _config = config;
            _resources = resources;
        }

        public static void Unpatch() => Harmony.UnpatchSelf();
    }
}