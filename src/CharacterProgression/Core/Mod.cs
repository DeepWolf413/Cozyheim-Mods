namespace CharacterProgressionMod.Core
{
    public sealed class Mod
    {
        public static bool IsInitialized { get; private set; }

        internal Mod()
        {
            if (IsInitialized) {
                Jotunn.Logger.LogWarning("The mod is already initialized.");
                return;
            }
            
            IsInitialized = true;
        }
    }
}