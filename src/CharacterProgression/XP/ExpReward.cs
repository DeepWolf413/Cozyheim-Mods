namespace DeepWolf.CharacterProgressionMod
{
    public struct ExpReward : ISerializableParameter
    {
        public int BaseReward { get; private set; }
        public int LevelBonus { get; private set; }
        public int RestedBonus { get; private set; }
        public ExpSource Source { get; private set; }

        private int TotalReward => BaseReward + LevelBonus + RestedBonus;

        public ExpReward(ExpSource source, int baseReward, int levelBonus, int restedBonus)
        {
            Source = source;
            BaseReward = baseReward;
            LevelBonus = levelBonus;
            RestedBonus = restedBonus;
        }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write((byte)Source);
            pkg.Write(BaseReward);
            pkg.Write(LevelBonus);
            pkg.Write(RestedBonus);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Source = (ExpSource)pkg.ReadByte();
            BaseReward = pkg.ReadInt();
            LevelBonus = pkg.ReadInt();
            RestedBonus = pkg.ReadInt();
        }

        public int GetEligibleExp(bool isRested)
        {
            var eligibleExp = BaseReward;
            eligibleExp += LevelBonus;

            if (isRested) {
                eligibleExp += RestedBonus;
            }

            return eligibleExp;
        }

        public override string ToString()
        {
            return $"{Source.ToString()} | {BaseReward} + {LevelBonus} + {RestedBonus} = {TotalReward}";
        }
    }
}