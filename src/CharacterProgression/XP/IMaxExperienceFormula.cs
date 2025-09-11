namespace CharacterProgressionMod
{
    public interface IMaxExperienceFormula
    {
        int EvaluateMaxExperience(int level, int maxLevel, int maxLevelTotalExperience);
    }
}