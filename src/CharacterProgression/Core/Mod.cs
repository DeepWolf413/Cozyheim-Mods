using BepInEx;

namespace CharacterProgressionMod.Core
{
    internal sealed class Mod
    {
        public static bool IsInitialized { get; private set; }

        internal Mod(BaseUnityPlugin plugin)
        {
            if (IsInitialized) {
                Jotunn.Logger.LogWarning("The mod is already initialized.");
                return;
            }
            
            IsInitialized = true;
            var config = new ModConfig(plugin.Config);
            var resources = new ModResources(config);
            
            Patcher.Initialize(config);
        }
    }
}