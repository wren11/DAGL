using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class WaitEventCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("Usage: wait_event <eventType> [timeout]");
        }

        var eventType = args[0];
        var timeout = args.Length > 1 && float.TryParse(args[1], out var t) ? t : 30.0f;

        // This would integrate with the event system to pause execution
        // until the specified event occurs or timeout is reached
    }
}