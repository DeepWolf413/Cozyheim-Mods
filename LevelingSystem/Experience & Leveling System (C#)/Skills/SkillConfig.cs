using BepInEx.Configuration;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Cozyheim.LevelingSystem
{
    internal class SkillConfig
    {
        public static List<SkillSettings> skillSettings = new List<SkillSettings>()
        {
            new SkillSettings() {
                skillType = SkillType.HP,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Core,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.Stamina,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Core,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.Eitr,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Core,
            defaultBaseValue = 0.0f
            },
            new SkillSettings() {
                skillType = SkillType.HPRegen,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Core,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.StaminaRegen,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Core,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.EitrRegen,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Core,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.CarryWeight,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 7.5f,
                category = SkillCategory.Utility,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.Woodcutting,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1.5f,
                category = SkillCategory.Utility,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.Mining,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1.5f,
                category = SkillCategory.Utility,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.PhysicalDamage,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1.5f,
                category = SkillCategory.Offensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.ElementalDamage,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1.5f,
                category = SkillCategory.Offensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.PhysicalResistance,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1.5f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.ResistanceBlunt,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.ResistanceSlash,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.ResistancePierce,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },

            // Elemental defense skills
            new SkillSettings()
            {
                skillType = SkillType.ElementalResistance,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1.5f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings() {
                skillType = SkillType.ResistanceFire,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings() {
                skillType = SkillType.ResistanceFrost,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings() {
                skillType = SkillType.ResistanceLightning,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings() {
                skillType = SkillType.ResistancePoison,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },
            new SkillSettings() {
                skillType = SkillType.ResistanceSpirit,
                enabled = true,
                defaultMaxLevel = 30,
                defaultBonusValue = 3f,
                category = SkillCategory.Defensive,
            defaultBaseValue = 0.0f
            },

            new SkillSettings()
            {
                skillType = SkillType.MovementSpeed,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 1f,
                category = SkillCategory.Utility,
                defaultBaseValue = 0.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.CriticalChance,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 0.5f,
                category = SkillCategory.Offensive,
                defaultBaseValue = 1.0f
            },
            new SkillSettings()
            {
                skillType = SkillType.CriticalDamage,
                enabled = true,
                defaultMaxLevel = 20,
                defaultBonusValue = 5f,
                category = SkillCategory.Offensive,
                defaultBaseValue = 10.0f
            }
        };
    

        public static void Init()
        {
            foreach(SkillSettings skill in skillSettings)
            {
                skill.CreateConfigEntries();
            }
        }


        public static SkillSettings GetSkill(SkillType skillType)
        {
            foreach(SkillSettings skill in skillSettings)
            {
                if(skill.skillType == skillType)
                {
                    return skill;
                }
            }
            return null;
        }
    }

    internal class SkillSettings
    {
        public SkillType skillType;
        public bool enabled;
        public int defaultMaxLevel;
        public float defaultBonusValue;
        public float defaultBaseValue;
        public SkillCategory category;

        private ConfigEntry<string> configSettings;

        public void CreateConfigEntries()
        {
            string settingsString = enabled.ToString() + ":" + defaultMaxLevel.ToString() + ":" + defaultBonusValue.ToString() + ":" + defaultBaseValue.ToString();
            configSettings = Main.CreateConfigEntry
            (
                "Skills",
                skillType.ToString(),
                settingsString,
                "Settings for " + skillType.ToString() + ". Must follow the following format 'bool:int:float:float' (enabled:maxLevel:bonusValue:baseValue)",
                true
            );
        }

        public bool GetEnabled()
        {
            string value = configSettings.Value.Split(':')[0];
            return bool.Parse(value);
        }

        public int GetMaxLevel()
        {
            string data = configSettings.Value.Split(':')[1];

            int value;
            int.TryParse(data, out value);

            return value;
        }

        public float GetBonusValue()
        {
            string data = configSettings.Value.Split(':')[2];

            float valueA;
            float valueB;
            float valueC;

            bool boolA = float.TryParse(data, out valueA);
            bool boolB = float.TryParse(data.Replace(',', '.'), out valueB);
            bool boolC = float.TryParse(data.Replace('.', ','), out valueC);

            float lowestValue = 0f;

            if (boolA && boolB && boolC)
            {
                lowestValue = Mathf.Min(Mathf.Min(valueA, valueB), valueC);
            }
            else if (boolA && boolB)
            {
                lowestValue = Mathf.Min(valueA, valueB);
            }
            else if (boolA && boolC)
            {
                lowestValue = Mathf.Min(valueA, valueC);
            }
            else if (boolB && boolC)
            {
                lowestValue = Mathf.Min(valueB, valueC);
            }
            else if (boolA)
            {
                lowestValue = valueA;
            }
            else if (boolB)
            {
                lowestValue = valueB;
            }
            else if (boolC)
            {
                lowestValue = valueC;
            }

            return lowestValue;
        }
        
        public float GetBaseValue()
        {
            var splitData = configSettings.Value.Split(':');
            string data;
            if (splitData.Length < 4) {
                data = defaultBaseValue.ToString();
            }
            else {
                data = splitData[3];
            }

            float valueA;
            float valueB;
            float valueC;

            bool boolA = float.TryParse(data, out valueA);
            bool boolB = float.TryParse(data.Replace(',', '.'), out valueB);
            bool boolC = float.TryParse(data.Replace('.', ','), out valueC);

            float lowestValue = 0f;

            if (boolA && boolB && boolC)
            {
                lowestValue = Mathf.Min(Mathf.Min(valueA, valueB), valueC);
            }
            else if (boolA && boolB)
            {
                lowestValue = Mathf.Min(valueA, valueB);
            }
            else if (boolA && boolC)
            {
                lowestValue = Mathf.Min(valueA, valueC);
            }
            else if (boolB && boolC)
            {
                lowestValue = Mathf.Min(valueB, valueC);
            }
            else if (boolA)
            {
                lowestValue = valueA;
            }
            else if (boolB)
            {
                lowestValue = valueB;
            }
            else if (boolC)
            {
                lowestValue = valueC;
            }

            return lowestValue;
        }
    }
}

public enum SkillCategory
{
    Offensive,
    Defensive,
    Core,
    Utility
}