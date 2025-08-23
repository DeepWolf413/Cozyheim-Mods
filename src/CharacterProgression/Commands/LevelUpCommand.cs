using Jotunn.Entities;

namespace CharacterProgressionMod.Commands
{
    public class LevelUpCommand : ConsoleCommand
    {
        public override string Name => "level_up";
        public override string Help => "Level up the specified player. Usage: level_up <playerName> <amount>";
        public override bool IsCheat => true;
        public override bool IsNetwork => true;

        public override void Run(string[] args)
        {
            if (args.Length < 2 || ZNet.instance.LocalPlayerIsAdminOrHost()) {
                return;
            }

            var targetPlayerName = args[0];

            if (!int.TryParse(args[1], out var level) || level < 1) {
                Console.instance.Print($"invalid level number: {args[1]}");
            }

            // TODO: Implement level-up command.
        }
    }
}