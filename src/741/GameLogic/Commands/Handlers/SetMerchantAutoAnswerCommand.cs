using System;
using System.Linq;
using DarkAges.Library.World;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SetMerchantAutoAnswerCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: set_merchant_auto_answer <merchantName> <response>");
        }

        var merchantName = args[0];
        var response = string.Join(" ", args.Skip(1));

        var merchant = FindObjectByName(context, merchantName) as WorldObject_NPC;
        if (merchant == null)
        {
            throw new ArgumentException($"Merchant '{merchantName}' not found");
        }

        merchant.SetDialog("auto_response", response);
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