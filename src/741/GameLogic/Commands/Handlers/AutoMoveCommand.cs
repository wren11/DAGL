using System;
using DarkAges.Library.Graphics;
using Vector2 = DarkAges.Library.Graphics.Vector2;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class AutoMoveCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: auto_move <x> <y> [pathfinding]");
        }

        if (!int.TryParse(args[0], out var x) || !int.TryParse(args[1], out var y))
        {
            throw new ArgumentException("Invalid coordinates");
        }

        var usePathfinding = args.Length > 2 && bool.TryParse(args[2], out var pf) && pf;

        if (context.CurrentPlayer == null)
        {
            throw new InvalidOperationException("No player character available");
        }

        if (!IsValidMapPosition(context, x, y))
        {
            throw new ArgumentException("Invalid destination");
        }

        var targetPos = new Vector2(x, y);
        var currentPos = context.CurrentPlayer.Position;
        var direction = Vector2.Normalize(targetPos - currentPos);
            
        context.CurrentPlayer.Velocity = direction * 50f; // Movement speed
        context.CurrentPlayer.SetState(World.WorldObjectState.Moving);
    }

    private bool IsValidMapPosition(CommandContext context, int x, int y)
    {
        if (!context.MapTiles.ContainsKey(context.CurrentMap)) return false;
        var tiles = context.MapTiles[context.CurrentMap];
        return x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1);
    }
}