namespace CharacterProgressionMod.Formulas
{
    public class LinearMaxExperienceFormula : IMaxExperienceFormula
    {
        public virtual int EvaluateMaxExperience(int level, int maxLevel, int maxLevelTotalExperience)
        {
            var maxExperiencePerLevel = maxLevelTotalExperience / maxLevel;
            return maxExperiencePerLevel * level;
        }
    }
}