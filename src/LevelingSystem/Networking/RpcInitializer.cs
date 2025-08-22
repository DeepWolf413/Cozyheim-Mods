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
            //ZRoutedRpc.instance.Register(rpcEntry.Value.RPCId, rpcEntry.Value.TargetFunction);
        }
    }
}