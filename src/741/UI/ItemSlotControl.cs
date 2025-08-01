using DarkAges.Library.Graphics;
using System.Numerics;
using DarkAges.Library.GameLogic;
using DarkAges.Library.Core.Events;
using System.Drawing;

namespace DarkAges.Library.UI;

public class ItemSlotControl : ControlPane
{
    public Item? Item { get; set; }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Render background slot
        // Render item icon if Item is not null
        if (Item?.Icon != null)
        {
            var destRect = new Rectangle(Position.X, Position.Y, Size.Width, Size.Height);
            spriteBatch.Draw(Item.Icon, destRect, ColorRgb565.White);
        }
    }

    public override bool HandleInput(Event e)
    {
        // Handle clicks for item details, etc.
        return false;
    }
}