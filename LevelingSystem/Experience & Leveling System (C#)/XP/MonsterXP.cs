using System.Collections.Generic;

namespace Cozyheim.LevelingSystem
{
    internal class MonsterXP
    {
        public uint monsterID;
        public List<PlayerDamage> playerDamages;

        public MonsterXP(uint monsterID)
        {
            this.monsterID = monsterID;
            playerDamages = new List<PlayerDamage>();
        }

        public void AddDamage(long playerID, float damage, string playerName = "")
        {
            foreach (var dmg in playerDamages) {
                if (dmg.playerID == playerID) {
                    dmg.playerTotalDamage += damage;
                    return;
                }
            }

            playerDamages.Add(new PlayerDamage
                                  { playerID = playerID, playerTotalDamage = damage, playerName = playerName });
        }

        public float GetTotalDamageDealt()
        {
            var totalDamage = 0f;
            foreach (var dmg in playerDamages) {
                totalDamage += dmg.playerTotalDamage;
            }

            return totalDamage;
        }
    }

    internal class PlayerDamage
    {
        public long playerID;
        public string playerName;
        public float playerTotalDamage;
    }
}