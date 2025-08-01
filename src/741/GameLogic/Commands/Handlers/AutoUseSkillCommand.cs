using System;
using System.Linq;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class AutoUseSkillCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: auto_use_skill <skillId> [target]");
        }

        if (!int.TryParse(args[0], out var skillId))
        {
            throw new ArgumentException("Invalid skill ID");
        }

        if (context.CurrentPlayer == null)
        {
            throw new InvalidOperationException("No player character available");
        }

        WorldObject_Living? target = null;
        if (args.Length > 1)
        {
            target = FindObjectByName(context, args[1]) as WorldObject_Living;
        }

        if (!context.CurrentPlayer.UseSkill(skillId, target))
        {
            throw new InvalidOperationException($"Failed to use skill {skillId}");
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