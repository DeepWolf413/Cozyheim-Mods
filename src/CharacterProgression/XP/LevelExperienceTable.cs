using System.Collections.Generic;
using System.Linq;

namespace CharacterProgressionMod
{
    public class LevelExperienceTable
    {
        private readonly int[] _entries;
        
        public int MaxLevel { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="LevelExperienceTable"/> with the specified entries.
        /// </summary>
        public LevelExperienceTable(int[] entries)
        {
            if (entries.Length == 0) {
                Jotunn.Logger.LogWarning("No entries was given.");
                return;
            }
            
            _entries = entries;
            // One is added because the table is stored like level:maxExperience, which means the last entry - with max level being 90 - is 89.
            MaxLevel = _entries.Length + 1;
            Jotunn.Logger.LogDebug($"Successfully loaded a player experience table. Max level is {MaxLevel}");
        }
        
        /// <summary>
        /// Loads the experience table from json.
        /// </summary>
        public LevelExperienceTable(string experienceTableJson)
        {
            if (string.IsNullOrEmpty(experienceTableJson)) {
                Jotunn.Logger.LogError("Invalid json for experience table.");
                return;
            }
            
            var xpTable = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, int>>(experienceTableJson);
            _entries = xpTable.Values.ToArray();
            Jotunn.Logger.LogDebug($"Successfully loaded a player experience table. Max level is {MaxLevel}");
        }
        
        public int GetMaxExperience(int level)
        {
            var levelIndex = level - 1;
            if (levelIndex < 0 || levelIndex >= _entries.Length) {
                Jotunn.Logger.LogError($"Level {level} is out of range. Max level is {_entries.Length}.");
                return 1;
            }

            return _entries[levelIndex];
        }

        public int GetTotalExperience(int level)
        {
            if (MaxLevel <= 1) {
                return 0;
            }

            var totalExperience = 0;
            for (var levelExperienceIndex = 0; levelExperienceIndex < _entries.Length; levelExperienceIndex++) {
                var tableLevel = levelExperienceIndex + 1;
                if (tableLevel >= level) {
                    break;
                }

                totalExperience += _entries[levelExperienceIndex];
            }
            
            return totalExperience;
        }

        public LevelEvaluationResult EvaluateLevel(int totalExperience)
        {
            var accumulatedExperience = 0;
            for (var entryIndex = 0; entryIndex < _entries.Length; entryIndex++) {
                var levelMaxExperience = _entries[entryIndex];
                accumulatedExperience += levelMaxExperience;

                if (totalExperience >= accumulatedExperience) {
                    continue;
                }
                
                var nextLevelTotalExperience = accumulatedExperience;
                var levelTotalExperience = accumulatedExperience - levelMaxExperience;
                var level = entryIndex + 1;
                bool isMaxLevel = level >= _entries.Length;
                return new LevelEvaluationResult(level, levelMaxExperience, levelTotalExperience, nextLevelTotalExperience, isMaxLevel);
            }

            return new LevelEvaluationResult();
        }
    }
}