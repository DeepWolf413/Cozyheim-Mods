using UnityEngine;

namespace CharacterProgressionMod.Formulas
{
    /// <summary>
    /// Implements this formula: https://easings.net/#easeInCubic
    /// </summary>
    public class InCubicMaxExperienceFormula : IMaxExperienceFormula
    {
        public virtual int EvaluateMaxExperience(int level, int maxLevel, int maxLevelTotalExperience)
        {
            var nextLevel = level + 1;
            var levelProgress = (float)nextLevel / maxLevel;
            var curveProgress = levelProgress * levelProgress * levelProgress;
            return Mathf.RoundToInt(maxLevelTotalExperience * curveProgress);
        }
    }
}