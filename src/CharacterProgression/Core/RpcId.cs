namespace CharacterProgressionMod.Core
{
    public static class RpcId
    {
        private const string Prefix = PluginInfo.Guid + "!";

        public static string Generate(string rpcName) => $"{Prefix}{rpcName}";
    }
}