using System;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SpawnCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 4)
        {
            throw new ArgumentException("Usage: spawn <type> <id> <x> <y> [parameters]");
        }

        var type = args[0];
        if (!int.TryParse(args[1], out var id) ||
            !int.TryParse(args[2], out var x) ||
            !int.TryParse(args[3], out var y))
        {
            throw new ArgumentException("Invalid parameters");
        }

        switch (type.ToLower())
        {
        case "monster":
            new PutMonsterCommand().Execute(context, [id.ToString(), x.ToString(), y.ToString()]);
            break;
        case "item":
            new PutItemCommand().Execute(context, [id.ToString(), x.ToString(), y.ToString()]);
            break;
        case "npc":
            var npc = CreateNPC(id);
            if (npc != null)
            {
                npc.Position = new System.Numerics.Vector2(x, y);
                AddWorldObject(context, npc);
            }
            break;
        default:
            throw new ArgumentException($"Unknown spawn type: {type}");
        }
    }

    private World.WorldObject_NPC? CreateNPC(int npcId)
    {
        var npc = new World.WorldObject_NPC
        {
            Name = $"NPC_{npcId}",
            Level = 1,
            IsHostile = false,
            IsMerchant = npcId % 3 == 0
        };
        return npc;
    }

    private void AddWorldObject(CommandContext context, World.WorldObject obj)
    {
        if (!context.MapObjects.ContainsKey(context.CurrentMap))
            context.MapObjects[context.CurrentMap] = [];
            
        context.MapObjects[context.CurrentMap].Add(obj);
    }
}