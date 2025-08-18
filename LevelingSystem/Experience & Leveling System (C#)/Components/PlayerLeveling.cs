using Cozyheim.LevelingSystem.Utilities;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace Cozyheim.LevelingSystem
{
    public sealed class PlayerLeveling : MonoBehaviour
    {
        private const string LevelSaveKey = "Cozyheim.Level";
        private const string TotalExpSaveKey = "Cozyheim.TotalExperience";

        private Player _player;

        public static PlayerLeveling Local { get; private set; }
        private string RpcAddExperience { get; } = RpcId.Create("AddExperience");

        private void Awake()
        {
            _player = GetComponent<Player>();

            if (_player.m_nview.GetZDO() == null) {
                return;
            }

            _player.m_nview.Register<ExpReward>(RpcAddExperience, RPC_AddExperience);

            if (!_player.IsOwner()) {
                return;
            }

            Local = this;
            UpdateLevel();
        }

        public void CreateGenericExperienceReward(ExpSource sourceType, string sourceName, float multiplier = 1.0f)
        {
            XpTable xpTable = null;
            switch (sourceType) {
                case ExpSource.Woodcutting:
                    xpTable = XPManager.WoodcuttingXpTable;
                    break;
                case ExpSource.Mining:
                    xpTable = XPManager.MiningXpTable;
                    break;
                case ExpSource.Pickable:
                    xpTable = XPManager.PickablesXpTable;
                    break;
                default:
                    return;
            }

            var baseXpSpreadMin = Mathf.Min(1 - Main.ModConfig.BaseXpSpreadMin.Value / 100f, 1f);
            var baseXpSpreadMax = Mathf.Max(1 + Main.ModConfig.BaseXpSpreadMax.Value / 100f, 1f);
            var globalExpMultiplier = Mathf.Max(0f, Main.ModConfig.AllXpMultiplier.Value / 100f);
            var restedMultiplier = Mathf.Max(0f, Main.ModConfig.RestedXpMultiplier.Value / 100f);

            var experience = xpTable.GetXp(sourceName) * multiplier;
            if (experience <= 0) {
                Logger.LogDebug(
                    $"Failed to find a valid xp reward for client. xpMul: {globalExpMultiplier} | iType: {sourceType.ToString()} | iName: {sourceName}");
                return;
            }

            experience = (int)(experience * globalExpMultiplier * Random.Range(baseXpSpreadMin, baseXpSpreadMax));
            var restedBonusXp = experience * restedMultiplier;

            var expReward = new ExpReward(sourceType, (int)experience, 0, (int)restedBonusXp);

            if (_player.IsOwner()) {
                RPC_AddExperience(_player.GetPlayerID(), expReward);
                return;
            }

            RpcInvoke.OwnerRpc(_player.m_nview, RpcAddExperience, expReward);
        }

        public void CreateKillExperienceReward(Character source)
        {
            if (source == null) {
                Logger.LogError("Can't create kill experience reward without knowing the source character.");
                return;
            }

            var expPercentagePerInstigator = 1.0f;
            var creatureName = source.name;
            var creatureLevel = source.m_level;

            var baseXpSpreadMin = Mathf.Min(1 - Main.ModConfig.BaseXpSpreadMin.Value / 100f, 1f);
            var baseXpSpreadMax = Mathf.Max(1 + Main.ModConfig.BaseXpSpreadMax.Value / 100f, 1f);
            var creatureLevelMultiplier = Mathf.Max(0f, Main.ModConfig.MonsterLvlXpMultiplier.Value / 100f);
            var expMultiplier = Mathf.Max(0f, Main.ModConfig.AllXpMultiplier.Value / 100f);
            var restedMultiplier = Mathf.Max(0f, Main.ModConfig.RestedXpMultiplier.Value / 100f);

            var rndExpSpread = Random.Range(baseXpSpreadMin, baseXpSpreadMax);
            var expTable = XPManager.CreaturesXpTable;
            var baseExp = expTable.GetXp(creatureName) * expPercentagePerInstigator *
                          rndExpSpread * expMultiplier;
            var creatureLevelExpBonus = (creatureLevel - 1) * creatureLevelMultiplier * baseExp;
            var restedExpBonus = baseExp * restedMultiplier;
            var expReward = new ExpReward(ExpSource.CreatureKill, (int)baseExp, (int)creatureLevelExpBonus,
                                          (int)restedExpBonus);

            if (_player.IsOwner()) {
                RPC_AddExperience(_player.GetPlayerID(), expReward);
                return;
            }

            RpcInvoke.OwnerRpc(_player.m_nview, RpcAddExperience, expReward);
        }

        private void RPC_AddExperience(long sender, ExpReward expReward)
        {
            Logger.LogDebug($"Received exp reward from server. ExpReward: {expReward}");
            var statusEffects = _player.GetSEMan();
            var isRested = statusEffects.HaveStatusEffect(SEMan.s_statusEffectRested);
            var newTotalExperience = GetTotalExperience() + expReward.GetEligibleExp(isRested);
            SetTotalExperience(newTotalExperience);
            UpdateLevel();
        }

        /// <summary>
        ///     Sets the total experience to match the experience necessary to reach the specified level.
        /// </summary>
        /// <remarks>
        ///     The stored level is updated after the total experience is set.
        /// </remarks>
        /// <param name="level">The desired level.</param>
        private void SetLevel(int level)
        {
            var totalExperience = XPManager.PlayerXpTable.GetTotalExpForLevel(level);
            _player.m_customData[TotalExpSaveKey] = totalExperience.ToString();
            UpdateLevel();
        }

        private void SetTotalExperience(int totalExperience)
        {
            _player.m_customData[TotalExpSaveKey] = totalExperience.ToString();
            UpdateLevel();
        }

        public int GetTotalExperience()
        {
            if (_player.m_customData.TryGetValue(TotalExpSaveKey, out var totalExperienceString) &&
                int.TryParse(totalExperienceString, out var totalExperience)) {
                return totalExperience;
            }

            return 0;
        }

        public int GetLevel()
        {
            if (_player.m_customData.TryGetValue(LevelSaveKey, out var levelString) &&
                int.TryParse(levelString, out var level)) {
                return level;
            }

            return 0;
        }

        /// <summary>
        ///     Calculates the level of the player based on the total experience.
        /// </summary>
        /// <remarks>
        ///     Should only be called by the owner of the player.
        /// </remarks>
        private void UpdateLevel()
        {
            var currentLevel = GetLevel();
            XPManager.PlayerXpTable.GetLevelFromTotalExp(currentLevel);

            var newLevel = 1;
            _player.m_customData[LevelSaveKey] = newLevel.ToString();
        }
    }
}