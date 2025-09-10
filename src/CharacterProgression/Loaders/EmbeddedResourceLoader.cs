using Jotunn.Utils;

namespace CharacterProgressionMod.Loaders
{
    public static class EmbeddedResourceLoader
    {
        public static string Load(string path) => AssetUtils.LoadTextFromResources(path);
    }
}