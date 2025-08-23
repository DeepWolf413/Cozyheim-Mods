using CharacterProgressionMod.Core;

namespace CharacterProgressionMod.Utilities
{
    internal static class RpcId
    {
        public static string Create(string rpcName)
        {
            return ModInfo.Guid + "!" + rpcName;
        }
    }
}