using System;
using System.Drawing;

namespace DarkAges.Library.GameLogic.Commands.Handlers;

public class SetColorCommand : ICommand
{
    public void Execute(CommandContext context, string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: set_color <r> <g> <b> [target]");
        }

        if (!byte.TryParse(args[0], out var r) ||
            !byte.TryParse(args[1], out var g) ||
            !byte.TryParse(args[2], out var b))
        {
            throw new ArgumentException("Invalid color values (0-255)");
        }

        var color = Color.FromArgb(255, r, g, b);
        var target = args.Length > 3 ? args[3] : "background";

        switch (target.ToLower())
        {
        case "background":
            context.GraphicsDevice.FillRectangle(0, 0, context.GraphicsDevice.Width, context.GraphicsDevice.Height, color);
            break;
        case "player":
            if (context.CurrentPlayer != null)
            {
                // Apply color tint to player character
            }
            break;
        default:
            throw new ArgumentException($"Unknown color target: {target}");
        }
    }
}