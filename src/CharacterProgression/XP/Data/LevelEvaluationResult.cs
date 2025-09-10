namespace CharacterProgressionMod
{
    public readonly struct LevelEvaluationResult
    {
        public LevelEvaluationResult(int level, int maxExperience, int totalExperience, int nextLevelTotalExperience, bool isMaxLevel)
        {
            Level = level;
            MaxExperience = maxExperience;
            TotalExperience = totalExperience;
            NextLevelTotalExperience = nextLevelTotalExperience;
            IsMaxLevel = isMaxLevel;
        }

        public int Level { get; }
        public int MaxExperience { get; }
        public int TotalExperience { get; }
        public int NextLevelTotalExperience { get; }
        public bool IsMaxLevel { get; }

        public float EvaluateProgressPercentage(int currentTotalExperience) =>
            ((float)(currentTotalExperience - TotalExperience) / MaxExperience) * 100.0f;
        
        public override string ToString() => $"{Level} | {MaxExperience} | {TotalExperience} | {NextLevelTotalExperience} | {IsMaxLevel}";
    }
}