using System;
using System.Linq;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SetTimerCommand(CommandDispatcher dispatcher) : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: set_timer <seconds> <command>");
        }

        if (!float.TryParse(args[0], out var seconds))
        {
            throw new ArgumentException("Invalid time value");
        }

        var command = string.Join(" ", args.Skip(1));

        context.TimerManager.AddEvent("TimerCommand", seconds, () => dispatcher.Dispatch(command), false);
    }
}