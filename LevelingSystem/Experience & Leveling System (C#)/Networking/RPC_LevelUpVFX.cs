using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cozyheim.LevelingSystem
{
    internal class RPC_LevelUpVFX
    {
        public static CustomRPC levelUpVFX = null;
        public static CustomRPC spawnVFX = null;

        public static void SetupRPC()
        {
            levelUpVFX = NetworkManager.Instance.AddRPC("LevelUpVFX" + "_Server", LevelUpVFX_Server, LevelUpVFX_Server);
            spawnVFX = NetworkManager.Instance.AddRPC("SpawnVFX" + "_Server", SpawnVFX, SpawnVFX);
        }

        private static IEnumerator LevelUpVFX_Server(long sender, ZPackage package)
        {
            ConsoleLog.Print("LevelUpVFX RPC");
            if (!NetworkHandler.IsServer() || package == null) {
                yield break;
            }
            
            ConsoleLog.Print("LevelUpVFX RPC Server");

            long playerID = package.ReadLong();
            Player player = Player.GetPlayer(playerID);
            if (!player) {
                ConsoleLog.Print($"Couldn't find a player with id '{playerID}'. Cancelling level up vfx.",
                    LogType.Error);
                yield break;
            }

            ZPackage newPackage = new ZPackage();
            newPackage.Write(playerID);
            spawnVFX.SendPackage(ZRoutedRpc.Everybody, newPackage);

            ConsoleLog.Print("Player with ID " + playerID.ToString() + " (" + player.GetPlayerName() + ") leveled up!");
        }

        private static IEnumerator SpawnVFX(long sender, ZPackage package)
        {
            ConsoleLog.Print("SpawnVFX");
            if (UIManager.Instance != null)
            {
                ConsoleLog.Print("SpawnVFX !UIManager");
                UIManager.rpc_LevelUpEffect.SendPackage(ZRoutedRpc.Everybody, package);
            }
            yield return null;
        }
    }
}
