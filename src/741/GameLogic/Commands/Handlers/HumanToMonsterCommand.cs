using System;
using System.Linq;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class HumanToMonsterCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: human_to_monster <humanName> <monsterId>");
        }

        var humanName = args[0];
        if (!int.TryParse(args[1], out var monsterId))
        {
            throw new ArgumentException("Invalid monster ID");
        }

        var human = FindObjectByName(context, humanName) as WorldObject_Human;
        if (human == null)
        {
            throw new ArgumentException($"Human '{humanName}' not found");
        }

        var monster = CreateMonster(monsterId, human.Level);
        if (monster != null)
        {
            monster.Position = human.Position;
            monster.MapId = human.MapId;
            monster.Name = human.Name + " (Transformed)";

            RemoveWorldObject(context, human);
            AddWorldObject(context, monster);
        }
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