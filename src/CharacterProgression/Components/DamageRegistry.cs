using System.Collections.Generic;
using UnityEngine;

namespace CharacterProgressionMod
{
    /// <summary>
    ///     Responsible for keeping track of damage dealt to the entity this component is attached to.
    /// </summary>
    /// <remarks>
    ///     Only player damage is tracked - damage is added by <see cref="Patches.DamageRecorderPatch" />.
    ///     <para>
    ///         The idea is to use the recorded damage to calculate the experience reward for players upon killing a
    ///         creature.
    ///     </para>
    /// </remarks>
    public sealed class DamageRegistry : MonoBehaviour
    {
        private readonly Dictionary<long, List<DamageEntry>> _instigators = new();

        public IReadOnlyDictionary<long, List<DamageEntry>> Instigators => _instigators;

        public int InstigatorCount => _instigators.Count;

        public void AddEntry(HitData hitData)
        {
            var attacker = hitData.GetAttacker();
            if (!hitData.HaveAttacker() || !attacker.IsPlayer()) {
                return;
            }

            var zdo = attacker.m_nview.GetZDO();
            var playerId = zdo.GetLong(ZDOVars.s_playerID);

            if (!_instigators.ContainsKey(playerId)) {
                _instigators[playerId] = new List<DamageEntry>();
            }

            _instigators[playerId].Add(new DamageEntry(playerId, hitData.m_damage, hitData.m_hitType));
        }

        public void ClearEntries()
        {
            _instigators.Clear();
        }

        public readonly struct DamageEntry
        {
            public DamageEntry(HitData hitData)
            {
                PlayerId = hitData.m_attacker.UserID;
                Damage = hitData.m_damage;
                HitType = hitData.m_hitType;
            }

            public DamageEntry(long playerId, HitData.DamageTypes damage, HitData.HitType hitType)
            {
                PlayerId = playerId;
                Damage = damage;
                HitType = hitType;
            }

            public long PlayerId { get; }
            public HitData.DamageTypes Damage { get; }
            public HitData.HitType HitType { get; }
        }
    }
}