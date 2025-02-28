namespace Cozyheim.LevelingSystem
{
    internal class SkillCriticalHitDamage : SkillBase
    {
        public static SkillCriticalHitDamage Instance;

        public SkillCriticalHitDamage(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
        {
            skillType = SkillType.CriticalDamage;
            Instance = this;
        }

        // See CriticalHitChance for the critical hit implementation

    }
}
