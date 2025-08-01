using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SetGndTileCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: set_gnd_tile <x> <y> <tileId>");
        }

        if (!int.TryParse(args[0], out var x) || 
            !int.TryParse(args[1], out var y) || 
            !int.TryParse(args[2], out var tileId))
        {
            throw new ArgumentException("Invalid parameters");
        }

        if (!IsValidMapPosition(context, x, y))
        {
            throw new ArgumentException("Invalid map position");
        }

        context.MapTiles[context.CurrentMap][x, y].GroundTileId = tileId;
        context.MapTiles[context.CurrentMap][x, y].IsPassable = GetTilePassability(tileId);
    }

    private bool IsValidMapPosition(CommandContext context, int x, int y)
    {
        if (!context.MapTiles.ContainsKey(context.CurrentMap)) return false;
        var tiles = context.MapTiles[context.CurrentMap];
        return x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1);
    }

    private bool GetTilePassability(int tileId)
    {
        // Simple passability check - water tiles (100+) are not passable
        return tileId < 100;
    }
}