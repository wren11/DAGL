using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class WeatherCommand : ICommand
{
    public event EventHandler<string>? MessageDisplayed;

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: weather <type> [intensity]");
        }

        var weatherType = args[0];
        var intensity = args.Length > 1 && float.TryParse(args[1], out var i) ? i : 0.5f;

        DisplayMessage($"Set weather to {weatherType} with intensity {intensity}");
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine($"[Command] {message}");
        MessageDisplayed?.Invoke(this, message);
    }
}