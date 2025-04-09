using HarmonyLib;

namespace Cozyheim.LevelingSystem
{
    [HarmonyPatch]
    internal static class RpcInitializer
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static void GameStart()
        {
            var rpcRegistries = ModRpcRegistry.Instance.GetAllRegistries();
            foreach (var rpcRegistry in rpcRegistries)
            {
                rpcRegistry.PushToGame();
            }
        }
    }
}