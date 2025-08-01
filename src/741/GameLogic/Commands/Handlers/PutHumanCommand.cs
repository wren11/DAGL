using System;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class PutHumanCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 4)
        {
            throw new ArgumentException("Usage: put_human <name> <x> <y> <class>");
        }

        var name = args[0];
        if (!int.TryParse(args[1], out var x) || !int.TryParse(args[2], out var y))
        {
            throw new ArgumentException("Invalid coordinates");
        }

        var characterClass = args[3];

        if (!IsValidMapPosition(context, x, y))
        {
            throw new ArgumentException("Invalid map position");
        }

        var human = new WorldObject_Human
        {
            Name = name,
            Position = new Vector2(x, y),
            MapId = GetMapId(context.CurrentMap),
            ClassName = characterClass
        };

        AddWorldObject(context, human);
    }

    private bool IsValidMapPosition(CommandContext context, int x, int y)
    {
        if (!context.MapTiles.ContainsKey(context.CurrentMap)) return false;
        var tiles = context.MapTiles[context.CurrentMap];
        return x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1);
    }

    private int GetMapId(string mapName)
    {
        return mapName.GetHashCode() & 0x7FFFFFFF;
    }

    private void AddWorldObject(CommandContext context, WorldObject obj)
    {
        if (!context.MapObjects.ContainsKey(context.CurrentMap))
            context.MapObjects[context.CurrentMap] = [];
            
        context.MapObjects[context.CurrentMap].Add(obj);
    }
}