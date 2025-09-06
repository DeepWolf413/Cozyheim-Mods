using HarmonyLib;
using CharacterProgressionMod.Core;

namespace CharacterProgressionMod.Patches
{
    [HarmonyPatch]
    internal static partial class Patcher
    {
        private static ModConfig _config;
        
        public static void Initialize(ModConfig config)
        {
            _config = config;
        }
    }
}