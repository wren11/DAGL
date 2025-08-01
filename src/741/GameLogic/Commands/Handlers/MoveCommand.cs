using System;
using System.Linq;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class MoveCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: move <target> <x> <y> [speed]");
        }

        var targetName = args[0];
        if (!int.TryParse(args[1], out var x) || !int.TryParse(args[2], out var y))
        {
            throw new ArgumentException("Invalid coordinates");
        }

        var speed = args.Length > 3 && float.TryParse(args[3], out var s) ? s : 1.0f;

        var targetObject = FindObjectByName(context, targetName);

        if (targetObject == null)
        {
            throw new ArgumentException($"Target '{targetName}' not found");
        }

        if (!IsValidMapPosition(context, x, y))
        {
            throw new ArgumentException("Invalid position");
        }

        if (!context.MapTiles[context.CurrentMap][x, y].IsPassable)
        {
            throw new ArgumentException("Position is not passable");
        }

        targetObject.MoveTo(new Vector2(x, y));
        targetObject.SetState(WorldObjectState.Moving);
    }

    private WorldObject? FindObjectByName(CommandContext context, string name)
    {
        if (name.Equals("player", StringComparison.OrdinalIgnoreCase))
        {
            return context.CurrentPlayer;
        }

        return context.MapObjects[context.CurrentMap].FirstOrDefault(obj => 
                obj.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsValidMapPosition(CommandContext context, int x, int y)
    {
        if (!context.MapTiles.ContainsKey(context.CurrentMap)) return false;
        var tiles = context.MapTiles[context.CurrentMap];
        return x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1);
    }
}