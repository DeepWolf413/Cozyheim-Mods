using System;
using CharacterProgressionMod.Core;
using UnityEngine;

namespace CharacterProgressionMod
{
    public sealed class PlayerLevelProgression : MonoBehaviour
    {
        private const string TotalExpSaveKey = "Cozyheim!TotalExperience";
        private const string LevelSaveKey = "Cozyheim!Level";

        private Player _player;
        private string _addExperienceRpcId;
        private ExperienceTable _experienceTable;
        private LevelEvaluationResult _currentLevelEvaluation;

        public ExperienceTable ExperienceTable
        {
            get => _experienceTable;
            set
            {
                if (_experienceTable == value || _player is null) {
                    return;
                }
                
                _experienceTable = value;
                UpdateLevel();
            }
        }
        
        private void Awake()
        {
            _addExperienceRpcId = RpcId.Generate("AddExperience");
            _player = GetComponent<Player>();
            
            if (_player.m_nview.GetZDO() == null) {
                return;
            }

            _player.m_nview.Register<int>(_addExperienceRpcId, RPC_AddExperience);
        }

        private void Start()
        {
            UpdateLevel();
        }

        public void AddExperience(int expReward)
        {
            if (_player.IsOwner()) {
                RPC_AddExperience(-1, expReward);
                return;
            }
            
            _player.m_nview.InvokeRPC(_addExperienceRpcId, expReward);
        }

        private void RPC_AddExperience(long sender, int expReward)
        {
            float finalTotalExperience = expReward;

            // add experience bonuses
            var statusEffects = _player.GetSEMan();
            var isRested = statusEffects.HaveStatusEffect(SEMan.s_statusEffectRested);
            const float restedExpMultiplier = 1.2f;
            float restedBonusExp = expReward * (1.0f - restedExpMultiplier);
            if (isRested) {
                finalTotalExperience += restedBonusExp;
            }

            Jotunn.Logger.LogDebug($"Added {finalTotalExperience:N0} experience");
            finalTotalExperience += GetTotalExperience();
            SetTotalExperience(Mathf.RoundToInt(finalTotalExperience));
        }

        private void SetTotalExperience(int totalExperience)
        {
            _player.m_customData[TotalExpSaveKey] = totalExperience.ToString();
            var levelProgressPercentage = _currentLevelEvaluation.EvaluateProgressPercentage(totalExperience);
            Jotunn.Logger.LogDebug($"Level progress: {totalExperience - _currentLevelEvaluation.TotalExperience:N0} / {_currentLevelEvaluation.MaxExperience} ({levelProgressPercentage:F0}%)");
            UpdateLevelProgress();
        }

        public int GetTotalExperience()
        {
            if (!_player.m_customData.TryGetValue(TotalExpSaveKey, out var totalExperience) ||
                !int.TryParse(totalExperience, out var parsedTotalExperience)) {
                return 0;
            }

            return parsedTotalExperience;
        }

        public int GetLevel()
        {
            if (!_player.m_customData.TryGetValue(LevelSaveKey, out var level) ||
                !int.TryParse(level, out var parsedLevel)) {
                return 0;
            }

            return parsedLevel;
        }

        /// <summary>
        /// Updates the level if the total experience isn't within the current level experience range.
        /// </summary>
        private void UpdateLevelProgress()
        {
            var totalExperience = GetTotalExperience();
            var relativeExperience = totalExperience - _currentLevelEvaluation.TotalExperience;
            var isLevelStillValid = relativeExperience >= 0 && relativeExperience < _currentLevelEvaluation.MaxExperience;
            
            if (isLevelStillValid) {
                return;
            }
            
            UpdateLevel();
        }
        
        private void UpdateLevel()
        {
            if (_experienceTable == null) {
                return;
            }

            _currentLevelEvaluation = _experienceTable.EvaluateLevel(GetTotalExperience());
            
            _player.m_customData[LevelSaveKey] = _currentLevelEvaluation.Level.ToString();
            Jotunn.Logger.LogDebug(_currentLevelEvaluation.ToString());
            Jotunn.Logger.LogDebug($"Current Total Experience: {GetTotalExperience():N0}");
            //Jotunn.Logger.LogDebug($"Level: {_currentLevelEvaluation.Level:N0} | Total Experience: {totalExperience:N0} | Is Max Level: {_currentLevelEvaluation.IsMaxLevel}");
        }
    }
}