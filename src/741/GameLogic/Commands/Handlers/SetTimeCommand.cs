using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SetTimeCommand : ICommand
{
    public event EventHandler<string>? MessageDisplayed;

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("Usage: time <hour> [minute]");
        }

        if (!int.TryParse(args[0], out var hour))
        {
            throw new ArgumentException("Invalid hour");
        }

        var minute = args.Length > 1 && int.TryParse(args[1], out var m) ? m : 0;

        if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
        {
            throw new ArgumentException("Invalid time values");
        }

        DisplayMessage($"Set time to {hour:D2}:{minute:D2}");
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine($"[Command] {message}");
        MessageDisplayed?.Invoke(this, message);
    }
}