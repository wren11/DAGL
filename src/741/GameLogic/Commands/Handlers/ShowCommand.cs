using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class ShowCommand : ICommand
{
    public event EventHandler<string>? MessageDisplayed;

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: show <element> [parameters]");
        }

        var element = args[0];
            
        switch (element.ToLower())
        {
        case "inventory":
            DisplayMessage("Showing inventory");
            break;
        case "stats":
            ShowPlayerStats(context);
            break;
        case "map":
            DisplayMessage($"Current map: {context.CurrentMap}");
            break;
        default:
            throw new ArgumentException($"Unknown element: {element}");
        }
    }

    private void ShowPlayerStats(CommandContext context)
    {
        if (context.CurrentPlayer == null)
        {
            DisplayMessage("No player character available");
            return;
        }

        var stats = context.CurrentPlayer.GetStatus();
        DisplayMessage("=== Player Stats ===");
        foreach (var stat in stats)
        {
            DisplayMessage($"{stat.Key}: {stat.Value}");
        }
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine($"[Command] {message}");
        MessageDisplayed?.Invoke(this, message);
    }
}