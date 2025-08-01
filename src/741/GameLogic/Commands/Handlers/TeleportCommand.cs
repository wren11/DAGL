using System;
using System.Linq;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class TeleportCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: teleport <map> <x> <y> [target]");
        }

        var mapName = args[0];
        if (!int.TryParse(args[1], out var x) || !int.TryParse(args[2], out var y))
        {
            throw new ArgumentException("Invalid coordinates");
        }

        var targetName = args.Length > 3 ? args[3] : "player";

        var targetObject = FindObjectByName(context, targetName);

        if (targetObject == null)
        {
            throw new ArgumentException($"Target '{targetName}' not found");
        }

        if (mapName != context.CurrentMap)
        {
            RemoveWorldObject(context, targetObject);
            context.CurrentMap = mapName;
            AddWorldObject(context, targetObject);
        }

        targetObject.Position = new Vector2(x, y);
        targetObject.MapId = GetMapId(mapName);
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

    private void RemoveWorldObject(CommandContext context, WorldObject obj)
    {
        if (context.MapObjects.ContainsKey(context.CurrentMap))
            context.MapObjects[context.CurrentMap].Remove(obj);
    }
}