using CharacterProgressionMod.Core;
using System.IO;
using Jotunn.Utils;

namespace CharacterProgressionMod
{
    internal static class XPManager
    {
        public static XpTable MiningXpTable { get; private set; }
        public static XpTable WoodcuttingXpTable { get; private set; }
        public static XpTable PickablesXpTable { get; private set; }
        public static XpTable CreaturesXpTable { get; private set; }
        public static LevelXpTable PlayerXpTable { get; private set; }

        public static void Initialize()
        {
            var resourceAssembly = ReflectionHelper.GetCallingAssembly();
            MiningXpTable = new XpTable(resourceAssembly, Path.Combine(ModEntry.ConfigFolder, "mining"), true);
            WoodcuttingXpTable = new XpTable(resourceAssembly, Path.Combine(ModEntry.ConfigFolder, "woodcutting"), true);
            PickablesXpTable = new XpTable(resourceAssembly, Path.Combine(ModEntry.ConfigFolder, "pickables"), true);
            CreaturesXpTable = new XpTable(resourceAssembly, Path.Combine(ModEntry.ConfigFolder, "creatures"), false);
            PlayerXpTable = new LevelXpTable(Path.Combine(ModEntry.ConfigFolder, "player"),
                                             "LevelingSystem.Resources.default_configs.player.xp_tables.default.json");
        }
    }
}