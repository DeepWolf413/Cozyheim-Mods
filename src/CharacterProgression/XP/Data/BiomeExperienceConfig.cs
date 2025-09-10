namespace CharacterProgressionMod
{
    public class BiomeExperienceConfig
    {
        public class ExperienceCategory
        {
            public string Name { get; }
            public int BaseExp { get; set; }
            public float TierMultiplier { get; set; }

            public ExperienceCategory(string name, int baseExp, float tierMultiplier = 1.0f)
            {
                Name = name;
                BaseExp = baseExp;
                TierMultiplier = tierMultiplier;
            }
        }
        
        public ExperienceCategory Foraging { get; } = new ExperienceCategory("Foraging", 5);
        public ExperienceCategory Woodcutting { get; } = new ExperienceCategory("Woodcutting", 10, 1.2f);
        public ExperienceCategory Mining { get; } = new ExperienceCategory("Mining", 5, 1.2f);
        public ExperienceCategory CreatureKilling { get; } = new ExperienceCategory("CreatureKilling", 2, 2.0f);
    }
}