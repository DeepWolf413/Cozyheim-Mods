using System;
using UnityEngine;

namespace CharacterProgressionMod
{
    public class LevelExperienceTableGenerator
    {
        public LevelExperienceTableGenerator()
        {
            MaxLevel = 10;
            MaxLevelTotalExperience = 5000;
            LevelExperienceFormula = LevelExperienceFormula.InCubic;
        }
        
        public LevelExperienceTableGenerator(int maxLevel, int maxLevelTotalExperience, LevelExperienceFormula levelExperienceFormula)
        {
            MaxLevel = maxLevel;
            MaxLevelTotalExperience = maxLevelTotalExperience;
            LevelExperienceFormula = levelExperienceFormula;
        }

        public int MaxLevel { get; set; }
        public int MaxLevelTotalExperience { get; set; }
        public LevelExperienceFormula LevelExperienceFormula { get; set; }

        public LevelExperienceTable Generate()
        {
            // Each entry has the format level:maxExperience, meaning an entry specifies the experience needed to reach next level.
            // Imagine max level is 10, by subtracting one the last entry will be 9:512 - the experience needed to reach from level 9 to 10 is 512.
            var tableEntryCount = MaxLevel - 1;
            var table = new int[tableEntryCount];

            int EvaluateMaxExperience(int level)
            {
                var nextLevel = level + 1;
                
                switch (LevelExperienceFormula) {
                    case LevelExperienceFormula.Linear:
                        var maxExperiencePerLevel = MaxLevelTotalExperience / MaxLevel;
                        return maxExperiencePerLevel * level;
                    case LevelExperienceFormula.InCubic:
                        var levelProgress = (float)nextLevel / MaxLevel;
                        var curveProgress = levelProgress * levelProgress * levelProgress;
                        return Mathf.RoundToInt(MaxLevelTotalExperience * curveProgress);
                    default:
                        Jotunn.Logger.LogError($"Unhandled experience formula: {LevelExperienceFormula}.");
                        break;
                }

                return 1;
            }
            
            Jotunn.Logger.LogDebug("Generating experience table...");
            for (var entryIndex = 0; entryIndex < table.Length; entryIndex++) {
                var level = entryIndex + 1;
                var levelMaxExperience = EvaluateMaxExperience(level);
                table[entryIndex] = levelMaxExperience;
                Jotunn.Logger.LogDebug($"Level: {level} | MaxExperience: {levelMaxExperience}");
            }
            
            return new LevelExperienceTable(table);
        }
    }
}