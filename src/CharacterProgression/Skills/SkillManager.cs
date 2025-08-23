using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace DeepWolf.CharacterProgressionMod
{
    internal class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance;

        internal static Dictionary<SkillType, SkillBase> skills;
        public int unspendPoints;
        private readonly float critTextOffsetTowardsCam = 0.75f;
        private readonly float critTextOffsetY = 1f;
        private GameObject criticalHitText;

        private GameObject criticalHitVFX;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitSkills();
            LoadSkills();

            criticalHitVFX = PrefabManager.Instance.GetPrefab("CriticalHitEffect");
            criticalHitText = PrefabManager.Instance.GetPrefab("CritDamageText");
        }

        public static void InitSkills()
        {
            skills = new Dictionary<SkillType, SkillBase>();

            foreach (var skill in SkillConfig.skillSettings) {
                if (skill.GetEnabled()) {
                    switch (skill.skillType) {
                        case SkillType.HP:
                            skills.Add(skill.skillType,
                                       new SkillHP(skill.GetMaxLevel(), skill.GetBonusValue(), "HP", "Health",
                                                   string.Empty,
                                                   skill.GetBaseValue()));
                            break;
                        case SkillType.Stamina:
                            skills.Add(skill.skillType,
                                       new SkillStamina(skill.GetMaxLevel(), skill.GetBonusValue(), "Stamina",
                                                        "Stamina",
                                                        string.Empty, skill.GetBaseValue()));
                            break;
                        case SkillType.Eitr:
                            skills.Add(skill.skillType,
                                       new SkillEitr(skill.GetMaxLevel(), skill.GetBonusValue(), "Eitr", "Eitr",
                                                     string.Empty, skill.GetBaseValue()));
                            break;
                        case SkillType.HPRegen:
                            skills.Add(skill.skillType,
                                       new SkillHPRegen(skill.GetMaxLevel(), skill.GetBonusValue(), "HPRegen",
                                                        "Health Regen", "%", skill.GetBaseValue()));
                            break;
                        case SkillType.StaminaRegen:
                            skills.Add(skill.skillType,
                                       new SkillStaminaRegen(skill.GetMaxLevel(), skill.GetBonusValue(), "StaminaRegen",
                                                             "Stamina Regen", "%", skill.GetBaseValue()));
                            break;
                        case SkillType.EitrRegen:
                            skills.Add(skill.skillType,
                                       new SkillEitrRegen(skill.GetMaxLevel(), skill.GetBonusValue(), "EitrRegen",
                                                          "Eitr Regen", "%", skill.GetBaseValue()));
                            break;
                        case SkillType.CarryWeight:
                            skills.Add(skill.skillType,
                                       new SkillCarryWeight(skill.GetMaxLevel(), skill.GetBonusValue(), "CarryWeight",
                                                            "Carry Weight", string.Empty, skill.GetBaseValue()));
                            break;
                        case SkillType.Woodcutting:
                            skills.Add(skill.skillType,
                                       new SkillWoodcutting(skill.GetMaxLevel(), skill.GetBonusValue(), "Woodcutting",
                                                            "Woodcutting", "% damage", skill.GetBaseValue()));
                            break;
                        case SkillType.Mining:
                            skills.Add(skill.skillType,
                                       new SkillMining(skill.GetMaxLevel(), skill.GetBonusValue(), "Mining", "Mining",
                                                       "% damage", skill.GetBaseValue()));
                            break;
                        case SkillType.PhysicalDamage:
                            skills.Add(skill.skillType,
                                       new SkillPhysicalDamage(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                               "PhysicalDamage",
                                                               "Physical Damage", "%", skill.GetBaseValue()));
                            break;
                        case SkillType.ElementalDamage:
                            skills.Add(skill.skillType,
                                       new SkillElementalDamage(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                "ElementalDamage", "Elemental Damage", "%",
                                                                skill.GetBaseValue()));
                            break;

                        // Physical defense skills
                        case SkillType.PhysicalResistance:
                            skills.Add(skill.skillType,
                                       new SkillPhysicalResistance(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                   "PhysicalResistance", "Physical Resistance", "%",
                                                                   skill.GetBaseValue()));
                            break;
                        case SkillType.ResistanceSlash:
                            skills.Add(skill.skillType,
                                       new SkillResistanceSlash(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                "PhysicalResistance", "Slash Resistance", "%",
                                                                skill.GetBaseValue()));
                            break;
                        case SkillType.ResistancePierce:
                            skills.Add(skill.skillType,
                                       new SkillResistancePierce(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                 "PhysicalResistance", "Pierce Resistance", "%",
                                                                 skill.GetBaseValue()));
                            break;
                        case SkillType.ResistanceBlunt:
                            skills.Add(skill.skillType,
                                       new SkillResistanceBlunt(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                "PhysicalResistance", "Blunt Resistance", "%",
                                                                skill.GetBaseValue()));
                            break;

                        // Elemental defense skills
                        case SkillType.ElementalResistance:
                            skills.Add(skill.skillType,
                                       new SkillElementalResistance(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                    "ElementalResistance", "Elemental Resistance", "%",
                                                                    skill.GetBaseValue()));
                            break;
                        case SkillType.ResistanceFire:
                            skills.Add(skill.skillType,
                                       new SkillResistanceFire(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                               "ElementalResistance", "Fire Resistance", "%",
                                                               skill.GetBaseValue()));
                            break;
                        case SkillType.ResistanceFrost:
                            skills.Add(skill.skillType,
                                       new SkillResistanceFrost(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                "ElementalResistance", "Frost Resistance", "%",
                                                                skill.GetBaseValue()));
                            break;
                        case SkillType.ResistanceLightning:
                            skills.Add(skill.skillType,
                                       new SkillResistanceLightning(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                    "ElementalResistance", "Lightning Resistance", "%",
                                                                    skill.GetBaseValue()));
                            break;
                        case SkillType.ResistancePoison:
                            skills.Add(skill.skillType,
                                       new SkillResistancePoison(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                 "ElementalResistance", "Poison Resistance", "%",
                                                                 skill.GetBaseValue()));
                            break;
                        case SkillType.ResistanceSpirit:
                            skills.Add(skill.skillType,
                                       new SkillResistanceSpirit(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                 "ElementalResistance", "Spirit Resistance", "%",
                                                                 skill.GetBaseValue()));
                            break;

                        case SkillType.MovementSpeed:
                            skills.Add(skill.skillType,
                                       new SkillMovementSpeed(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                              "MovementSpeed",
                                                              "Movement Speed", "%", skill.GetBaseValue()));
                            break;
                        case SkillType.CriticalChance:
                            skills.Add(skill.skillType,
                                       new SkillCriticalHitChance(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                  "CriticalHitChance", "Critical Hit Chance", "%",
                                                                  skill.GetBaseValue()));
                            break;
                        case SkillType.CriticalDamage:
                            skills.Add(skill.skillType,
                                       new SkillCriticalHitDamage(skill.GetMaxLevel(), skill.GetBonusValue(),
                                                                  "CriticalHitDamage", "Critical Hit Damage", "%",
                                                                  skill.GetBaseValue()));
                            break;
                    }
                }
            }
        }

        public void SpawnCriticalHitVFX(Vector3 position, float damage)
        {
            if (!Main.ModConfig.CriticalHitVFX.Value) {
                return;
            }

            if (Main.ModConfig.CriticalHitShake.Value) {
                GameCamera.instance.AddShake(Player.m_localPlayer.transform.position, 10f,
                                             Main.ModConfig.CriticalHitShakeIntensity.Value, false);
            }

            var dirToCamera = (GameCamera.instance.transform.position - position).normalized;
            var critPos = position + Vector3.up * critTextOffsetY + dirToCamera * critTextOffsetTowardsCam;

            var critText = Instantiate(criticalHitText, critPos, Quaternion.identity);
            critText.GetComponent<CritTextAnim>().SetText(damage, 1);

            var newVFX = Instantiate(criticalHitVFX, position, Quaternion.identity);
            Destroy(newVFX, 4f);
        }

        public void UpdateAllSkillInformation()
        {
            foreach (var kvp in skills) {
                kvp.Value.UpdateSkillInformation();
            }
        }

        public void SkillSetLevel(SkillType skillType, int level)
        {
            var skill = GetSkillByType(skillType);
            if (skill == null) {
                return;
            }

            Logger.LogDebug($"Setting skill '{skillType}' to level {level}");
            skill.SetLevel(level);
            UpdateUnspendPoints();
        }

        public int GetTotalSkillsCount()
        {
            return skills.Count;
        }

        public SkillBase GetSkillByIndex(int index)
        {
            if (skills.ContainsKey((SkillType)index)) {
                return skills[(SkillType)index];
            }

            return null;
        }

        public SkillBase GetSkillByType(SkillType type)
        {
            return skills.TryGetValue(type, out var skill) ? skill : null;
        }

        public void SkillLevelUp(SkillType skillType)
        {
            var skill = GetSkillByType(skillType);
            if (skill == null) {
                return;
            }

            if (!HasUnspendPoints()) {
                return;
            }

            var pointsToSpend = 1;

            if (Input.GetKey(Main.ModConfig.AddMultiplePointsKey.Value)) {
                pointsToSpend = Mathf.Min(Main.ModConfig.AddMultiplePointsAmount.Value, unspendPoints);
            }

            if (Input.GetKey(Main.ModConfig.AddMaxPointsKey.Value)) {
                pointsToSpend = unspendPoints;
            }

            for (var i = 0; i < pointsToSpend; i++) {
                if (!skill.AddLevel()) {
                    break;
                }
            }

            UpdateUnspendPoints();
        }

        public void SkillLevelDown(SkillType skillType)
        {
            var skill = GetSkillByType(skillType);
            if (skill == null) {
                return;
            }

            var pointsToRemove = 1;

            if (Input.GetKey(Main.ModConfig.AddMultiplePointsKey.Value)) {
                pointsToRemove = Main.ModConfig.AddMultiplePointsAmount.Value;
            }

            if (Input.GetKey(Main.ModConfig.AddMaxPointsKey.Value)) {
                SkillReset(skillType);
                return;
            }

            for (var i = 0; i < pointsToRemove; i++) {
                if (!skill.RemoveLevel()) {
                    break;
                }
            }

            UpdateUnspendPoints();
        }

        public int SkillReset(SkillType skillType)
        {
            var skill = GetSkillByType(skillType);
            if (skill == null) {
                return 0;
            }

            var value = skill.ResetLevel();
            UpdateUnspendPoints();
            return value;
        }

        public void SkillResetAll()
        {
            foreach (var kvp in skills) {
                kvp.Value.ResetLevel();
            }

            UpdateUnspendPoints();
        }

        public bool IsSkillMaxLevel(SkillType skillType)
        {
            var skill = GetSkillByType(skillType);
            if (skill == null) {
                return false;
            }

            return skill.IsLevelMax();
        }

        public bool HasUnspendPoints()
        {
            return unspendPoints > 0;
        }

        public int GetSkillPointSpendOnCategory(SkillCategory category)
        {
            var count = 0;
            foreach (var skill in SkillConfig.skillSettings) {
                if (!skill.GetEnabled()) {
                    continue;
                }

                if (skill.category == category) {
                    count += GetSkillByType(skill.skillType).GetLevel();
                }
            }

            return count;
        }

        public void UpdateUnspendPoints()
        {
            if (skills != null) {
                RecalculateUnspendPoints();

                UIManager.Instance.remainingPoints.text = "Remaining points: " + unspendPoints;
                UIManager.Instance.UpdateCategoryPoints();

                SaveSkills();
                UpdateAllSkillInformation();
            }
        }

        public void RecalculateUnspendPoints()
        {
            var points = Mathf.FloorToInt(PlayerExpPool.Local.GetLevel() * Main.ModConfig.PointsPerLevel.Value);
            foreach (var kvp in skills) {
                points -= kvp.Value.GetLevel();
            }

            unspendPoints = points;
        }

        private void LoadSkills()
        {
            foreach (var kvp in skills) {
                var skillName = Main.ModName + "_" + kvp.Key;
                if (Player.m_localPlayer.m_customData.ContainsKey(skillName)) {
                    int value;
                    var savedString = Player.m_localPlayer.m_customData[skillName];
                    if (int.TryParse(savedString, out value)) {
                        kvp.Value.SetLevel(value);
                    }
                }
            }

            UpdateUnspendPoints();
        }

        private void SaveSkills()
        {
            foreach (var kvp in skills) {
                var skillName = Main.ModName + "_" + kvp.Key;
                Player.m_localPlayer.m_customData[skillName] = kvp.Value.GetLevel().ToString();
            }
        }

        public void DestroySelf()
        {
            SaveSkills();
            Destroy(gameObject);
        }

        public void ReloadAllSkills()
        {
            InitSkills();
            LoadSkills();
            UIManager.Instance.UpdateUI(true);
        }
    }
}