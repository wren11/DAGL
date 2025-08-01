using System;
using System.Linq;
using System.Numerics;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class MotionCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: motion <target> <motionType> [speed]");
        }

        var targetName = args[0];
        var motionType = args[1];
        var speed = args.Length > 2 && float.TryParse(args[2], out var s) ? s : 1.0f;

        var targetObject = FindObjectByName(context, targetName);

        if (targetObject == null)
        {
            throw new ArgumentException($"Target '{targetName}' not found");
        }

        ApplyMotion(targetObject, motionType, speed);
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

    private void ApplyMotion(WorldObject target, string motionType, float speed)
    {
        switch (motionType.ToLower())
        {
        case "walk":
            target.SetState(WorldObjectState.Moving);
            break;
        case "run":
            target.SetState(WorldObjectState.Moving);
            target.Velocity *= 2.0f * speed;
            break;
        case "stop":
            target.SetState(WorldObjectState.Idle);
            target.Velocity = Vector2.Zero;
            break;
        default:
            throw new ArgumentException($"Unknown motion type: {motionType}");
        }
    }
}