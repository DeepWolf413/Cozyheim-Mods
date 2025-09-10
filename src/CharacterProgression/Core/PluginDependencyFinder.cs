using System.Linq;
using BepInEx.Bootstrap;

namespace CharacterProgressionMod.Core
{
    internal static class PluginDependencyFinder
    {
        /// <summary>
        /// Contains plugin guids for all dependencies.
        /// </summary>
        public static class Guids
        {
            public const string SmoothbrainsJewelcrafting = "org.bepinex.plugins.jewelcrafting";
        }

        public static bool CanFind(string pluginGuid) => Chainloader.PluginInfos.Values.Any(x => x.Metadata.GUID == pluginGuid);
    }
}