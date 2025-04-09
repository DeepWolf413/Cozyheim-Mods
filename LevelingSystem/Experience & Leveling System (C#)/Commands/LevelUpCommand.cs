using Cozyheim.LevelingSystem.Constants;
using Jotunn.Entities;

namespace Cozyheim.LevelingSystem.Commands
{
    public class LevelUpCommand : ConsoleCommand
    {
        public override string Name => "level_up";
        public override string Help => "Level up the specified player. Usage: level_up <playerName> <amount>";
        public override bool IsCheat => true;
        public override bool IsNetwork => true;

        public override void Run(string[] args)
        {
            if (args.Length < 2 || ZNet.instance.LocalPlayerIsAdminOrHost())
            {
                return;
            }
            
            string targetPlayerName = args[0];

            if (!int.TryParse(args[1], out int level) || level < 1)
            {
                Console.instance.Print($"invalid level number: {args[1]}");
                return;
            }

            // TODO: Implement level-up command.
            var newPackage = new ZPackage();
            newPackage.Write(targetPlayerName);
            newPackage.Write(level);
            ModRpcRegistry.Instance.SendServerRpc(RpcConstants.ServerSetLevel, newPackage);
        }
    }
}