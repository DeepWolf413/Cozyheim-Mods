namespace DeepWolf.CharacterProgressionMod.Utilities
{
    internal static class RpcId
    {
        public static string Create(string rpcName)
        {
            return Main.Guid + "!" + rpcName;
        }
    }
}