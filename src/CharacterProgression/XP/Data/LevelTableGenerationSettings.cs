namespace CharacterProgressionMod
{
    public class LevelTableGenerationSettings
    {
        public LevelTableGenerationSettings(int maxLevel, int initialMaxExperience, string maxExperienceModifierFormula)
        {
            MaxLevel = maxLevel;
            InitialMaxExperience = initialMaxExperience;
            MaxExperienceModifierFormula = new MaxExperienceModifierFormula(maxExperienceModifierFormula);
        }

        public int MaxLevel { get; }
        public int InitialMaxExperience { get; }
        public MaxExperienceModifierFormula MaxExperienceModifierFormula { get; }
    }
}