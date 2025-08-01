using System;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class PutItemCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: put_item <itemId> <x> <y> [quantity]");
        }

        if (!int.TryParse(args[0], out var itemId) ||
            !int.TryParse(args[1], out var x) ||
            !int.TryParse(args[2], out var y))
        {
            throw new ArgumentException("Invalid parameters");
        }

        var quantity = args.Length > 3 && int.TryParse(args[3], out var q) ? q : 1;

        if (!IsValidMapPosition(context, x, y))
        {
            throw new ArgumentException("Invalid map position");
        }

        var item = CreateItem(itemId, quantity);
        if (item != null)
        {
            var worldItem = new WorldObject_Item(item)
            {
                Position = new Vector2(x, y),
                MapId = GetMapId(context.CurrentMap)
            };

            AddWorldObject(context, worldItem);
        }
    }

    private bool IsValidMapPosition(CommandContext context, int x, int y)
    {
        if (!context.MapTiles.ContainsKey(context.CurrentMap)) return false;
        var tiles = context.MapTiles[context.CurrentMap];
        return x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1);
    }

    private Item? CreateItem(int itemId, int quantity = 1)
    {
        var item = new Item(itemId, $"Item_{itemId}", $"Description for Item_{itemId}")
        {
            Quantity = quantity,
            Value = itemId * 10,
            Weight = 1
        };
        return item;
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