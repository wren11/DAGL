using System;
using DarkAges.Library.Core;
using Monitor = DarkAges.Library.Core.Monitor;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SetAttrTrackerCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: set_attr_tracker <attribute> <threshold>");
        }

        var attribute = args[0];
        if (!int.TryParse(args[1], out var threshold))
        {
            throw new ArgumentException("Invalid threshold value");
        }

        var monitor = new Monitor();
            
        switch (attribute.ToLower())
        {
        case "health":
        case "hp":
            if (context.CurrentPlayer != null)
            {
                var condition = new HealthMonitorCondition(() => context.CurrentPlayer.Health, () => context.CurrentPlayer.MaxHealth, threshold / 100.0f);
                monitor.AddCondition(condition);
            }
            break;
                    
        case "mana":
        case "mp":
            if (context.CurrentPlayer != null)
            {
                var condition = new ManaMonitorCondition(() => context.CurrentPlayer.Mana, () => context.CurrentPlayer.MaxMana, threshold / 100.0f);
                monitor.AddCondition(condition);
            }
            break;
                    
        default:
            throw new ArgumentException($"Unknown attribute: {attribute}");
        }
    }
}