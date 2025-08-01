using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SendFieldmapCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: send_fieldmap <mapId> <x> <y>");
        }

        if (!ushort.TryParse(args[0], out var mapId) ||
            !ushort.TryParse(args[1], out var x) ||
            !ushort.TryParse(args[2], out var y))
        {
            throw new ArgumentException("Invalid parameters");
        }

        // This would send map information to the client/server
    }
}