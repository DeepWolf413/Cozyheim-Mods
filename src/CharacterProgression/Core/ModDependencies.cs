using System.Linq;
using BepInEx.Bootstrap;

namespace CharacterProgressionMod.Core
{
    internal static class ModDependencies
    {
        public const string JewelcraftingModGuid = "org.bepinex.plugins.jewelcrafting";
        
        /// <summary>
        /// Whether the Jewelcrafting mod by Smoothbrain is loaded.
        /// </summary>
        /// <remarks>
        /// Link to mod: https://thunderstore.io/c/valheim/p/Smoothbrain/Jewelcrafting/
        /// </remarks>
        public static bool IsJewelcraftingLoaded { get; private set; }

        public static void CheckForLoadedDependencies()
        {
            IsJewelcraftingLoaded = IsModLoaded(JewelcraftingModGuid);
        }
        
        private static bool IsModLoaded(string guid) => Chainloader.PluginInfos.Values.Any(x => x.Metadata.GUID == guid);
    }
}