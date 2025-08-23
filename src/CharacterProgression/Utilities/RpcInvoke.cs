using Jotunn;

namespace DeepWolf.CharacterProgressionMod.Utilities
{
    internal static class RpcInvoke
    {
        public static void GlobalServerRpc(string rpcId, params object[] parameters)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), rpcId, parameters);
        }

        public static void GlobalTargetRpc(long targetPeerId, string rpcId, params object[] parameters)
        {
            if (targetPeerId == ZRoutedRpc.Everybody) {
                Logger.LogDebug($"Sending rpc '{rpcId}' to everybody");
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, rpcId, parameters);
                return;
            }

            Logger.LogDebug($"Sending rpc '{rpcId}' to '{targetPeerId}'");
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerId, rpcId, parameters);
        }

        public static void OwnerRpc(ZNetView nview, string rpcId, params object[] parameters)
        {
            nview.InvokeRPC(rpcId, parameters);
        }

        public static void TargetRpc(ZNetView nview, long targetPeerId, string rpcId, params object[] parameters)
        {
            if (targetPeerId == ZRoutedRpc.Everybody) {
                Logger.LogDebug($"Sending rpc '{rpcId}' to everybody");
                nview.InvokeRPC(ZRoutedRpc.Everybody, rpcId, parameters);
                return;
            }

            nview.InvokeRPC(targetPeerId, rpcId, parameters);
        }
    }
}