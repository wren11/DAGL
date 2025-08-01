using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class MessageCommand : ICommand
{
    public event EventHandler<string>? MessageDisplayed;

    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: message <text>");
        }

        var message = string.Join(" ", args);
        DisplayMessage(message);
    }

    private void DisplayMessage(string message)
    {
        Console.WriteLine($"[Command] {message}");
        MessageDisplayed?.Invoke(this, message);
    }
}