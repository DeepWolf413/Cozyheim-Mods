using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn;
using Jotunn.Utils;

namespace DeepWolf.CharacterProgressionMod
{
    public sealed class LevelXpTable
    {
        private readonly string _configFolderPath;
        private readonly string _fallbackConfigFilePath;
        private int[] _levels;

        public LevelXpTable(string configFolderPath, string fallbackConfigFilePath)
        {
            _configFolderPath = configFolderPath;
            _fallbackConfigFilePath = fallbackConfigFilePath;
            LoadConfig();
        }

        public int MaxLevel => _levels.Length;

        private void LoadConfig()
        {
            if (!Directory.Exists(_configFolderPath)) {
                LoadFallbackConfig();
                return;
            }

            var jsonFiles = Directory.GetFiles(_configFolderPath, "*.json", SearchOption.TopDirectoryOnly);
            foreach (var jsonFile in jsonFiles) {
                var json = File.ReadAllText(jsonFile);
                var xpTable = SimpleJson.SimpleJson.DeserializeObject<IReadOnlyDictionary<string, int>>(json);
                if (xpTable.Count == 0) {
                    continue;
                }

                _levels = xpTable.Values.ToArray();
                Logger.LogDebug($"Successfully loaded the config for level xp table: '{jsonFile}'");
                break;
            }
        }

        /// <summary>
        ///     Loads the fallback configuration for the level experience table.
        ///     This method is called when the primary configuration files cannot be located or parsed.
        ///     It reads and deserializes a fallback JSON resource to ensure the level XP table is populated
        ///     with default values, avoiding runtime issues.
        /// </summary>
        /// <remarks>
        ///     If the fallback configuration cannot be loaded or is invalid, an error is logged and the XP table remains
        ///     uninitialized.
        /// </remarks>
        private void LoadFallbackConfig()
        {
            var json = AssetUtils.LoadTextFromResources(_fallbackConfigFilePath);
            var xpTable = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, int>>(json);
            if (xpTable.Count == 0) {
                Logger.LogError("Failed to load the fallback config for level xp table.");
                return;
            }

            _levels = xpTable.Values.ToArray();
        }

        /// <summary>
        ///     Retrieves the maximum experience points required to reach a specified level.
        ///     This method returns the corresponding XP value from the level XP table for the given level,
        ///     ensuring level constraints are respected.
        /// </summary>
        /// <param name="level">
        ///     The level for which the maximum XP is requested. Must be a positive integer within the valid level
        ///     range.
        /// </param>
        /// <returns>
        ///     The maximum XP required to reach the specified level. If the level is out of range, returns 1 as a default value.
        ///     Logs an error if the level is invalid.
        /// </returns>
        public int GetMaxExpAtLevel(int level)
        {
            var levelIndex = level - 1;
            if (levelIndex < 0 || levelIndex >= _levels.Length) {
                Logger.LogError($"Level {level} is out of range. Max level is {_levels.Length}.");
                return 1;
            }

            return _levels[levelIndex];
        }

        public int GetTotalExpForLevel(int level)
        {
            var previousLevel = level - 1;
            if (previousLevel <= 0 || previousLevel >= _levels.Length) {
                Logger.LogError($"Level {level} is out of range.");
                return 0;
            }

            var previousLevelIndex = previousLevel - 1;
            return _levels[previousLevelIndex];
        }

        public int GetLevelFromTotalExp(int totalExp)
        {
            for (var i = 0; i < _levels.Length; i++) {
                if (totalExp < _levels[i]) {
                    return i + 1;
                }
            }

            return 0;
        }
    }
}