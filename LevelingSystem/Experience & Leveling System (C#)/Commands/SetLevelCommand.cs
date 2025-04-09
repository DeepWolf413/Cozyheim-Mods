using Cozyheim.LevelingSystem.Constants;
using Jotunn.Entities;

namespace Cozyheim.LevelingSystem.Commands
{
    public class SetLevelCommand : ConsoleCommand
    {
        public override string Name => "set_level";
        public override string Help => "Sets the level of the specified player. Usage: set_level <playerName> <level>";
        public override bool IsCheat => true;
        public override bool IsNetwork => true;

        public override void Run(string[] args)
        {
            if (args.Length < 2 || ZNet.instance.LocalPlayerIsAdminOrHost())
            {
                return;
            }
            
            string targetPlayerName = args[0];

            if (!int.TryParse(args[1], out int level))
            {
                Console.instance.Print($"invalid level number: {args[1]}");
                return;
            }

            var newPackage = new ZPackage();
            newPackage.Write(targetPlayerName);
            newPackage.Write(level);
            ModRpcRegistry.Instance.SendServerRpc(RpcConstants.ServerSetLevel, newPackage);
        }
    }
}