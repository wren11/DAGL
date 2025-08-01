using System;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class PutMonsterCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: put_monster <monsterId> <x> <y> [level]");
        }

        if (!int.TryParse(args[0], out var monsterId) ||
            !int.TryParse(args[1], out var x) ||
            !int.TryParse(args[2], out var y))
        {
            throw new ArgumentException("Invalid parameters");
        }

        var level = args.Length > 3 && int.TryParse(args[3], out var l) ? l : 1;

        if (!IsValidMapPosition(context, x, y))
        {
            throw new ArgumentException("Invalid map position");
        }

        var monster = CreateMonster(monsterId, level);
        if (monster != null)
        {
            monster.Position = new Vector2(x, y);
            monster.MapId = GetMapId(context.CurrentMap);
            AddWorldObject(context, monster);
        }
    }

    private bool IsValidMapPosition(CommandContext context, int x, int y)
    {
        if (!context.MapTiles.ContainsKey(context.CurrentMap)) return false;
        var tiles = context.MapTiles[context.CurrentMap];
        return x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1);
    }

    private WorldObject_NPC? CreateMonster(int monsterId, int level)
    {
        var monster = new WorldObject_NPC
        {
            Name = $"Monster_{monsterId}",
            Level = level,
            MaxHealth = level * 20,
            Health = level * 20,
            Strength = level * 2,
            IsHostile = true
        };
        return monster;
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