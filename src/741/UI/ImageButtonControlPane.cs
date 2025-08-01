using System.Drawing;
using DarkAges.Library.Graphics;

namespace DarkAges.Library.UI;

public class ImageButtonControlPane(IndexedImage normal, IndexedImage pressed, IndexedImage hover) : ButtonControlPane
{
    public override void Render(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        var imageToRender = State switch
        {
            1 => hover,
            2 => pressed,
            _ => normal,
        };

        // Render the appropriate image
        if (imageToRender != null)
        {
            spriteBatch.Draw(imageToRender, new DarkAges.Library.Graphics.Vector2(Bounds.X, Bounds.Y), Color.White);
        }
            
        base.Render(spriteBatch);
    }
}