using System;
using DarkAges.Library.UI.MiniGame;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class PlayMinigameCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: play_minigame <gameId>");
        }

        if (!int.TryParse(args[0], out var gameId))
        {
            throw new ArgumentException("Invalid game ID");
        }

        var miniGameSystem = new MiniGameSystem();
        miniGameSystem.StartGame(gameId);
    }
}