using System;
using System.Linq;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class AutoUseSpellCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Usage: auto_use_spell <spellId> [target]");
        }

        if (!int.TryParse(args[0], out var spellId))
        {
            throw new ArgumentException("Invalid spell ID");
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

        if (!context.CurrentPlayer.CastSpell(spellId, target))
        {
            throw new InvalidOperationException($"Failed to cast spell {spellId}");
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