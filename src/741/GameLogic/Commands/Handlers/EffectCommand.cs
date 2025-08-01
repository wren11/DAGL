using System;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class EffectCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("Usage: effect <effectId> [x] [y] [duration]");
        }

        if (!int.TryParse(args[0], out var effectId))
        {
            throw new ArgumentException("Invalid effect ID");
        }

        var x = args.Length > 1 && int.TryParse(args[1], out var px) ? px : (context.CurrentPlayer?.Position.X ?? 0);
        var y = args.Length > 2 && int.TryParse(args[2], out var py) ? py : (context.CurrentPlayer?.Position.Y ?? 0);
        var duration = args.Length > 3 && float.TryParse(args[3], out var d) ? d : 3.0f;

        var effect = CreateVisualEffect(effectId, new Vector2(x, y), duration, context);
        if (effect != null)
        {
            AddWorldObject(context, effect);
        }
    }

    private WorldObject? CreateVisualEffect(int effectId, Vector2 position, float duration, CommandContext context)
    {
        var effect = new WorldObject(WorldObjectType.Effect)
        {
            Name = $"Effect_{effectId}",
            Position = position
        };
            
        context.TimerManager.AddEvent($"Effect_{effectId}", duration, () => RemoveWorldObject(context, effect), false);
            
        return effect;
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