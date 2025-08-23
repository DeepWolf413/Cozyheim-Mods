using UnityEngine;
using Logger = Jotunn.Logger;

namespace DeepWolf.CharacterProgressionMod
{
    /// <summary>
    ///     Responsible for rewarding experience to damage dealers.
    /// </summary>
    public sealed class RewardExpOnDeath : MonoBehaviour
    {
        private Character _character;

        private void Start()
        {
            if (!TryGetComponent(out _character)) {
                return;
            }

            _character.m_onDeath += Character_OnDeath;
        }

        private void Character_OnDeath()
        {
            Logger.LogDebug($"Are we the owner of {_character.name}?: {_character.IsOwner()}");
            _character.m_onDeath -= Character_OnDeath;

            if (!TryGetComponent(out DamageRegistry damageRegistry)) {
                return;
            }

            foreach (var instigator in damageRegistry.Instigators) {
                var playerId = instigator.Key;
                var player = Player.GetPlayer(playerId);
                if (player == null) {
                    Logger.LogError($"Failed to give experience to player({playerId}) - player not found.");
                    continue;
                }

                if (!player.TryGetComponent(out PlayerExpPool expPool)) {
                    continue;
                }

                expPool.CreateKillExperienceReward(_character);
            }
        }
    }
}