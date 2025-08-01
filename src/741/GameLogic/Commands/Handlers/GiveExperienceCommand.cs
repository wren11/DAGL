using System;
using System.Linq;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class GiveExperienceCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("Usage: give_experience <amount> [target]");
        }

        if (!int.TryParse(args[0], out var amount))
        {
            throw new ArgumentException("Invalid experience amount");
        }

        var targetName = args.Length > 1 ? args[1] : "player";

        if (targetName.Equals("player", StringComparison.OrdinalIgnoreCase))
        {
            if (context.CurrentPlayer != null)
            {
                context.CurrentPlayer.AddExperience(amount);
            }
        }
        else
        {
            var targetObject = FindObjectByName(context, targetName) as WorldObject_Human;
            if (targetObject != null)
            {
                targetObject.AddExperience(amount);
            }
            else
            {
                throw new ArgumentException($"Target '{targetName}' not found");
            }
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
}