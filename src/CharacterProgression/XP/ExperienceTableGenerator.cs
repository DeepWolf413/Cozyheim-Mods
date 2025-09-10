using System;
using UnityEngine;

namespace CharacterProgressionMod
{
    public class ExperienceTableGenerator
    {
        public ExperienceTableGenerator()
        {
            MaxLevel = 10;
            MaxLevelTotalExperience = 5000;
            ExperienceFormula = ExperienceFormula.InCubic;
        }
        
        public ExperienceTableGenerator(int maxLevel, int maxLevelTotalExperience, ExperienceFormula experienceFormula)
        {
            MaxLevel = maxLevel;
            MaxLevelTotalExperience = maxLevelTotalExperience;
            ExperienceFormula = experienceFormula;
        }

        public int MaxLevel { get; set; }
        public int MaxLevelTotalExperience { get; set; }
        public ExperienceFormula ExperienceFormula { get; set; }

        public ExperienceTable Generate()
        {
            // Each entry has the format level:maxExperience, meaning an entry specifies the experience needed to reach next level.
            // Imagine max level is 10, by subtracting one the last entry will be 9:512 - the experience needed to reach from level 9 to 10 is 512.
            var tableEntryCount = MaxLevel - 1;
            var table = new int[tableEntryCount];

            int EvaluateMaxExperience(int level)
            {
                var nextLevel = level + 1;
                
                switch (ExperienceFormula) {
                    case ExperienceFormula.Linear:
                        var maxExperiencePerLevel = MaxLevelTotalExperience / MaxLevel;
                        return maxExperiencePerLevel * level;
                    case ExperienceFormula.InCubic:
                        var levelProgress = (float)nextLevel / MaxLevel;
                        var curveProgress = levelProgress * levelProgress * levelProgress;
                        return Mathf.RoundToInt(MaxLevelTotalExperience * curveProgress);
                    default:
                        Jotunn.Logger.LogError($"Unhandled experience formula: {ExperienceFormula}.");
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
            
            return new ExperienceTable(table);
        }
    }
}