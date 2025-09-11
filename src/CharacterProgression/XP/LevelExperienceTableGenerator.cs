using CharacterProgressionMod.Formulas;

namespace CharacterProgressionMod
{
    public class LevelExperienceTableGenerator
    {
        public LevelExperienceTableGenerator()
        {
            MaxLevel = 10;
            MaxLevelTotalExperience = 5000;
            MaxExperienceFormula = new InCubicMaxExperienceFormula();
        }
        
        public LevelExperienceTableGenerator(int maxLevel, int maxLevelTotalExperience, IMaxExperienceFormula maxExperienceFormula)
        {
            MaxLevel = maxLevel;
            MaxLevelTotalExperience = maxLevelTotalExperience;
            MaxExperienceFormula = maxExperienceFormula;
        }

        public int MaxLevel { get; set; }
        public int MaxLevelTotalExperience { get; set; }
        public IMaxExperienceFormula MaxExperienceFormula { get; set; }

        public LevelExperienceTable Generate()
        {
            // Each entry has the format level:maxExperience, meaning an entry specifies the experience needed to reach next level.
            // Imagine max level is 10, by subtracting one the last entry will be 9:512 - the experience needed to reach from level 9 to 10 is 512.
            var tableEntryCount = MaxLevel - 1;
            var table = new int[tableEntryCount];

            Jotunn.Logger.LogDebug("Generating experience table...");
            for (var entryIndex = 0; entryIndex < table.Length; entryIndex++) {
                var level = entryIndex + 1;
                var levelMaxExperience = MaxExperienceFormula.EvaluateMaxExperience(level, MaxLevel, MaxLevelTotalExperience);
                table[entryIndex] = levelMaxExperience;
                Jotunn.Logger.LogDebug($"Level: {level} | MaxExperience: {levelMaxExperience}");
            }
            
            return new LevelExperienceTable(table);
        }
    }
}