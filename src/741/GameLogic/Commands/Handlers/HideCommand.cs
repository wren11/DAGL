using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class HideCommand : ICommand
{
    public event EventHandler<string>? MessageDisplayed;

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: hide <element>");
        }

        var element = args[0];
        DisplayMessage($"Hiding {element}");
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine($"[Command] {message}");
        MessageDisplayed?.Invoke(this, message);
    }
}