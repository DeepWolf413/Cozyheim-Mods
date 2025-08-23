namespace DeepWolf.CharacterProgressionMod
{
    internal enum SkillType
    {
        HP,
        HPRegen,
        Stamina,
        StaminaRegen,
        Eitr,
        EitrRegen,
        CarryWeight,
        MovementSpeed,
        Woodcutting,
        Mining,
        PhysicalDamage,
        ElementalDamage,
        PhysicalResistance,
        ElementalResistance,
        CriticalChance,
        CriticalDamage,
        Hunting,
        Farming,
        ResistanceSlash,
        ResistanceBlunt,
        ResistancePierce,
        ResistanceFire,
        ResistanceFrost,
        ResistanceLightning,
        ResistancePoison,
        ResistanceSpirit,
        EndOfEnum
    }

    internal class SkillBase
    {
        public float baseBonus;
        public float bonusPerLevel;
        public string bonusUnit;
        public string displayName;
        public string iconName;
        protected int maxLevel;
        protected SkillType skillType;

        protected SkillOption uiSettings;

        public SkillBase(int maxLevel, float bonusPerLevel, string iconName, string displayName, string bonusUnit = "",
                         float baseBonus = 0f)
        {
            level = 0;
            this.maxLevel = maxLevel;
            this.bonusPerLevel = bonusPerLevel;
            this.bonusUnit = bonusUnit;
            this.displayName = displayName;
            this.iconName = iconName;
            this.baseBonus = baseBonus;
        }

        protected int level { get; set; }

        public void SetSkillUI(SkillOption uiSettings)
        {
            uiSettings.addPointButton.onClick.RemoveAllListeners();
            uiSettings.removePointButton.onClick.RemoveAllListeners();
            uiSettings.resetPointButton.onClick.RemoveAllListeners();

            //            ConsoleLog.Print("Setting up skill");
            this.uiSettings = uiSettings;
            uiSettings.addPointButton.onClick.AddListener(delegate { SkillManager.Instance.SkillLevelUp(skillType); });

            uiSettings.removePointButton.onClick.AddListener(delegate
            {
                SkillManager.Instance.SkillLevelDown(skillType);
            });

            uiSettings.resetPointButton.onClick.AddListener(delegate { SkillManager.Instance.SkillReset(skillType); });
        }

        public int ResetLevel()
        {
            var returnValue = level;
            level = 0;
            return returnValue;
        }

        public void SetLevel(int level)
        {
            if (level > GetMaxLevel()) {
                level = GetMaxLevel();
            }

            if (level < 0) {
                level = 0;
            }

            this.level = level;
        }

        public bool AddLevel()
        {
            if (IsLevelMax()) {
                return false;
            }

            level++;
            return true;
        }

        public bool RemoveLevel()
        {
            if (level <= 0) {
                return false;
            }

            level--;
            return true;
        }

        public bool IsLevelMax()
        {
            return level == maxLevel;
        }

        public bool IsLevelZero()
        {
            return level == 0;
        }

        public float GetBonus()
        {
            return bonusPerLevel * level + baseBonus;
        }

        public string GetName()
        {
            return displayName;
        }

        public int GetLevel()
        {
            return level;
        }

        public int GetMaxLevel()
        {
            return maxLevel;
        }

        public void UpdateSkillInformation()
        {
            if (uiSettings != null) {
                uiSettings.UpdateAllButtonVisibility(this);
                uiSettings.UpdateInformation(this);
            }
        }
    }
}