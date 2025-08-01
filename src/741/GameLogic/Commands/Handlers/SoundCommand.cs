using System;
using System.Numerics;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SoundCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: sound <soundId> [volume] [x] [y]");
        }

        if (!short.TryParse(args[0], out var soundId))
        {
            throw new ArgumentException("Invalid sound ID");
        }

        var volume = args.Length > 1 && float.TryParse(args[1], out var v) ? v : 1.0f;
        var x = args.Length > 2 && float.TryParse(args[2], out var px) ? px : 0f;
        var y = args.Length > 3 && float.TryParse(args[3], out var py) ? py : 0f;

        context.SoundManager.PlaySound(soundId, volume, new Vector2(x, y));
    }
}